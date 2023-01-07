using Assets.DOTS.Components;
using System.Collections;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Assets.DOTS.Utility.Queue;
using Unity.Mathematics;
using Assets.DOTS.Utility.Stack;
using Unity.Transforms;

namespace Assets.DOTS.Systems
{
    [UpdateAfter(typeof(PlatformNavJob))]
    public partial struct QueueSystem : ISystem
    {
        BufferLookup<QueueEntryComponent> queueEntires;

        public void OnCreate(ref SystemState state)
        {
            queueEntires = state.GetBufferLookup<QueueEntryComponent>();
        }

        public void OnDestroy(ref SystemState state)
        {
            
        }

        public void OnUpdate(ref SystemState state)
        {
            if (SystemAPI.Time.ElapsedTime < 2.5f)
                return;

            queueEntires.Update(ref state);

            var entryJob = new QueueEntryJob { buffer = queueEntires };
            state.Dependency = entryJob.Schedule(state.Dependency);
            state.Dependency.Complete();

            queueEntires.Update(ref state);
            EntityCommandBuffer ECB = new EntityCommandBuffer(Allocator.Persistent);
            var queueJob = new QueueJob { buffer = queueEntires, ECB = ECB };
            state.Dependency = queueJob.Schedule(state.Dependency);
            state.Dependency.Complete();
            ECB.Playback(state.EntityManager);
            ECB.Dispose();

            var queuerJob = new CommuterQueueJob {  };
            state.Dependency = queuerJob.Schedule(state.Dependency);
            state.Dependency.Complete();
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

            Debug.Log($"Length of queue entires: {buffer[commuter.currentPlatform].Length}");
        }
    }

    public partial struct QueueJob : IJobEntity
    {
        public BufferLookup<QueueEntryComponent> buffer;
        public EntityCommandBuffer ECB;

        public void Execute(in Entity entity, in PlatformComponent platform, 
            in QueueComponent queueC)
        {
            // Find a queue for entrying commuters
            //foreach (var item in buffer[entity])
            //{
            //    var list = SmallestQueue(queueC);

            //    list.Enqueue(item.val);
            //}

            if (buffer[entity].IsEmpty)
                return;

            var list = SmallestQueue(queueC);
            list.Enqueue(buffer[entity][0].val);
            buffer[entity].RemoveAt(0);

            // Update existing queues
            UpdateCommuters(ECB, queueC.queue0, 
                queueC.queuePoint0, queueC.queuePoint0_1);
            UpdateCommuters(ECB, queueC.queue1,
                queueC.queuePoint1, queueC.queuePoint1_1);
            UpdateCommuters(ECB, queueC.queue2,
                queueC.queuePoint2, queueC.queuePoint2_1);
            UpdateCommuters(ECB, queueC.queue3,
                queueC.queuePoint3, queueC.queuePoint3_1);
            UpdateCommuters(ECB, queueC.queue4,
                queueC.queuePoint4, queueC.queuePoint4_1);

            // See if train is boarding

        }

        public void UpdateCommuters(EntityCommandBuffer ECB, 
            NativeList<Entity> queue, float3 queuePos0, float3 queuePos1)
        {
            Debug.Log($"Queue pos: {queuePos0}");
            for (int i = 0; i < queue.Length; i++)
            {
                ECB.SetComponent<CommuterQueuerComponent>(queue[i], new CommuterQueuerComponent
                {
                    inQueue = true,
                    queueIndex = i,
                    readyForBoarding = false,
                    finishedTask = false,
                    queueStartPosition = queuePos0,
                    queueDirection = queuePos1 - queuePos0,
                    state = QueueState.InQueue,
                });
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
        public void Execute(in Entity entity, in CommuterQueuerComponent queuer, ref WalkComponent walker, 
            in CommuterComponent commuter, in WorldTransform transform)
        {

            if (commuter.tasks.IsEmpty)
                return;

            if (commuter.tasks.NextStackElement().state != CommuterState.QUEUE)
                return;

            if (queuer.state != QueueState.InQueue)
                return;

            if (!walker.destinations.IsEmpty)
            {
                walker.destinations.Clear();
            }

            if (walker.destinations.IsEmpty)
            {
                float3 target = queuer.queueStartPosition + queuer.queueDirection * queuer.queueIndex * 0.5f;

                if (math.distance(target, transform.Position) > 0.1f)
                    walker.destinations.Push(target);
            }
        }
    }
}