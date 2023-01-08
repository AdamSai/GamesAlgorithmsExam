using System.Linq;
using DOTS.Components;
using DOTS.Components.Train;
using DOTS.Jobs;
using DOTS.Utility;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

// Run the system after  SetupTrainsSystem
[UpdateAfter(typeof(SetupTrainsSystem))]
public partial struct UpdateCarriagesSystem : ISystem
{
    private EntityQuery trainQuery;
    private EntityQuery metroLineQuery;
    private EntityQuery metroLineQuery2;
    private EntityCommandBuffer ECB;

    private ComponentLookup<TrainPositionComponent> trainPosLookUp;
    private ComponentLookup<TrainIDComponent> trainIDs;
    private ComponentLookup<TrainIDComponent> trainIDs2;
    private ComponentLookup<PlatformComponent> platforms;
    private ComponentLookup<MetroLineComponent> metroLineLookUp;
    private ComponentLookup<MetroLineComponent> metroLineLookUp2;
    private ComponentLookup<BezierPathComponent> bezierPathLookup;
    private EntityQuery bezierPathQuery;
    private EntityQuery platformEntitiesQuery;
    private BufferLookup<DOTS.BezierPoint> bezierLookup;
    private BufferLookup<DOTS.BezierPoint> bezierLookup2;


    public void OnCreate(ref SystemState state)
    {
        trainQuery =
            new EntityQueryBuilder(Allocator.Temp)
                .WithAll<TrainPositionComponent, TrainIDComponent, TrainSpeedComponent>().Build(ref state);
        metroLineQuery =
            new EntityQueryBuilder(Allocator.Temp).WithAll<BezierPathComponent, MetroLineComponent>().Build(ref state);
        metroLineQuery2 =
            new EntityQueryBuilder(Allocator.Temp).WithAll<BezierPathComponent, MetroLineComponent>().Build(ref state);
        bezierPathQuery =
            new EntityQueryBuilder(Allocator.Temp).WithAll<BezierPathComponent>().Build(ref state);
        platformEntitiesQuery =
            new EntityQueryBuilder(Allocator.Temp).WithAll<PlatformComponent>().Build(ref state);
        trainPosLookUp = state.GetComponentLookup<TrainPositionComponent>();
        trainIDs = state.GetComponentLookup<TrainIDComponent>();
        trainIDs2 = state.GetComponentLookup<TrainIDComponent>();
        platforms = state.GetComponentLookup<PlatformComponent>();
        bezierLookup = state.GetBufferLookup<DOTS.BezierPoint>(true);
        bezierLookup2 = state.GetBufferLookup<DOTS.BezierPoint>(true);
        metroLineLookUp = state.GetComponentLookup<MetroLineComponent>();
        metroLineLookUp2 = state.GetComponentLookup<MetroLineComponent>();
        bezierPathLookup = state.GetComponentLookup<BezierPathComponent>();
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {
        ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        var bezierPaths = bezierPathQuery.ToComponentDataArray<BezierPathComponent>(Allocator.Persistent);
        var platformEntities = platformEntitiesQuery.ToEntityArray(Allocator.Persistent);
        var metroLines2 =
            metroLineQuery2.ToEntityArray(Allocator.Persistent);
        trainPosLookUp.Update(ref state);
        trainIDs.Update(ref state);
        platforms.Update(ref state);
        bezierLookup.Update(ref state);
        metroLineLookUp.Update(ref state);
        var ecb = new EntityCommandBuffer(Allocator.Persistent);
        var updateTrainsJob = new UpdateTrainStatesJob
        {
            trainsPositions = trainPosLookUp,
            trainIDs = trainIDs,
            platforms = platforms,
            bezierPathComponents = bezierPaths,
            platformEntities = platformEntities,
            bezierLookup = bezierLookup,
            metroLineComponents = metroLineLookUp,
            metroLines = metroLines2,
            deltaTime = SystemAPI.Time.DeltaTime,
        };
        updateTrainsJob.Run();
        // updateTrainHandle.Complete();
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
        // updateTrainHandle.Complete();

        var trainJob = new UpdateTrainsPositionsJob {deltaTime = SystemAPI.Time.DeltaTime};
        var updateTrainPosHandle = trainJob.Schedule(state.Dependency);

        var carriageDependency = JobHandle.CombineDependencies(updateTrainPosHandle, state.Dependency);

        var trains =
            trainQuery.ToEntityArray(Allocator.Persistent);
        var metroLines =
            metroLineQuery.ToEntityArray(Allocator.Persistent);

        metroLineLookUp2.Update(ref state);
        trainIDs2.Update(ref state);
        bezierLookup2.Update(ref state);
        bezierPathLookup.Update(ref state);
        var carriageJob = new UpdateCarriageJob
        {
            trains = trains,
            ECB = ECB,
            metroLines = metroLines,
            tPos = state.GetComponentLookup<TrainPositionComponent>(),
            bezierPoints = bezierLookup2,
            MetroLineComponents = metroLineLookUp2,
            trainIDs = trainIDs2,
            BezierPaths = bezierPathLookup
        };
        state.Dependency = carriageJob.Schedule(carriageDependency);
        // state.Dependency.Complete();
        state.CompleteDependency();
        // var dependency = JobHandle.CombineDependencies(trainJobHandle, state.Dependency);
        // state.Dependency = new UpdateCarriageJob {trains = trains}.Schedule(dependency);
    }
}

public partial struct UpdateTrainsPositionsJob : IJobEntity
{
    public float deltaTime;

    public void Execute(in Entity ent, ref TrainPositionComponent tpos, ref TrainSpeedComponent speed)
    {
        // Set initial speed and position
        // var pos = tpos.value;

        tpos.value = ((tpos.value += speed.speed * deltaTime) % 1f);
        speed.speed *= speed.friction; // TODO: See Train_railFriction on Metro.cs

        // tpos.value = pos;
        }
}

public partial struct UpdateCarriageJob : IJobEntity
{
    public EntityCommandBuffer ECB;
    public NativeArray<Entity> trains;
    public NativeArray<Entity> metroLines;
    public ComponentLookup<TrainPositionComponent> tPos;
    public ComponentLookup<TrainIDComponent> trainIDs;
    public ComponentLookup<MetroLineComponent> MetroLineComponents;
    public ComponentLookup<BezierPathComponent> BezierPaths;
    public BufferLookup<DOTS.BezierPoint> bezierPoints;

    public void Execute(Entity ent, CarriageIDComponent carriageIDComponent, ref CarriagePositionComponent carriagePos)
    {
        Entity trainEntity = default;
        Entity metroLine = default;

        // Find the correct Train
        for (var i = 0; i < trains.Length; i++)
        {
            if (trainIDs.GetRefRO(trains[i]).ValueRO.LineIndex == carriageIDComponent.lineIndex &&
                trainIDs.GetRefRO(trains[i]).ValueRO.TrainIndex == carriageIDComponent.trainIndex)
            {
                trainEntity = trains[i];
                break;
            }
        }

        // Find the correct MetroLine
        for (var i = 0; i < metroLines.Length; i++)
        {
            if (MetroLineComponents.GetRefRO(metroLines[i]).ValueRO.MetroLineID == carriageIDComponent.lineIndex)
            {
                metroLine = metroLines[i];
                break;
            }
        }

        var bezierBuffer = bezierPoints[metroLine];

        // UpdateCarriages
        // Update position on the bezier
        var bezier = BezierPaths.GetRefRO(metroLine).ValueRO;
        float carriageOffset = 3f;
        float pos = tPos[trainEntity].value - carriageIDComponent.id * carriageOffset / bezier.distance;


        if (pos >= 1f)
            pos %= 1f;

        carriagePos.value = pos;

        var posOnRail = BezierUtility.Get_Position(pos, bezier.distance, bezierBuffer);
        var rotOnRail = BezierUtility.Get_NormalAtPosition(pos, bezier.distance, bezierBuffer);
        // Debug.Log(carriageIDComponent.lineIndex + ":" + carriageIDComponent.id + ": " + carriageIDComponent.id * carriageOffset);

        // Set rotation and position
        var transform = LocalTransform.FromPosition(posOnRail);
        var rot = Quaternion.LookRotation(transform.Position - (transform.Position - rotOnRail), Vector3.up);
        transform.Rotation = rot;
        ECB.SetComponent(ent, transform);
    }
}