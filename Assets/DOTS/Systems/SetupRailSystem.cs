using DOTS.Components;
using DOTS.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct SetupRailSystem : ISystem
{
    private EntityQuery railMarkerQuery;
    private NativeList<RailMarkerComponent> linerRailMarkers;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        railMarkerQuery =
            new EntityQueryBuilder(Allocator.Temp).WithAll<RailMarkerComponent>().Build(ref state);
        linerRailMarkers = new NativeList<RailMarkerComponent>(Allocator.Persistent);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        railMarkerQuery.Dispose();
    }

    [BurstCompile]
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
                EM = state.EntityManager,
                // lineRailMarkers = linerRailMarkers
            }
            .Schedule(dependency0);
        outboundsJob.Complete();
        ecb.Playback(state.EntityManager);
        
        var dependency1 = JobHandle.CombineDependencies(outboundsJob, state.Dependency);
        
        state.Dependency = new RotatePlatformsJob
        {
            EM = state.EntityManager
        }.Schedule(dependency1);
        

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
}