using DOTS.Components;
using DOTS.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
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
        // TODO: Instead of looking up railmarkers look up children and get the components from there maybe ?
        var railMarkerQuery =
            new EntityQueryBuilder(Allocator.Temp).WithAll<RailMarkerComponent>().Build(ref state)
                .ToComponentDataListAsync<RailMarkerComponent>(Allocator.Persistent, out var railMarkers);

        var dependency = JobHandle.CombineDependencies(railMarkers, state.Dependency);

        state.Dependency = new AddOutboundPointsJob() { railMarkers = railMarkerQuery }.Schedule(dependency);

        // Stop the job
        state.Enabled = false;
    }
}