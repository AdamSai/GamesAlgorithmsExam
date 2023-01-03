using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public partial struct PassengerJob : IJobEntity
{
    public EntityCommandBuffer ECB;
    public ComponentLookup<CarriageComponentDummy> platformComponents;
    public ComponentLookup<LocalTransform> transforms;
    public ComponentLookup<LocalToWorld> worldTransforms;

    public void Execute(Entity e, LocalToWorld worldTransform, in PassengerComponent passenger, in CommuterComponent commuter, ref LocalTransform transformm)
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