using DOTS.Components;
using DOTS.Components.Train;
using DOTS.Jobs;
using Unity.Collections;
using Unity.Entities;

public enum TrainStateDOTS
{
    EN_ROUTE,
    ARRIVING,
    DOORS_OPEN,
    UNLOADING,
    LOADING,
    DOORS_CLOSE,
    DEPARTING,
    EMERGENCY_STOP
}

[UpdateAfter(typeof(SetupRailSystem))]
public partial struct SetupTrainsSystem : ISystem
{
    private EntityQuery trainQuery;
    private ComponentLookup<TrainIDComponent> trainIDLookup;

    public void OnCreate(ref SystemState state)
    {
        trainQuery =
            new EntityQueryBuilder(Allocator.Temp).WithAll<TrainAheadComponent>().Build(ref state);
        trainIDLookup = state.GetComponentLookup<TrainIDComponent>();
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {
        var setupTrainsECB = new EntityCommandBuffer(Allocator.Persistent);

        var SetupTrainsJob = new SetupTrainsJob
        {
            ECB = setupTrainsECB
        };
        SetupTrainsJob.Run();
        setupTrainsECB.Playback(state.EntityManager);

        var trains =
            trainQuery.ToEntityArray(Allocator.Persistent);
        trainIDLookup.Update(ref state);
        var SetupTrainAheadJob = new SetupTrainAheadJob
        {
            trainIdLookup = trainIDLookup,
            trains = trains
        };

        SetupTrainAheadJob.Run();
        var setupCarriagesECB = new EntityCommandBuffer(Allocator.Persistent);

        var SetupCarriagesJob = new SetupCarriagesJob
        {
            ECB = setupCarriagesECB,
            EM = state.EntityManager
        };

        SetupCarriagesJob.Run();
        setupCarriagesECB.Playback(state.EntityManager);

        var seatsECB = new EntityCommandBuffer(Allocator.Persistent);
        var SetupSeatsJob = new SetupTrainSeatsJob()
        {
            EM = state.EntityManager
        };
        SetupSeatsJob.Run();
        seatsECB.Playback(state.EntityManager);


        seatsECB.Dispose();
        setupTrainsECB.Dispose();
        setupCarriagesECB.Dispose();
        state.Enabled = false;
    }
}
