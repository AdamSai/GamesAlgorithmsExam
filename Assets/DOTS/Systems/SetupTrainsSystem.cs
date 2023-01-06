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
            Debug.Log($"train pos {MLA.MetroLineID}: "+ pos);
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
            
            ECB.AddComponent(train, new TrainAheadComponent());
        }
    }
}

public partial struct SetupCarriagesJob : IJobEntity
{
    public EntityCommandBuffer ECB;
    public EntityManager EM;

    public void Execute(MetroLineCarriageDataComponent MLCarriage, MetroLineTrainDataComponent MLTrain, in MetroLineComponent MLID, ColorComponent color)
    //public void Execute(MetroLineCarriageDataComponent MLCarriage, MetroLineTrainDataComponent MLTrain, MetroLineComponent MLID)
    {
        Debug.Log("Setup carriages");
        for (int i = 0; i < MLTrain.maxTrains; i++)
        {
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

                //var children = EM.GetBuffer<LinkedEntityGroup>(carriage);

                //for (int k = 0; k < children.Count(); k++)
                //{
                //Use Has Component
                //    if (!EM.GetComponentData<CarriageColorTag>(children[i].Value).Equals(null))
                //    {
                //        UnityEngine.Debug.Log("Carriage Color Tag Found On Line: " + MLID.MetroLineID);
                //    }
                //}

                //carr
                //var buffer = ECB.
                //Entity Carriage = ECB.Instantiate
                //ECB.Instantiate()
                //Set Colour to Line Colour
                //Set Colour to Carriages Material
            }
        }
    }
}

public partial struct SetupTrainAheadJob : IJobEntity
{
    public ComponentLookup<TrainIDComponent> trainIdLookup;
    public NativeArray<Entity> trains;

    public void Execute(in Entity entity, ref TrainAheadComponent trainAheadComponent, in AmountOfTrainsInLineComponent maxTrains)
    {
        foreach (var train in trains)
        {
            var trainID = trainIdLookup.GetRefRO(entity).ValueRO;
            var other = trainIdLookup.GetRefRO(train).ValueRO;
            if (other.LineIndex == trainID.LineIndex)
            {
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

