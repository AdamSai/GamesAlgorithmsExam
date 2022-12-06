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
public partial struct SetupTrains : ISystem
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

        //ecb.Instantiate(new Entity());

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
    public void Execute(MetroLineTrainDataComponent MLTDC)
    {
        //float trainSpacing = 1 / maxtrains; 

        for (int i = 0; i < MLTDC.maxTrains; i++)
        {
            //Spawn empty Entity
            //Add TrainIDComponent, TrainData and State

            UnityEngine.Debug.Log("ECB: " + ECB);
            Entity train = ECB.Instantiate(MLTDC.entity);

            //Setup Index as I
            //Add Metro Line Index

            //Setup Position
        }
    }
}

public partial struct SpawnTrainJob : IJobEntity
{
    public float spacing;
    public void Execute()
    {
        //Set Initial Position
        //position = spacing * trainIndex


    }
}

public partial struct SetupCarriagesJob : IJobEntity
{
    public void Execute()
    {
        //Spawn Carriages
    }
}

