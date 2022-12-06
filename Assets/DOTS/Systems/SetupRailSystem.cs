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

        // Creating an EntityCommandBuffer to defer the structural changes required by instantiation.
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        
        var dependency = JobHandle.CombineDependencies(railMarkers, state.Dependency);
        state.Dependency = new AddOutboundPointsJob() { railMarkers = railMarkerQuery, ECB = ecb }.Schedule(dependency);

        // Fix outbound handles
        // new FixOutboundHandlesJob().Schedule();

        // Fix return handles


        // Stop the job
        state.Enabled = false;
    }
}