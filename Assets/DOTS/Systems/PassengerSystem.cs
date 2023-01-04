using Assets.DOTS.Components.Train;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Assets.DOTS.Systems
{
    public partial struct PassengerSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {

        }

        public void OnDestroy(ref SystemState state)
        {

        }

        public void OnUpdate(ref SystemState state)
        {

        }

        public partial struct PassengerJob : IJobEntity
        {
            public EntityCommandBuffer ECB;
            public ComponentLookup<CarriagePassengerSeatsComponent> carriageSeats;
            public ComponentLookup<LocalToWorld> worldTransforms;

            public void Execute(in Entity e, in LocalToWorld worldTransform, in PassengerComponent passenger, in CommuterComponent commuter, ref LocalTransform transformm)
            {
                if (passenger.currentCarriage == Entity.Null)
                    return;

                // Check is train is stopped
                // Then see if commuter needs to leave


                // Otherwise:
                // Set rotation and position
                var seatPos = worldTransforms[passenger.carriageSeat].Position;
                var transform = LocalTransform.FromPosition(seatPos);
                ECB.SetComponent(e, transform);

                //LocalTransform.FromPosition(posOnRail);
            }
        }
    }
}