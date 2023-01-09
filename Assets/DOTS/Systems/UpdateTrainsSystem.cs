using DOTS.Components;
using DOTS.Jobs;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

// Run the system after  SetupTrainsSystem
[UpdateAfter(typeof(SetupTrainsSystem))]
public partial struct UpdateTrainsSystem : ISystem
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


        //var platformEntities = platformEntitiesQuery.ToEntityArray(Allocator.Persistent);

        //NativeArray<Entity> platformEntitiesSorted = new NativeArray<Entity>(platformEntities.Length, Allocator.Persistent);



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
            ECB = ecb,
            EM = state.EntityManager,
        };
        state.Dependency = updateTrainsJob.Schedule(state.Dependency);
        // updateTrainHandle.Complete();
        state.Dependency.Complete();
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
        // updateTrainHandle.Complete();

        var trainJob = new UpdateTrainsPositionsJob { deltaTime = SystemAPI.Time.DeltaTime };
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
        var carriageJob = new UpdateCarriagesJob
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