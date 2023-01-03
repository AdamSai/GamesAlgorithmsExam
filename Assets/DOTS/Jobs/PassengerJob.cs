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

    public void Execute(in PassengerComponent passenger, in CommuterComponent commuter, ref LocalTransform transform)
    {
        if (passenger.currentCarriage == Entity.Null)
            return;

        // Check is train is stopped
        // Then see if commuter needs to leave


        // Otherwise:
        transform.Translate(transforms[passenger.carriageSeat].Position);

        //LocalTransform.FromPosition(posOnRail);
    }
}