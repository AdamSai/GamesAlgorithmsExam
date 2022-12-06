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
public struct SetupTrains : ISystem
{
    public void OnCreate(ref SystemState state)
    {
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {

        //Spawn Trains for each line
        //Add Index to train and its Line Index
        //Calculate spacing between each train 
        //float trainSpacing = 1 / maxtrains; 

        //Setup Carriages
        state.Enabled = false;
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


