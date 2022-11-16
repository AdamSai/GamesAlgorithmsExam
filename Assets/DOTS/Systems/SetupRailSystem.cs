using DOTS.Components;
using DOTS.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
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
        //new AddOutboundPointsJob().Run();


        NativeList<RailMarkerComponent> entityQuery = 
            new EntityQueryBuilder(Allocator.Temp).WithAll<RailMarkerComponent>().Build(ref state).
            ToComponentDataListAsync<RailMarkerComponent>(Allocator.Persistent, out var foo);

        EntityQuery metroLine = new EntityQueryBuilder(Allocator.Temp).WithAll<BezierPathComponent>().Build(ref state);

        var dependency = JobHandle.CombineDependencies(foo, state.Dependency);

        state.Dependency = new AddOutboundPointsJob() { query = entityQuery }.Schedule(dependency);

        // Stop the job
        state.Enabled = false;
    }
}

