using DOTS.Components;
using DOTS.Jobs;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

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
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        // TODO: Instead of looking up railmarkers look up children and get the components from there maybe ?

        #region SetupRails

        // Creating an EntityCommandBuffer to defer the structural changes required by instantiation.
        var railMarkers =
            railMarkerQuery.ToComponentDataListAsync<RailMarkerComponent>(Allocator.Persistent,
                out var railMarkerJobHandle);

        var dependency0 = JobHandle.CombineDependencies(railMarkerJobHandle, state.Dependency);
        // var dependency = JobHandle.CombineDependencies(railMarkerJobHandle, state.Dependency);
        var outboundsJob = new AddOutboundPointsJob()
        {
            railMarkers = railMarkers,
            ECB = ecb,
            // lineRailMarkers = linerRailMarkers
        }
            .Schedule(dependency0);
        outboundsJob.Complete();
        //ecb.Playback(state.EntityManager);

        var platforms =
            platformQuery.ToEntityListAsync(Allocator.Persistent, out var platformJobHandle);

        new ConnectOppositePlatformsJob().Run();
        // var dependency2 = JobHandle.CombineDependencies(rotatePlatformsJob, platformJobHandle, state.Dependency);
        var dependency2 = JobHandle.CombineDependencies(platformJobHandle, state.Dependency);

        dependency2.Complete();
        platformComponentLookup.Update(ref state);
        var addMissingPlatformsJob = new AddMissingPlatformsJob
        {
            PlatformsEntities = platforms,
            platformLookUp = platformComponentLookup
        }.Schedule(dependency2);

        // state.Dependency.Complete();
        var dependency3 = JobHandle.CombineDependencies(addMissingPlatformsJob, state.Dependency);
        platform2ComponentLookup.Update(ref state);
        state.Dependency = new FooJob { platforms = platform2ComponentLookup }.Schedule(dependency3);

        #endregion

        #region Trains

        // state.Dependency = new SetupTrainsJob
        // {
        //     ECB = ecb
        // }.Schedule(state.Dependency);
        //
        //
        //
        // state.Dependency = new SetupCarriagesJob
        // {
        //     ECB = ecb
        // }.Schedule(state.Dependency);
        //
        //
        // // var entityArray = new EntityQueryBuilder(Allocator.Temp)
        // //     .WithAny<CarriageColorTag>().Build(ref state).ToEntityArray(Allocator.Temp);
        // //
        // // UnityEngine.Debug.Log("Entity Array size: " + entityArray.Length);
        // //
        // // for (int i = 0; i < entityArray.Length; i++)
        // // {
        // //     ecb.AddComponent(entityArray[i], new URPMaterialPropertyBaseColor {Value = new float4(0, 0, 1, 1)});
        // //     UnityEngine.Debug.Log("Added Color to: " + entityArray[i].ToString());
        // // }
        //

        #endregion

        state.CompleteDependency();

        // Stop the job
        state.Enabled = false;
    }

    [WithAll(typeof(PlatformComponent))]
    public partial struct FooJob : IJobEntity
    {
        public ComponentLookup<PlatformComponent> platforms;

        public void Execute(in Entity ent)
        {
            var thisOne = platforms.GetRefRO(ent).ValueRO;
            Debug.Log("Length: " + thisOne.neighborPlatforms.Length);
            foreach (var platform in thisOne.neighborPlatforms)
            {
                var foo = platforms.GetRefRO(platform).ValueRO;
                Debug.Log(
                    $"{thisOne.parentMetroName}_{thisOne.platformIndex} -- {foo.parentMetroName}_{foo.platformIndex}");
            }
        }
    }
}