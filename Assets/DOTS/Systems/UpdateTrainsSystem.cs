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

//[WithAll(typeof(TrainTag))]
//public partial struct UpdateTrainCarriagesJob : IJobEntity
//{
//    public void Execute(Entity ent, )
//    {
//        //Get MetroLine entity
//        //for (var i = 0; i < metroLines.Length; i++)
//        //{
//        //    if (EM.GetComponentData<MetroLineComponent>(metroLines[i]).MetroLineID == carriageIDComponent.lineIndex)
//        //    {
//        //        metroLine = metroLines[i];
//        //        break;
//        //    }
//        //}
//        //Update Each Train
//        //Get All Carriages From that Train
//    }
//}
public partial struct UpdateTrainStatesJob : IJobEntity
{
    public void Execute(TrainStateComponent TSC)
    {
        switch (TSC.value)
        {
            case TrainStateDOTS.EN_ROUTE:
                break;

            case TrainStateDOTS.ARRIVING:
                //Change State
                {
                    //Set Speed to Speed on Arrival
                }

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

                //1 Second Delay
                //Update Next Platform train needs to go to

                break;
            case TrainStateDOTS.EMERGENCY_STOP:
                break;

            default:
                break;
        }
    }

    //void ChangeState(TrainStateDOTS _newState)
    //{

    //    switch (_newState)
    //    {
    //        case TrainStateDOTS.EN_ROUTE:
    //            // keep current speed
    //            break;
    //        case TrainStateDOTS.ARRIVING:
    //            //float maxSpeed = parentLine.maxTrainSpeed;
    //            //speed_on_platform_arrival = Mathf.Clamp(speed, maxSpeed * 0.1f, maxSpeed);
    //            break;
    //        case TrainStateDOTS.DOORS_OPEN:
    //            // slight delay, then open the required door
    //            speed = 0f;
    //            stateDelay = Metro.INSTANCE.Train_delay_doors_OPEN;
    //            break;
    //        case TrainStateDOTS.UNLOADING:
    //            Prepare_DISEMBARK();
    //            break;
    //        case TrainStateDOTS.LOADING:
    //            Prepare_EMBARK();
    //            break;
    //        case TrainStateDOTS.DOORS_CLOSE:
    //            passengers_to_DISEMBARK.Clear();
    //            passengers_to_EMBARK.Clear();
    //            // once totalPassengers == (totalPassengers + (waitingToBoard - availableSpaces)) - shut the doors
    //            stateDelay = Metro.INSTANCE.Train_delay_doors_CLOSE;
    //            // sort out vars for next stop (nextPlatform, door side, passengers wanting to get off etc)
    //            break;
    //        case TrainStateDOTS.DEPARTING:
    //            // Determine next platform / station we'll be stopping at
    //            Update_NextPlatform();
    //            // slight delay
    //            stateDelay = Metro.INSTANCE.Train_delay_departure;
    //            // get list of passengers who wish to depart at the next stop
    //            break;
    //        case TrainStateDOTS.EMERGENCY_STOP:
    //            break;
    //    }
    //}
}


//public partial struct UpdateCarriagesJob : IJobEntity
//{
//    public void Execute()
//    {
//        //Update Position and look at
//    }
//}