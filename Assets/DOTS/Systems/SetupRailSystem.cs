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
    private BufferLookup<DOTS.BezierPoint> bezierPointBufferLookup;

    // [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        railMarkerQuery =
            new EntityQueryBuilder(Allocator.Temp).WithAll<RailMarkerComponent>().Build(ref state);
        platformQuery =
            new EntityQueryBuilder(Allocator.Temp).WithAll<PlatformComponent>().Build(ref state);
        platformComponentLookup = state.GetComponentLookup<PlatformComponent>();
        platform2ComponentLookup = state.GetComponentLookup<PlatformComponent>();
        bezierPointBufferLookup = state.GetBufferLookup<DOTS.BezierPoint>();
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
        bezierPointBufferLookup.Update(ref state);
        // Create the bezier curves for the metrolines and spawn platforms and rails.
        var outboundsJob = new AddOutboundPointsJob()
        {
            railMarkers = railMarkers,
            ECB = ecb,
            bezierPoints = bezierPointBufferLookup
            // lineRailMarkers = linerRailMarkers
        }.Schedule(railmarkerJobHandle);
        outboundsJob.Complete();
        ecb.Playback(state.EntityManager);

        // Setting nav points for all platforms
        var platformNavJob = new PlatformNavJob
        {
            navTransforms = state.GetComponentLookup<WorldTransform>(),
            navPoints = state.GetComponentLookup<NavPointComponent>(),
            EM = state.EntityManager
        };
        state.Dependency = platformNavJob.Schedule(state.Dependency);
        state.Dependency.Complete();

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

public partial struct PlatformNavJob : IJobEntity
{
    public ComponentLookup<WorldTransform> navTransforms;
    public ComponentLookup<NavPointComponent> navPoints;
    public EntityManager EM;

    public void Execute(in Entity entity, ref PlatformComponent platform)
    {
        var buffer = EM.GetBuffer<LinkedEntityGroup>(entity);
        Debug.Log("Nav Job!");
        foreach (var item in buffer)
        {
            Entity e = item.Value;

            if (EM.HasComponent<NavPointComponent>(e))
            {
                Debug.Log("Setting nav point: " + navPoints[e].pointID);
                switch (navPoints[e].pointID)
                {
                    case 0:
                        platform.platform_entrance0 = navTransforms[e].Position;
                        break;
                    case 1:
                        platform.platform_entrance1 = navTransforms[e].Position;
                        break;
                    case 2:
                        platform.platform_entrance2 = navTransforms[e].Position;
                        break;
                    case 10:
                        // Exit nav points IDs are offset by 10
                        platform.platform_exit0 = navTransforms[e].Position;
                        break;
                    case 11:
                        platform.platform_exit1 = navTransforms[e].Position;
                        break;
                    case 12:
                        platform.platform_exit2 = navTransforms[e].Position;
                        break;
                    default:
                        Debug.Log("ERROR: Nav points ID don't match!");
                        break;
                }
            }
        }
    }
}