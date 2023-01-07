using Assets.DOTS.Components;
using DOTS.Components;
using DOTS.Jobs;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UI;

// [BurstCompile]
public partial struct SetupRailSystem : ISystem
{
    private EntityQuery railMarkerQuery;
    private EntityQuery platformQuery;
    private ComponentLookup<PlatformComponent> platformComponentLookup;
    private ComponentLookup<PlatformComponent> platform2ComponentLookup;

    // [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        railMarkerQuery =
            new EntityQueryBuilder(Allocator.Temp).WithAll<RailMarkerComponent>().Build(ref state);
        platformQuery =
            new EntityQueryBuilder(Allocator.Temp).WithAll<PlatformComponent>().Build(ref state);
        platformComponentLookup = state.GetComponentLookup<PlatformComponent>();
        platform2ComponentLookup = state.GetComponentLookup<PlatformComponent>();
    }

    // [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        state.Enabled = false;
    }

    // [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Persistent);
        // TODO: Instead of looking up railmarkers look up children and get the components from there maybe ?

        // Creating an EntityCommandBuffer to defer the structural changes required by instantiation.
        var railMarkers =
            railMarkerQuery.ToComponentDataListAsync<RailMarkerComponent>(Allocator.Persistent,
                out var railMarkerJobHandle);
        var railmarkerJobHandle = JobHandle.CombineDependencies(railMarkerJobHandle, state.Dependency);
        
        // Create the bezier curves for the metrolines and spawn platforms and rails.
        var outboundsJob = new AddOutboundPointsJob()
        {
            railMarkers = railMarkers,
            ECB = ecb,
            // lineRailMarkers = linerRailMarkers
        }.Schedule(railmarkerJobHandle);
        outboundsJob.Complete();
        ecb.Playback(state.EntityManager);

        // Connect plaforms to the ones on the opposite side of the tracks
        new ConnectOppositePlatformsJob().Run();

        // Query for all the platforms so we can connect them
        var platforms =
            platformQuery.ToEntityListAsync(Allocator.Persistent, out var platformJobHandle);
        var platformQueryJobHandle = JobHandle.CombineDependencies(platformJobHandle, state.Dependency);
        platformQueryJobHandle.Complete();
        
        // Connect platforms with their neighbours
        platformComponentLookup.Update(ref state);
        state.Dependency = new ConnectPlatformNeighbours
        {
            PlatformsEntities = platforms,
            platformLookUp = platformComponentLookup
        }.Schedule(platformQueryJobHandle);


        ecb.Dispose();
        state.CompleteDependency();

        // Stop the job
        state.Enabled = false;
    }
}

public partial struct SpawnCommutersJob : IJobEntity
{
    EntityCommandBuffer ECB;

    public void Execute(in Entity entity, in CommuterSpawnComponent spawner, in LocalTransform transform)
    {
        for (int i = 0; i < spawner.amount; i++)
        {
            Entity commuter = ECB.Instantiate(spawner.commuter);

            ECB.SetComponent<LocalTransform>(commuter, LocalTransform.FromPosition(transform.Position));
            ECB.SetComponent<CommuterComponent>(commuter, new CommuterComponent
            {
                tasks = new NativeList<CommuterComponentTask>(Allocator.Persistent),
                currentPlatform = entity
            });
        }

        ECB.RemoveComponent<SpawnCommutersJob>(entity);
    }
}