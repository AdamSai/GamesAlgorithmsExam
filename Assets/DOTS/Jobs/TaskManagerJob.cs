using Assets.DOTS.Components.Train;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Assets.DOTS.Utilities.Stack;

namespace Assets.DOTS.Jobs
{
    public partial struct TaskManagerJob : IJobEntity
    {
        public EntityCommandBuffer ECB;
        public ComponentLookup<PlatformComponent> platformComponent;
        public ComponentLookup<LocalToWorld> worldTransforms;
        public ComponentLookup<CarriagePassengerSeatsComponent> seatsComponent;
        public ComponentLookup<CarriageNavPointsComponent> carriageNavPoints;

        // A job which will switch tasks for the commuter
        // Job is dependent on platformcomponent and carriageseats component

        // IMPORTANT: CANNOT BE SCHEDULED IN PARRALLEL
        public void Execute(in Entity entity, ref PassengerComponent passenger, 
            ref CommuterComponent commuter, ref WalkComponent walker)
        {
            bool jobFinished = false;

            var currentTask = commuter.tasks.LastElement();

            switch (currentTask.state)
            {
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
                    break;
                case CommuterState.WAIT_FOR_STOP:
                    // See if train has stopped and is open
                    break;

            }

            if (jobFinished)
            {
                // Remove last, finished task
                commuter.tasks.Pop();

                // Get new task
                var newTask = commuter.tasks.LastElement();

                switch (newTask.state)
                {
                    case CommuterState.WALK:
                        // Walk from one platform to another platform
                        // Note: Stack push, therefore destinations are added in reverse order
                        walker.destinations.Push(platformComponent[newTask.endPlatform].platform_entrance2);
                        walker.destinations.Push(platformComponent[newTask.endPlatform].platform_entrance1);
                        walker.destinations.Push(platformComponent[newTask.endPlatform].platform_entrance0);
                        walker.destinations.Push(platformComponent[newTask.startPlatform].platform_exit0);
                        walker.destinations.Push(platformComponent[newTask.startPlatform].platform_exit1);
                        walker.destinations.Push(platformComponent[newTask.startPlatform].platform_exit2);
                        break;
                    case CommuterState.GET_ON_TRAIN:
                        // Get on train that is already stopped
                        // Get train entity
                        // Get carriage entity
                        // Get get carriage entrance navpoint entity
                        // Set walk to that entity's world position
                        // Iterate carriage seat entities to find one available
                        // Set it unavailable and walk there
                        break;
                    case CommuterState.GET_OFF_TRAIN:
                        // Get off train that is already stopped
                        // Get train entity
                        // Get platform entity
                        // add walk destination for carriage nav point
                        // walker.destinations.Push(platformComponent[newTask.endPlatform].platform_entrance2);
                        break;
                    case CommuterState.QUEUE:
                        // Wait for train to stop, when on platform
                        // If there is time, make a fancy queue
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