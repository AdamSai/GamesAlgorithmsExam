﻿using Assets.DOTS.Components;
using Assets.DOTS.Components.Train;
using Assets.DOTS.Utility.Stack;
using DOTS.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Assets.DOTS.Systems
{
    [UpdateAfter(typeof(SetupCarriagesSystem))]
    [UpdateAfter(typeof(SetupSeatsSystem))]
    public partial struct TaskManagerSystem : ISystem
    {
        private EntityQuery carriageIDQuery;

        public void OnCreate(ref SystemState state)
        {
            carriageIDQuery =
            new EntityQueryBuilder(Allocator.Persistent).WithAll<CarriageIDComponent>().Build(ref state);
        }

        public void OnDestroy(ref SystemState state)
        {

        }

        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer ECB = new EntityCommandBuffer(Allocator.Persistent);
            ComponentLookup<PlatformComponent> platformComponent = state.GetComponentLookup<PlatformComponent>();
            platformComponent.Update(ref state);
            ComponentLookup<LocalToWorld> worldTransforms = state.GetComponentLookup<LocalToWorld>();
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
            ComponentLookup<TrainStateComponent> trainStateComponents = state.GetComponentLookup<TrainStateComponent>();
            trainStateComponents.Update(ref state);

            var carriageIDEntities =
            carriageIDQuery.ToEntityArray(Allocator.Persistent);

            var job = new TaskManagerJob
            {
                ECB = ECB,
                platformComponent = platformComponent,
                worldTransforms = worldTransforms,
                seatsComponent = seatsComponent,
                carriageNavPoints = carriageNavPoints,
                carriageIDComponents = carriageIDComponents,
                seatComponents = seatComponents,
                trainIDComponents = trainIDComponents,
                trainStateComponents = trainStateComponents,
                carriageIDEntities = carriageIDEntities,
            };

            state.Dependency = job.Schedule(state.Dependency);
            state.Dependency.Complete();

            ECB.Dispose();
        }
    }

    public partial struct TaskManagerJob : IJobEntity
    {
        public EntityCommandBuffer ECB;
        public ComponentLookup<PlatformComponent> platformComponent;
        public ComponentLookup<LocalToWorld> worldTransforms;
        public ComponentLookup<CarriagePassengerSeatsComponent> seatsComponent;
        public ComponentLookup<CarriageNavPointsComponent> carriageNavPoints;
        public ComponentLookup<CarriageSeatComponent> seatComponents;
        public ComponentLookup<CarriageIDComponent> carriageIDComponents;
        public ComponentLookup<TrainIDComponent> trainIDComponents;
        public ComponentLookup<TrainStateComponent> trainStateComponents;
        public NativeArray<Entity> carriageIDEntities;

        // A job which will switch tasks for the commuter
        // Job is dependent on platformcomponent and carriageseats component

        // IMPORTANT: CANNOT BE SCHEDULED IN PARRALLEL
        public void Execute(in Entity entity, ref PassengerComponent passenger,
            ref CommuterComponent commuter, ref WalkComponent walker, ref CommuterQueuerComponent queuer)
        {
            if (commuter.tasks.IsEmpty)
                return;

            bool jobFinished = false;

            var currentTask = commuter.tasks.NextStackElement();

            switch (currentTask.state)
            {
                case CommuterState.START:
                    jobFinished = true;
                    break;
                case CommuterState.WALK:
                    if (walker.destinations.IsEmpty)
                        jobFinished = true;
                    break;
                case CommuterState.GET_ON_TRAIN:
                    if (walker.destinations.IsEmpty)
                        jobFinished = true;
                    break;
                case CommuterState.GET_OFF_TRAIN:
                    if (walker.destinations.IsEmpty)
                        jobFinished = true;
                    break;
                case CommuterState.QUEUE:
                    // See if there is a train available
                    if (queuer.state == QueueState.Boarding)
                    {
                        jobFinished = true;
                    }
                    break;
                case CommuterState.WAIT_FOR_STOP:
                    // See if train has stopped and is open
                    //Debug.Log($"Passenger train entity: {passenger.currentTrain}");
                    if (trainStateComponents[passenger.currentTrain].value == TrainStateDOTS.UNLOADING)
                    {
                        jobFinished = true;
                    }
                    break;
            }

            

            if (jobFinished)
            {
                // Remove last, finished task
                commuter.tasks.Pop();

                if (commuter.tasks.IsEmpty)
                {
                    // No more tasks, return
                    return;
                }

                // Get new task
                var newTask = commuter.tasks.NextStackElement();

                //Debug.Log($"Current task: {currentTask.state}. Next: {newTask.state}.");

                switch (newTask.state)
                {
                    case CommuterState.WALK:
                        //Debug.Log($"Task is: walk");
                        // Walk from one platform to another platform
                        // Note: Stack push, therefore destinations are added in reverse order
                        //Debug.Log($"Walk task target: {platformComponent[newTask.startPlatform].platform_exit0} with component: {newTask.startPlatform}");
                        walker.destinations.Push(platformComponent[newTask.endPlatform].platform_entrance0);
                        walker.destinations.Push(platformComponent[newTask.endPlatform].platform_entrance1);
                        walker.destinations.Push(platformComponent[newTask.endPlatform].platform_entrance2);
                        walker.destinations.Push(platformComponent[newTask.startPlatform].platform_exit2);
                        walker.destinations.Push(platformComponent[newTask.startPlatform].platform_exit1);
                        walker.destinations.Push(platformComponent[newTask.startPlatform].platform_exit0);
                        break;
                    case CommuterState.GET_ON_TRAIN:
                        //Debug.Log($"Task is: get on train");
                        // Get on train that is already stopped
                        // Get train entity
                        // Get carriage entity
                        // Get get carriage entrance navpoint entity
                        // Set walk to that entity's world position
                        // Iterate carriage seat entities to find one available
                        // Set it unavailable and walk there
                        //Debug.Log("Here0 " + passenger.currentCarriage);
                        //Debug.Log("Here1 " + carriageNavPoints[passenger.currentCarriage].entrancePointEntity);
                        walker.destinations.Clear();
                        walker.destinations.Push(worldTransforms[passenger.carriageSeat].Position);
                        walker.destinations.Push(worldTransforms[carriageNavPoints[passenger.currentCarriage].entrancePointEntity].Position);
                        break;
                    case CommuterState.GET_OFF_TRAIN:
                        //Debug.Log($"Task is: get off train");
                        // Get off train that is already stopped
                        // Get train entity
                        // Get platform entity
                        // add walk destination for carriage nav point
                        // walker.destinations.Push(platformComponent[newTask.endPlatform].platform_entrance2);
                        walker.destinations.Clear();
                        walker.destinations.Push(worldTransforms[carriageNavPoints[passenger.currentCarriage].entrancePointEntity].Position);
                        break;
                    case CommuterState.QUEUE:
                        //Debug.Log($"Queue state in TaskManagerSystem: {queuer.state}");
                        // Wait for train to stop, when on platform
                        // If there is time, make a fancy queue
                        queuer.state = QueueState.None;
                        break;
                    case CommuterState.WAIT_FOR_STOP:
                        //Debug.Log($"Task is: wait for stop");
                        // Wait for train to stop, when on train
                        // Get train entity and see if it is in a stopped state
                        break;
                }
            }
        }

        
    }


}