using Assets.DOTS.Components;
using Assets.DOTS.Components.Train;
using Assets.DOTS.Systems;
using DOTS.Components;
using DOTS.Components.Train;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

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

        SetupTrainAheadJob.Schedule();
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

public partial struct SetupTrainsJob : IJobEntity
{
    public EntityCommandBuffer ECB;

    public void Execute(MetroLineTrainDataComponent MLTDC, MetroLineComponent MLA)
    {
        Debug.Log("Setup trains");
        float trainSpacing = 1f / MLTDC.maxTrains;

        for (byte i = 0; i < MLTDC.maxTrains; i++)
        {
            //Spawn empty Entity
            //Add TrainIDComponent, TrainData and State

            Entity train = ECB.Instantiate(MLTDC.trainPrefab);
            ECB.SetComponent(train, new TrainIDComponent
            {
                LineIndex = MLA.MetroLineID,
                TrainIndex = i
            });

            float pos = trainSpacing * i;
            Debug.Log($"train pos {MLA.MetroLineID}: " + pos);
            ECB.SetName(train, $"Train_{MLA.MetroLineID}:{i}");
            ECB.SetComponent(train, new TrainPositionComponent
            {
                value = pos
            });

            ECB.SetComponent(train, new TrainSpeedComponent
            {
                speed = MLTDC.maxTrainSpeed,
                friction = MLTDC.friction
            });

            ECB.SetComponent(train, new MaxTrainSpeedComponent
            {
                value = MLTDC.maxTrainSpeed
            });

            ECB.SetComponent(train, new AmountOfTrainsInLineComponent
            {
                value = MLTDC.maxTrains
            });

            ECB.SetComponent(train, new TrainStateComponent
            {
                value = TrainStateDOTS.DEPARTING
            });

            ECB.AddComponent<TrainAheadComponent>(train);

            ECB.AddComponent<TimerComponent>(train);
        }
    }
}

public partial struct SetupCarriagesJob : IJobEntity
{
    public EntityCommandBuffer ECB;
    public EntityManager EM;

    public void Execute(MetroLineCarriageDataComponent MLCarriage, MetroLineTrainDataComponent MLTrain,
            in MetroLineComponent MLID, ColorComponent color)
        //public void Execute(MetroLineCarriageDataComponent MLCarriage, MetroLineTrainDataComponent MLTrain, MetroLineComponent MLID)
    {
        Debug.Log("Setup carriages");
        for (int i = 0; i < MLTrain.maxTrains; i++)
        {
            Entity previousCarriage = Entity.Null;
            for (int j = 0; j < MLCarriage.carriages; j++)
            {
                //Instantiate Carriages
                Entity carriage = ECB.Instantiate(MLCarriage.carriage);
                
                
                ECB.SetComponent(carriage, new CarriageIDComponent
                {
                    id = j,
                    trainIndex = i,
                    lineIndex = MLID.MetroLineID
                });

                ECB.SetComponent(carriage, new ColorComponent
                {
                    value = color.value
                });

                ECB.AddComponent(carriage, new CarriageAheadOfMeComponent
                {
                    Value = previousCarriage
                });
                
                ECB.AddComponent(carriage, new CarriagePassengerSeatsComponent
                {
                    init = false
                });
 

                previousCarriage = carriage;
            }
        }
    }
}

public partial struct SetupTrainAheadJob : IJobEntity
{
    public ComponentLookup<TrainIDComponent> trainIdLookup;
    public NativeArray<Entity> trains;

    public void Execute(in Entity entity, ref TrainAheadComponent trainAheadComponent,
        in AmountOfTrainsInLineComponent maxTrains)
    {
        foreach (var train in trains)
        {
            var trainID = trainIdLookup.GetRefRO(entity).ValueRO;
            var other = trainIdLookup.GetRefRO(train).ValueRO;
            if (other.LineIndex == trainID.LineIndex)
            {
                if (entity.Index == train.Index)
                    continue;

                if (trainID.TrainIndex == maxTrains.value - 1 && other.TrainIndex == 0)
                {
                    trainAheadComponent.Value = train;
                    return;
                }

                if (other.TrainIndex == trainID.TrainIndex + 1)
                {
                    trainAheadComponent.Value = train;
                    return;
                }
            }
        }
    }
}

[WithAll(typeof(CarriageIDComponent))]
public partial struct SetupTrainSeatsJob : IJobEntity
{
    public EntityManager EM;
    public void Execute(in Entity ent)
    {
        var children = EM.GetBuffer<LinkedEntityGroup>(ent);
        var seats = new NativeList<Entity>(Allocator.Persistent);
        for (var i = 0; i < children.Length; i++)
        {
            if (EM.HasComponent<CarriageSeatComponent>(children[i].Value))
            {
                seats.Add(children[i].Value);
            }
        }
        
        EM.SetComponentData(ent, new CarriagePassengerSeatsComponent
        {
            init = true,
            seats = seats
        });
    }
}