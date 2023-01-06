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
    private EntityCommandBuffer ECB;

    private ComponentLookup<TrainPositionComponent> trainPosLookUp;
    private ComponentLookup<TrainIDComponent> trainIDs;
    private ComponentLookup<PlatformComponent> platforms;
    private EntityQuery bezierPathQuery;
    private EntityQuery platformEntitiesQuery;
    
    
    public void OnCreate(ref SystemState state)
    {
        trainQuery =
            new EntityQueryBuilder(Allocator.Temp)
                .WithAll<TrainPositionComponent, TrainIDComponent, TrainSpeedComponent>().Build(ref state);
        metroLineQuery =
            new EntityQueryBuilder(Allocator.Temp).WithAll<BezierPathComponent, MetroLineComponent>().Build(ref state);
       
        bezierPathQuery =
            new EntityQueryBuilder(Allocator.Temp).WithAll<BezierPathComponent>().Build(ref state);
        platformEntitiesQuery =
            new EntityQueryBuilder(Allocator.Temp).WithAll<PlatformComponent>().Build(ref state);
        trainPosLookUp = state.GetComponentLookup<TrainPositionComponent>();
        trainIDs = state.GetComponentLookup<TrainIDComponent>();
        platforms = state.GetComponentLookup<PlatformComponent>();
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

        trainPosLookUp.Update(ref state);
        trainIDs.Update(ref state);
        platforms.Update(ref state);
        var updateTrainsJob = new UpdateTrainStatesJob
        {
            trainsPositions = trainPosLookUp,
            trainIDs = trainIDs,
            platforms = platforms,
            bezierPaths = bezierPaths,
            platformEntities = platformEntities,
        };

        var updateTrainHandle = updateTrainsJob.Schedule(state.Dependency);
        updateTrainHandle.Complete();

        var trainPositionDependencies = JobHandle.CombineDependencies(updateTrainHandle, state.Dependency);
        var trainJob = new UpdateTrainsPositionsJob {deltaTime = SystemAPI.Time.DeltaTime};
        state.Dependency = trainJob.Schedule(trainPositionDependencies);
        state.Dependency.Complete();

        var trains =
            trainQuery.ToEntityArray(Allocator.Persistent);
        var metroLines =
            metroLineQuery.ToEntityArray(Allocator.Persistent);
        var carriageJob = new UpdateCarriageJob
        {
            trains = trains,
            ECB = ECB,
            metroLines = metroLines,
            EM = state.EntityManager,
            tPos = state.GetComponentLookup<TrainPositionComponent>()
        };
        state.Dependency = carriageJob.Schedule(state.Dependency);
        state.Dependency.Complete();

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
        var pos = tpos.value;
        var Speed = speed.speed;
        var Friction = speed.friction;

        pos = ((pos += Speed * deltaTime) % 1f);
        Speed *= Friction; // TODO: See Train_railFriction on Metro.cs

        speed.speed = Speed;
        tpos.value = pos;
    }
}

public partial struct UpdateCarriageJob : IJobEntity
{
    public EntityManager EM;
    public EntityCommandBuffer ECB;
    public NativeArray<Entity> trains;
    public NativeArray<Entity> metroLines;
    public ComponentLookup<TrainPositionComponent> tPos;

    public void Execute(Entity ent, CarriageIDComponent carriageIDComponent, ref CarriagePositionComponent carriagePos)
    {
        Entity trainEntity = default;
        Entity metroLine = default;

        // Find the correct Train
        for (var i = 0; i < trains.Length; i++)
        {
            if (EM.GetComponentData<TrainIDComponent>(trains[i]).LineIndex == carriageIDComponent.lineIndex &&
                EM.GetComponentData<TrainIDComponent>(trains[i]).TrainIndex == carriageIDComponent.trainIndex)
            {
                trainEntity = trains[i];
                break;
            }
        }

        // Find the correct MetroLine
        for (var i = 0; i < metroLines.Length; i++)
        {
            if (EM.GetComponentData<MetroLineComponent>(metroLines[i]).MetroLineID == carriageIDComponent.lineIndex)
            {
                metroLine = metroLines[i];
                break;
            }
        }

        // UpdateCarriages
        // Update position on the bezier
        var bezier = EM.GetComponentData<BezierPathComponent>(metroLine);
        float carriageOffset = 10f;
        float pos = tPos[trainEntity].value + carriageIDComponent.id * carriageOffset / bezier.distance;


        if (pos >= 1f)
            pos %= 1f;

        carriagePos.value = pos;

        var posOnRail = BezierUtility.Get_Position(pos, bezier.distance, bezier.points);
        var rotOnRail = BezierUtility.Get_NormalAtPosition(pos, bezier.distance, bezier.points);
        // Debug.Log(carriageIDComponent.lineIndex + ":" + carriageIDComponent.id + ": " + carriageIDComponent.id * carriageOffset);

        // Set rotation and position
        var transform = LocalTransform.FromPosition(posOnRail);
        var rot = Quaternion.LookRotation(transform.Position - (transform.Position - rotOnRail), Vector3.up);
        transform.Rotation = rot;
        ECB.SetComponent(ent, transform);
    }

 
}