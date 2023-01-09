using Assets.DOTS.Components;
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
        public ComponentLookup<PlatformComponent> platformComponent;
        public ComponentLookup<LocalToWorld> worldTransforms;
        public ComponentLookup<CarriageNavPointsComponent> carriageNavPoints;
        private ComponentLookup<TrainStateComponent> trainStateComponents;
        
        public void OnCreate(ref SystemState state)
        {
            worldTransforms = state.GetComponentLookup<LocalToWorld>();
            platformComponent = state.GetComponentLookup<PlatformComponent>();
            carriageNavPoints = state.GetComponentLookup<CarriageNavPointsComponent>();
            trainStateComponents = state.GetComponentLookup<TrainStateComponent>();
        }

        public void OnDestroy(ref SystemState state)
        {

        }

        public void OnUpdate(ref SystemState state)
        {
            platformComponent.Update(ref state);
            worldTransforms.Update(ref state);
            carriageNavPoints.Update(ref state);
            trainStateComponents.Update(ref state);



            var job = new TaskManagerJob
            {
                platformComponent = platformComponent,
                worldTransforms = worldTransforms,
                carriageNavPoints = carriageNavPoints,
                trainStateComponents = trainStateComponents,
            };

            state.Dependency = job.Schedule(state.Dependency);
            state.Dependency.Complete();

        }
    }

    public partial struct TaskManagerJob : IJobEntity
    {
        public ComponentLookup<PlatformComponent> platformComponent;
        public ComponentLookup<LocalToWorld> worldTransforms;
        public ComponentLookup<CarriageNavPointsComponent> carriageNavPoints;
        public ComponentLookup<TrainStateComponent> trainStateComponents;

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


                switch (newTask.state)
                {
                    case CommuterState.WALK:
                        // Walk from one platform to another platform
                        // Note: Stack push, therefore destinations are added in reverse order
                        walker.destinations.Push(platformComponent[newTask.endPlatform].platform_entrance0);
                        walker.destinations.Push(platformComponent[newTask.endPlatform].platform_entrance1);
                        walker.destinations.Push(platformComponent[newTask.endPlatform].platform_entrance2);
                        walker.destinations.Push(platformComponent[newTask.startPlatform].platform_exit2);
                        walker.destinations.Push(platformComponent[newTask.startPlatform].platform_exit1);
                        walker.destinations.Push(platformComponent[newTask.startPlatform].platform_exit0);
                        break;
                    case CommuterState.GET_ON_TRAIN:
                        // Get on train that is already stopped
                        // Get train entity
                        // Get carriage entity
                        // Get get carriage entrance navpoint entity
                        // Set walk to that entity's world position
                        // Iterate carriage seat entities to find one available
                        // Set it unavailable and walk there
                        walker.destinations.Clear();
                        walker.destinations.Push(worldTransforms[passenger.carriageSeat].Position);
                        walker.destinations.Push(worldTransforms[carriageNavPoints[passenger.currentCarriage].entrancePointEntity].Position);
                        break;
                    case CommuterState.GET_OFF_TRAIN:
                        // Get off train that is already stopped
                        // Get train entity
                        // Get platform entity
                        // add walk destination for carriage nav point
                        // walker.destinations.Push(platformComponent[newTask.endPlatform].platform_entrance2);
                        walker.destinations.Clear();
                        walker.destinations.Push(worldTransforms[carriageNavPoints[passenger.currentCarriage].entrancePointEntity].Position);
                        break;
                    case CommuterState.QUEUE:
                        // Wait for train to stop, when on platform
                        // If there is time, make a fancy queue
                        queuer.state = QueueState.None;
                        break;
                    case CommuterState.WAIT_FOR_STOP:
                        // Wait for train to stop, when on train
                        // Get train entity and see if it is in a stopped state
                        break;
                }
            }
        }

        
    }


}