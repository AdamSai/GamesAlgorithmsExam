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
            var transformLookup = state.GetComponentLookup<LocalToWorld>(true);

            var job = new PassengerJob { worldTransforms = transformLookup };

            job.Run();
        }

        public partial struct PassengerJob : IJobEntity
        {
            //public EntityCommandBuffer ECB;
            //public ComponentLookup<CarriagePassengerSeatsComponent> carriageSeats;
            public ComponentLookup<LocalToWorld> worldTransforms;

            public void Execute(in PassengerComponent passenger, ref LocalTransform transformm)
            {
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