using DOTS.Components;
using DOTS.Jobs;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[BurstCompile]
public partial struct SetupRailSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {

    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Order RailMarker by index
        new AddOutboundPointsJob().Run();
        // Stop the job
        state.Enabled = false;
    }
}

