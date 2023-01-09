using Assets.DOTS.Components.Train;
using Assets.DOTS.Utility.Stack;
using System.Collections;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Assets.DOTS.Systems
{
    [BurstCompile]
    public partial struct PassengerSystem : ISystem
    {
        public ComponentLookup<WorldTransform> transformLookup;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            transformLookup = state.GetComponentLookup<WorldTransform>();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            
            transformLookup.Update(ref state);

            var job = new PassengerJob { worldTransforms = transformLookup };

            job.Run();
        }

        [BurstCompile]
        public partial struct PassengerJob : IJobEntity
        {
            //public EntityCommandBuffer ECB;
            //public ComponentLookup<CarriagePassengerSeatsComponent> carriageSeats;
            public ComponentLookup<WorldTransform> worldTransforms;

            public void Execute(in CommuterComponent commuter, in PassengerComponent passenger, ref LocalTransform transformm)
            {
                if (commuter.tasks.IsEmpty || commuter.tasks.NextStackElement().state != CommuterState.WAIT_FOR_STOP)
                    return;

                if (passenger.currentCarriage == Entity.Null)
                    return;

                // Check is train is stopped
                // Then see if commuter needs to leave


                // Otherwise:
                // Set rotation and position
                //var seatPos = worldTransforms[passenger.carriageSeat].Position;
                //var transform = LocalTransform.FromPosition(seatPos);
                //ECB.SetComponent(e, transform);

                transformm.Position = worldTransforms[passenger.carriageSeat].Position;

                //LocalTransform.FromPosition(posOnRail);
            }
        }
    }
}