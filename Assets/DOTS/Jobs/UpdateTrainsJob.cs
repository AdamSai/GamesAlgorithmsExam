using System.Linq;
using DOTS.Components;
using DOTS.Components.Train;
using DOTS.Utility;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DOTS.Jobs
{
    public partial struct UpdateTrainStatesJob : IJobEntity
    {
        public ComponentLookup<TrainPositionComponent> trainsPositions;
        public ComponentLookup<TrainIDComponent> trainIDs;
        public ComponentLookup<PlatformComponent> platforms;
        public ComponentLookup<MetroLineComponent> metroLineComponents;
        
        public NativeArray<BezierPathComponent> bezierPathComponents;

        public NativeArray<Entity> platformEntities;
        public BufferLookup<BezierPoint> bezierLookup;
        public NativeArray<Entity> metroLines;


        public void Execute(in Entity entity, ref TrainStateComponent TSC, in TrainAheadComponent trainAheadOfMeComp,
            in AmountOfTrainsInLineComponent maxTrains, ref TrainSpeedComponent speed,
            in MaxTrainSpeedComponent maxTrainSpeed, ref TrainsNextPlatformComponent nextPlatformComponent)
        {
            float accelerationStrength = 0.000001f;
            var trainID = trainIDs.GetRefRO(entity).ValueRO;
            DynamicBuffer<BezierPoint> bezierBuffer = default;
            BezierPathComponent bezierPath = default;

            foreach (var metroLine in metroLines)
            {
                if (metroLineComponents.GetRefRO(metroLine).ValueRO.MetroLineID ==  trainID.LineIndex)
                {
                    bezierBuffer = bezierLookup[metroLine];
                    break;
                }
            }

            foreach (var bezier in bezierPathComponents)
            {
                if (bezier.MetroLineID == trainID.LineIndex)
                {
                    bezierPath = bezier;
                }
            }

            var currentPos = trainsPositions.GetRefRO(entity).ValueRO.value;
            // Set platform if none exists

            if (nextPlatformComponent.value == default)
            {
                Debug.Log("Value null");
                UpdatePlatform(ref nextPlatformComponent, bezierBuffer, currentPos, entity);
            }
            var nextPlatform = platforms.GetRefRW(nextPlatformComponent.value, false).ValueRO;

            switch (TSC.value)
            {
                case TrainStateDOTS.EN_ROUTE:
                    // Debug.Log("En route");
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
                    // Debug.Log("Region " + BezierUtility.GetRegionIndex(currentPos, bezierBuffer) + " " + nextPlatform.point_platform_START.index);
                    if (BezierUtility.GetRegionIndex(currentPos, bezierBuffer) ==
                        nextPlatform.point_platform_START.index)
                    {
                        TSC.value = TrainStateDOTS.ARRIVING;
                        speed.speedOnPlatformArriving =
                            math.clamp(speed.speed, maxTrainSpeed.value * 0.1f, maxTrainSpeed.value);
                    }

                    break;

                case TrainStateDOTS.ARRIVING:
                    Debug.Log("Arriving");
                    var _platform_start = nextPlatform.point_platform_START.distanceAlongPath;
                    float _platform_end = nextPlatform.point_platform_END.distanceAlongPath;
                    float _platform_length = _platform_end - _platform_start;
                    float arrivalProgress =
                        (BezierUtility.Get_proportionAsDistance(currentPos, bezierPath.distance) -
                         _platform_start) /
                        _platform_length;

                    arrivalProgress = 1f - math.cos(arrivalProgress * math.PI * 0.5f);
                    speed.speed = speed.speedOnPlatformArriving * (1f - arrivalProgress);

                    if (arrivalProgress >= 0.975f) // see Metro.PLATFORM_ARRIVAL_THRESHOLD
                    {
                        // ===== CHANGE STATE =====
                        TSC.value = TrainStateDOTS.DOORS_OPEN;
                        speed.speed = 0f;
                    }


                    //Change State
                    //Set Speed to Speed on Arrival

                    break;

                case TrainStateDOTS.DOORS_OPEN:
                    Debug.Log("Doors open");
                    // This should just open the doors, and then move on to the next state
                    // ===== CHANGE STATE =====
                    TSC.value = TrainStateDOTS.UNLOADING;
                    //TODO: PrepareDisembark

                    break;

                case TrainStateDOTS.UNLOADING:
                    Debug.Log("Unloading");
                    // TODO: If Passengers_To_Disembark is 0, change to loading state
                    //If Passengers to Disembark are 0, change state to Loading
                    // ===== CHANGE STATE =====
                    TSC.value = TrainStateDOTS.LOADING;
                    // TODO: Prepare_EMbark
                    break;

                case TrainStateDOTS.LOADING:
                    Debug.Log("Loading");
                    // If passengers_to_embark is 0 change state
                    // ===== CHANGE STATE =====
                    TSC.value = TrainStateDOTS.DOORS_CLOSE;
                    // TODO: Empty passengers to embark and disembark lists
                    break;
                case TrainStateDOTS.DOORS_CLOSE:
                    Debug.Log("Doors close");
                    // TODO: Doors actually close ?

                    // ===== CHANGE STATE =====
                    UpdatePlatform(ref nextPlatformComponent, bezierBuffer, currentPos, entity);
                    TSC.value = TrainStateDOTS.DEPARTING;
                    // stateDelay = Metro.INSTANCE.Train_delay_departure;


                    break;
                case TrainStateDOTS.DEPARTING:
                    Debug.Log("Departing");
                    // ===== CHANGE STATE =====
                    TSC.value = TrainStateDOTS.EN_ROUTE;

                    // Get_NextPlatform
                    //1 Second Delay
                    //Update Next Platform train needs to go to

                    break;
                case TrainStateDOTS.EMERGENCY_STOP:
                    Debug.LogError("Departing");

                    break;

                default:
                    break;
            }
        }

        private void UpdatePlatform(ref TrainsNextPlatformComponent nextPlatformComponent,
            DynamicBuffer<BezierPoint> bezierBuffer,
            float currentPos, Entity trainEntity)
        {
            int totalPoints = bezierBuffer.Length;
            int currentRegionIndex = BezierUtility.GetRegionIndex(currentPos, bezierBuffer);

            for (int i = 0; i < totalPoints; i++)
            {
                var testIndex = (currentRegionIndex + i) % totalPoints;
                foreach (var platformEnt in platformEntities)
                {
                    var platform = platforms.GetRefRO(platformEnt).ValueRO;
                    if (platform.point_platform_START.index == testIndex &&
                        !nextPlatformComponent.value.Equals(platformEnt))
                    {
                        nextPlatformComponent.value = platformEnt;
                        // TODO: Maybe this will mess up if a current train is in the nextPlatform and we overwrite it.
                        platforms.GetRefRW(platformEnt, false).ValueRW.currentTrain = trainEntity;
                        // TODO; Also add Train to platform
                    }
                }
            }
        }
    }
}