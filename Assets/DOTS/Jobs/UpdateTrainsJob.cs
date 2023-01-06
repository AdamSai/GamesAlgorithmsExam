using System.Linq;
using DOTS.Components;
using DOTS.Components.Train;
using DOTS.Utility;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace DOTS.Jobs
{
   public partial struct UpdateTrainStatesJob : IJobEntity
    {
        public ComponentLookup<TrainPositionComponent> trainsPositions;
        public ComponentLookup<TrainIDComponent> trainIDs;
        public ComponentLookup<PlatformComponent> platforms;
        public NativeList<BezierPathComponent> bezierPaths;

        public NativeList<Entity> platformEntities;

        public void Execute(in Entity entity, ref TrainStateComponent TSC, in TrainAheadComponent trainAheadOfMeComp,
            in AmountOfTrainsInLineComponent maxTrains, ref TrainSpeedComponent speed,
            in MaxTrainSpeedComponent maxTrainSpeed, ref TrainsNextPlatformComponent nextPlatformComponent)
        {
            float accelerationStrength = 0.000001f;
            var trainID = trainIDs.GetRefRO(entity).ValueRO;
            var bezierPath = bezierPaths.First(x => x.MetroLineID == trainID.LineIndex);
            var currentPos = trainsPositions.GetRefRO(entity).ValueRO.value;
            var nextPlatform = platforms.GetRefRW(nextPlatformComponent.value, false);

            switch (TSC.value)
            {
                case TrainStateDOTS.EN_ROUTE:
                    var trainAheadOfMe = trainAheadOfMeComp.Value;
                    // TODO: Maybe make RW?
                    float trainAhead_stopPoint = trainsPositions.GetRefRO(trainAheadOfMe).ValueRO.value;

                    int index = trainID.TrainIndex;
                    int trainAheadIndex = trainIDs.GetRefRO(trainAheadOfMe).ValueRO.TrainIndex;

                    if (trainAheadIndex < index)
                    {
                        trainAhead_stopPoint += 1f;
                    }

                    float distanceToTrainAhead = math.abs(trainAhead_stopPoint - currentPos);
                    if (distanceToTrainAhead > 0.05f || maxTrains.value == 1)
                    {
                        if (speed.speed <= maxTrainSpeed.value)
                        {
                            speed.speed += accelerationStrength;
                        }
                    }
                    else
                    {
                        speed.speed *= 0.85f;
                    }

                    // Get platfrom from this

                    // ===== CHANGE STATE =====
                    if (BezierUtility.GetRegionIndex(currentPos, bezierPath.points) ==
                        nextPlatform.ValueRO.point_platform_START.index)
                    {
                        TSC.value = TrainStateDOTS.ARRIVING;
                        speed.speedOnPlatformArriving =
                            math.clamp(speed.speed, maxTrainSpeed.value * 0.1f, maxTrainSpeed.value);
                    }
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

                    //If Passengers to Disembark are 0, change state to Loading
                    TSC.value = TrainStateDOTS.LOADING;
                    break;

                case TrainStateDOTS.LOADING:
                    break;
                case TrainStateDOTS.DOORS_CLOSE:
                    // TODO: Doors actually close ?
                    UpdatePlatform(ref nextPlatformComponent, bezierPath, currentPos);
                    TSC.value = TrainStateDOTS.DEPARTING;
                    // stateDelay = Metro.INSTANCE.Train_delay_departure;

                
                    break;
                case TrainStateDOTS.DEPARTING:
                    TSC.value = TrainStateDOTS.EN_ROUTE;
                    
                    // Get_NextPlatform
                    //1 Second Delay
                    //Update Next Platform train needs to go to

                    break;
                case TrainStateDOTS.EMERGENCY_STOP:
                    break;

                default:
                    break;
            }
        }

        private void UpdatePlatform(ref TrainsNextPlatformComponent nextPlatformComponent, BezierPathComponent bezierPath,
            float currentPos)
        {
            int totalPoints = bezierPath.points.Length;
            int currentRegionIndex = BezierUtility.GetRegionIndex(currentPos, bezierPath.points);

            for (int i = 0; i < totalPoints; i++)
            {
                var testIndex = (currentRegionIndex + i) % totalPoints;
                foreach (var platformEnt in platformEntities)
                {
                    var platform = platforms.GetRefRO(platformEnt).ValueRO;
                    if (platform.point_platform_START.index == testIndex && !nextPlatformComponent.value.Equals(platformEnt))
                    {
                        nextPlatformComponent.value = platformEnt;
                    }
                }
            }
        }
    }
}