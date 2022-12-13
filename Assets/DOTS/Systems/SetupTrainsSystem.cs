using DOTS.Authoring;
using DOTS.Components;
using System.Diagnostics;
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
        //Spawn Trains for each line
        //Add Index to train and its Line Index
        //Calculate spacing between each train for each line
        //float trainSpacing = 1 / maxtrains; 
        //Setup total Carriages

        state.Enabled = false;
    }
}

public partial struct SetupTrainsJob : IJobEntity
{
    public EntityCommandBuffer ECB;
    public void Execute(MetroLineTrainDataComponent MLTDC, MetroLineComponent MLA)
    {
        float trainSpacing = 1 / MLTDC.maxTrains;

        for (int i = 0; i < MLTDC.maxTrains; i++)
        {
            //Spawn empty Entity
            //Add TrainIDComponent, TrainData and State

            UnityEngine.Debug.Log("ECB: " + ECB);
            Entity train = ECB.Instantiate(MLTDC.trainPrefab);

            ECB.SetComponent(train, new TrainIDComponent
            {
                LineIndex = MLA.MetroLineID,
                TrainIndex = i
            });

            float pos = trainSpacing * i;
            ECB.SetComponent(train, new TrainDataComponent
            {
                Speed = MLTDC.maxTrainSpeed,
                Position = pos
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
    public void Execute(MetroLineTrainDataComponent MLTDC)
    {
        int amountOfCarriages = MLTDC.carriages;

        for (int i = 0; i < amountOfCarriages; i++)
        {
            //ECB.Instantiate()
            //Set Colour to Line Colour
        }
        //Spawn Carriages
    }
}

