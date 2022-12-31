using Unity.Entities;

public partial struct UpdateTrainsSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {

        //Update Trains State Job

        //Update Carriages Job

    }
}

public partial struct UpdateTrainStatesJob : IJobEntity
{
    public void Execute(TrainStateComponent TSC)
    {
        switch (TSC.value)
        {
            case TrainStateDOTS.EN_ROUTE:



                break;

            case TrainStateDOTS.ARRIVING:
                break;

            case TrainStateDOTS.DOORS_OPEN:
                break;

            case TrainStateDOTS.UNLOADING:
                break;

            case TrainStateDOTS.LOADING:
                break;
            case TrainStateDOTS.DOORS_CLOSE:
                break;
            case TrainStateDOTS.DEPARTING:
                break;
            case TrainStateDOTS.EMERGENCY_STOP:
                break;

            default:
                break;
        }
    }
}

public partial struct UpdateCarriagesJob : IJobEntity
{
    public void Execute()
    {
        //Update Position and look at
    }
}