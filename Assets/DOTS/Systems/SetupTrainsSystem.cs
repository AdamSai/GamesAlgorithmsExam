using DOTS.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
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
    EntityCommandBuffer ecb;
    public void OnCreate(ref SystemState state)
    {
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {
      
        state.Enabled = false;
    }
}

public partial struct SetupTrainsJob : IJobEntity
{
    public EntityCommandBuffer ECB;
    public void Execute(MetroLineTrainDataComponent MLTDC, MetroLineComponent MLA)
    {
        Debug.Log("Setup trains");
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
                value = MLTDC.maxTrainSpeed

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
    public void Execute(MetroLineCarriageDataComponent MLCarriage, MetroLineTrainDataComponent MLTrain, MetroLineComponent MLID)
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

public partial struct SetupCarriagesColorJob : IJobEntity
{
    public EntityCommandBuffer ECB;
    public void Execute(CarriageColorTag ccTag)
    {

        //ECB.AddComponent(ccTag, new URPMaterialPropertyBaseColor { Value = new float4(1, 0, 1, 1) });
    }
}

