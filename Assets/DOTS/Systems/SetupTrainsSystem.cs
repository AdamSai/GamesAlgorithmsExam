using DOTS.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

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
public partial struct SetupTrainsSystem : ISystem
{
    EntityCommandBuffer ecb;
    public void OnCreate(ref SystemState state)
    {
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {
        ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        var SetupTrainsJob = new SetupTrainsJob
        {
            ECB = ecb
        };
        SetupTrainsJob.Run();

        var SetupCarriagesJob = new SetupCarriagesJob
        {
            ECB = ecb,
            EM = state.EntityManager
        };

        SetupCarriagesJob.Run();

        var entityArray = new EntityQueryBuilder(Allocator.Temp)
        .WithAny<CarriageColorTag>().Build(ref state).ToEntityArray(Allocator.Temp);

        UnityEngine.Debug.Log("Entity Array size: " + entityArray.Length);


        for (int i = 0; i < entityArray.Length; i++)
        {
            ecb.AddComponent(entityArray[i], new URPMaterialPropertyBaseColor { Value = new float4(0, 0, 1, 1) });
            UnityEngine.Debug.Log("Added Color to: " + entityArray[i].ToString());
        }
        state.Enabled = false;
    }
}

public partial struct SetupTrainsJob : IJobEntity
{
    public EntityCommandBuffer ECB;
    public void Execute(MetroLineTrainDataComponent MLTDC, in MetroLineComponent MLA)
    {
        float trainSpacing = 1 / MLTDC.maxTrains;

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
            ECB.SetComponent(train, new TrainPositionComponent
            {
                value = pos
            });

            ECB.SetComponent(train, new TrainSpeedComponent
            {
                speed = MLTDC.maxTrainSpeed,
                friction = MLTDC.friction

            });

            ECB.SetComponent(train, new TrainStateComponent
            {
                value = TrainStateDOTS.DEPARTING
            });
        }
    }
}

public partial struct SetupCarriagesJob : IJobEntity
{
    public EntityCommandBuffer ECB;
    public EntityManager EM;

    public void Execute(MetroLineCarriageDataComponent MLCarriage, MetroLineTrainDataComponent MLTrain, in MetroLineComponent MLID)
    {
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
