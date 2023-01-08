using Assets.DOTS.Components;
using System.Collections;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Assets.DOTS.Utility.Queue;
using Unity.Mathematics;
using Assets.DOTS.Utility.Stack;
using Unity.Transforms;
using Assets.DOTS.Components.Train;
using DOTS.Components;
using Unity.Entities.UniversalDelegates;

namespace Assets.DOTS.Systems
{
    [UpdateAfter(typeof(PlatformNavJob))]
    public partial struct QueueSystem : ISystem
    {
        BufferLookup<QueueEntryComponent> queueEntires;
        private EntityQuery carriageIDQuery;
        private EntityQuery queuerQuery;

        public void OnCreate(ref SystemState state)
        {
            queueEntires = state.GetBufferLookup<QueueEntryComponent>();
            carriageIDQuery =
            new EntityQueryBuilder(Allocator.Persistent).WithAll<CarriageIDComponent>().Build(ref state);
            queuerQuery =
            new EntityQueryBuilder(Allocator.Persistent).WithAll<CommuterQueuerComponent>().Build(ref state);
        }

        public void OnDestroy(ref SystemState state)
        {
            
        }

        public void OnUpdate(ref SystemState state)
        {
            if (SystemAPI.Time.ElapsedTime < 2.5f)
                return;

            //Enty queue
            queueEntires.Update(ref state);
            var entryJob = new QueueEntryJob { buffer = queueEntires };
            state.Dependency = entryJob.Schedule(state.Dependency);
            state.Dependency.Complete();

            // Queue job
            queueEntires.Update(ref state);
            ComponentLookup<TrainStateComponent> trainStateComponents = state.GetComponentLookup<TrainStateComponent>();
            trainStateComponents.Update(ref state);
            ComponentLookup<CommuterQueuerComponent> queuerComponents = state.GetComponentLookup<CommuterQueuerComponent>();
            queuerComponents.Update(ref state);
            ComponentLookup<WorldTransform> worldTransforms = state.GetComponentLookup<WorldTransform>();
            trainStateComponents.Update(ref state);
            EntityCommandBuffer ECB = new EntityCommandBuffer(Allocator.Persistent);
            var queueJob = new QueueJob { 
                buffer = queueEntires, 
                ECB = ECB,
                trainStateComponents = trainStateComponents,
                queuerComponents = queuerComponents,
                worldTransforms = worldTransforms,
            };
            state.Dependency = queueJob.Schedule(state.Dependency);
            state.Dependency.Complete();
            //var queueJobHandle = queueJob.Schedule(state.Dependency);
            //state.Dependency.Complete();
            ECB.Playback(state.EntityManager);
            ECB.Dispose();

            // CommuterQueuer job
            ECB = new EntityCommandBuffer(Allocator.Persistent);
            ComponentLookup<PlatformComponent> platformComponent = state.GetComponentLookup<PlatformComponent>();
            platformComponent.Update(ref state);
            worldTransforms = state.GetComponentLookup<WorldTransform>();
            worldTransforms.Update(ref state);
            ComponentLookup<CarriagePassengerSeatsComponent> seatsComponent = state.GetComponentLookup<CarriagePassengerSeatsComponent>();
            seatsComponent.Update(ref state);
            ComponentLookup<CarriageSeatComponent> seatComponents = state.GetComponentLookup<CarriageSeatComponent>();
            seatsComponent.Update(ref state);
            ComponentLookup<CarriageNavPointsComponent> carriageNavPoints = state.GetComponentLookup<CarriageNavPointsComponent>();
            carriageNavPoints.Update(ref state);
            ComponentLookup<CarriageIDComponent> carriageIDComponents = state.GetComponentLookup<CarriageIDComponent>();
            carriageIDComponents.Update(ref state);
            ComponentLookup<TrainIDComponent> trainIDComponents = state.GetComponentLookup<TrainIDComponent>();
            carriageIDComponents.Update(ref state);

            var carriageIDEntities =
            carriageIDQuery.ToEntityArray(Allocator.Persistent);

            var queuerJob = new CommuterQueueJob {
                ECB = ECB,
                platformComponent = platformComponent,
                carriageIDComponents = carriageIDComponents,
                trainIDComponents = trainIDComponents,
                seatComponents = seatComponents,
                seatsComponent = seatsComponent,
                worldTransforms = worldTransforms,
                carriageIDEntities = carriageIDEntities,
                EM = state.EntityManager,
            };
            state.Dependency = queuerJob.Schedule(state.Dependency);
            state.Dependency.Complete();

            ECB.Playback(state.EntityManager);
            ECB.Dispose();
        }
    }

    public partial struct QueueEntryJob : IJobEntity
    {
        public BufferLookup<QueueEntryComponent> buffer;

        public void Execute(in Entity entity, in CommuterComponent commuter, ref CommuterQueuerComponent queuer)
        {
            if (commuter.tasks.IsEmpty)
                return;

            if (commuter.tasks.NextStackElement().state != CommuterState.QUEUE)
                return;

            if (queuer.state != QueueState.None)
                return;

            buffer[commuter.currentPlatform].Add(new QueueEntryComponent { val = entity });
            queuer.state = QueueState.Entry;
        }
    }

    public partial struct QueueJob : IJobEntity
    {
        public BufferLookup<QueueEntryComponent> buffer;
        public EntityCommandBuffer ECB;
        public ComponentLookup<TrainStateComponent> trainStateComponents;
        public ComponentLookup<CommuterQueuerComponent> queuerComponents;
        public ComponentLookup<WorldTransform> worldTransforms;

        public void Execute(in Entity entity, in PlatformComponent platform, 
            ref QueueComponent queueC)
        {
            if (!buffer[entity].IsEmpty)
            {
                var list = SmallestQueue(queueC);
                list.Enqueue(buffer[entity][0].val);
                buffer[entity].RemoveAt(0);
            }

            TrainStateDOTS trainState = TrainStateDOTS.ARRIVING;

            if (platform.currentTrain != Entity.Null)
            {
                trainState = trainStateComponents[platform.currentTrain].value;
            }

            // Update existing queues
            UpdateCommuters(ECB, queueC.queue0, 
                queueC.queuePoint0, queueC.queuePoint0_1, trainState, worldTransforms, 0);
            UpdateCommuters(ECB, queueC.queue1,
                queueC.queuePoint1, queueC.queuePoint1_1, trainState, worldTransforms, 1);
            UpdateCommuters(ECB, queueC.queue2,
                queueC.queuePoint2, queueC.queuePoint2_1, trainState, worldTransforms, 2);
            UpdateCommuters(ECB, queueC.queue3,
                queueC.queuePoint3, queueC.queuePoint3_1, trainState, worldTransforms, 3);
            UpdateCommuters(ECB, queueC.queue4,
                queueC.queuePoint4, queueC.queuePoint4_1, trainState, worldTransforms, 4);

            // See if train is boarding
            DequeueQueuers(queueC.queue0, queuerComponents);
            DequeueQueuers(queueC.queue1, queuerComponents);
            DequeueQueuers(queueC.queue2, queuerComponents);
            DequeueQueuers(queueC.queue3, queuerComponents);
            DequeueQueuers(queueC.queue4, queuerComponents);
        }

        public void UpdateCommuters(EntityCommandBuffer ECB, 
            NativeList<Entity> queue, float3 queuePos0, float3 queuePos1, TrainStateDOTS trainState, 
            ComponentLookup<WorldTransform> worldTransforms, int queueIndex)
        {
            for (int i = 0; i < queue.Length; i++)
            {
                QueueState state = QueueState.InQueue;

                if (i == 0 && trainState == TrainStateDOTS.LOADING && 
                    math.distance(worldTransforms[queue[i]].Position, queuePos0) < 0.1f)
                {
                    // Must be close to the queue start
                    state = QueueState.ReadyForBoarding;
                }

                ECB.SetComponent<CommuterQueuerComponent>(queue[i], new CommuterQueuerComponent
                {
                    inQueue = true,
                    lineIndex = i,
                    queueIndex = queueIndex,
                    readyForBoarding = false,
                    finishedTask = false,
                    queueStartPosition = queuePos0,
                    queueDirection = queuePos1 - queuePos0,
                    state = state,
                });
            }
        }

        public void DequeueQueuers(NativeList<Entity> queue, ComponentLookup<CommuterQueuerComponent> queuerComponents)
        {
            if (queue.IsEmpty)
                return; 

            if (queuerComponents[queue.NextQueueElement()].state == QueueState.Boarding)
            {
                queue.Dequeue();
            }
        }

        public NativeList<Entity> SmallestQueue(QueueComponent queueC)
        {
            int min = int.MaxValue;
            NativeList<Entity> retval = new NativeList<Entity>();

            if (queueC.queue0.Length < min)
            {
                min = queueC.queue0.Length;
                retval = queueC.queue0;
            }
            if (queueC.queue1.Length < min)
            {
                min = queueC.queue1.Length;
                retval = queueC.queue1;
            }
            if (queueC.queue2.Length < min)
            {
                min = queueC.queue2.Length;
                retval = queueC.queue2;
            }
            if (queueC.queue3.Length < min)
            {
                min = queueC.queue3.Length;
                retval = queueC.queue3;
            }
            if (queueC.queue4.Length < min)
            {
                min = queueC.queue4.Length;
                retval = queueC.queue4;
            }
            return retval;
        }
    }

    public partial struct CommuterQueueJob : IJobEntity
    {
        public EntityCommandBuffer ECB;
        public ComponentLookup<PlatformComponent> platformComponent;
        public ComponentLookup<CarriageIDComponent> carriageIDComponents;
        public ComponentLookup<TrainIDComponent> trainIDComponents;
        public ComponentLookup<CarriageSeatComponent> seatComponents;
        public ComponentLookup<CarriagePassengerSeatsComponent> seatsComponent;
        public ComponentLookup<WorldTransform> worldTransforms;
        public NativeArray<Entity> carriageIDEntities;
        public EntityManager EM;

        public void Execute(in Entity entity, ref CommuterQueuerComponent queuer, ref WalkComponent walker, 
            in CommuterComponent commuter, ref PassengerComponent passenger)
        {

            if (commuter.tasks.IsEmpty)
                return;

            if (commuter.tasks.NextStackElement().state != CommuterState.QUEUE)
                return;

            switch (queuer.state)
            {
                case QueueState.InQueue:
                    if (!walker.destinations.IsEmpty)
                    {
                        walker.destinations.Clear();
                    }

                    if (walker.destinations.IsEmpty)
                    {
                        float3 target = queuer.queueStartPosition + queuer.queueDirection * queuer.lineIndex * 0.5f;

                        if (math.distance(target, worldTransforms[entity].Position) > 0.1f)
                            walker.destinations.Push(target);
                    }
                    break;
                case QueueState.ReadyForBoarding:
                    Entity platform = commuter.currentPlatform;
                    Entity train = platformComponent[platform].currentTrain;
                    TrainIDComponent trainC = trainIDComponents[train];
                    Entity carriage = GetCarriageFromTrain(trainC.LineIndex, trainC.TrainIndex, queuer.queueIndex, carriageIDComponents, carriageIDEntities);
                    CarriagePassengerSeatsComponent seatsCollection = seatsComponent[carriage];
                    NativeList<Entity> seats = seatsCollection.seats;
                    Debug.Log($"{EM.GetName(entity)} is going to train {EM.GetName(train)} on platform {EM.GetName(commuter.currentPlatform)}");
                    for (int j = 0; j < seats.Length; j++)
                    {
                        CarriageSeatComponent seatC = seatComponents.GetRefRW(seats[j], false).ValueRW;
                        if (seatC.available)
                        {
                            // STUFF
                            seatC.available = false;
                            passenger.currentCarriage = carriage;
                            passenger.carriageSeat = seats[j];
                            queuer.state = QueueState.Boarding;
                            break;
                        }
                        ECB.SetComponent(seats[j], seatC);
                    }
                    break;
                case QueueState.Boarding:
                    break;
            }
        }

        public Entity GetCarriageFromTrain(int lineIndex, int trainIndex, int carriageIndex,
        ComponentLookup<CarriageIDComponent> components, NativeArray<Entity> carriageEntities)
        {
            foreach (Entity item in carriageEntities)
            {
                if (components[item].trainIndex == trainIndex && 
                    components[item].id == carriageIndex && 
                    components[item].lineIndex == lineIndex)
                {
                    return item;
                }
            }
            throw new System.Exception("Could not find the specific carriage.");
        }
    }
}