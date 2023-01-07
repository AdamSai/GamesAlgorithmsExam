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

        public float deltaTime;


        public void Execute(in Entity entity, ref TrainStateComponent TSC, in TrainAheadComponent trainAheadOfMeComp,
            in AmountOfTrainsInLineComponent maxTrains, ref TrainSpeedComponent speed,
            in MaxTrainSpeedComponent maxTrainSpeed, ref TrainsNextPlatformComponent nextPlatformComponent,
            ref TimerComponent timer)
        {
            // float accelerationStrength = 0.000001f;
            float accelerationStrength = 0.001f;
            var trainID = trainIDs.GetRefRO(entity).ValueRO;
            DynamicBuffer<BezierPoint> bezierBuffer = default;
            BezierPathComponent bezierPath = default;


            foreach (var metroLine in metroLines)
            {
                if (metroLineComponents.GetRefRO(metroLine).ValueRO.MetroLineID == trainID.LineIndex)
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
                UpdatePlatform(ref nextPlatformComponent, bezierBuffer, currentPos, entity, bezierPath);
            }

            var nextPlatform = platforms.GetRefRW(nextPlatformComponent.value, false).ValueRO;

            switch (TSC.value)
            {
                case TrainStateDOTS.EN_ROUTE:
                    Debug.Log("State: En route");
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
                        Debug.Log("Too close ot train ahead, slowing down");
                        speed.speed *= 0.85f;
                    }

                    // Get platfrom from this

                    // ===== CHANGE STATE =====
                    if (Get_RegionIndex(currentPos, bezierPath, bezierBuffer) ==
                        nextPlatform.point_platform_START.index)
                    {
                        TSC.value = TrainStateDOTS.ARRIVING;
                        speed.speedOnPlatformArriving =
                            math.clamp(speed.speed, maxTrainSpeed.value * 0.1f, maxTrainSpeed.value);
                    }

                    break;

                case TrainStateDOTS.ARRIVING:
                    Debug.Log("State: Arriving");
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
                        timer.duration = 1f;
                        TSC.value = TrainStateDOTS.DOORS_OPEN;
                        speed.speed = 0f;
                    }


                    //Change State
                    //Set Speed to Speed on Arrival

                    break;

                case TrainStateDOTS.DOORS_OPEN:
                    Debug.Log("State: Doors open");
                    if (TimerUtility.RunTimer(ref timer, deltaTime))
                    {
                        // This should just open the doors, and then move on to the next state
                        // ===== CHANGE STATE =====
                        TSC.value = TrainStateDOTS.UNLOADING;
                        //TODO: PrepareDisembark
                    }

                    break;

                case TrainStateDOTS.UNLOADING:
                    Debug.Log("State: Unloading");
                    // TODO: If Passengers_To_Disembark is 0, change to loading state
                    //If Passengers to Disembark are 0, change state to Loading
                    // ===== CHANGE STATE =====
                    TSC.value = TrainStateDOTS.LOADING;
                    // TODO: Prepare_EMbark
                    break;

                case TrainStateDOTS.LOADING:
                    Debug.Log("State: Loading");
                    // If passengers_to_embark is 0 change state
                    // ===== CHANGE STATE =====
                    TSC.value = TrainStateDOTS.DOORS_CLOSE;
                    timer.duration = 1f;
                    // TODO: Empty passengers to embark and disembark lists
                    break;
                case TrainStateDOTS.DOORS_CLOSE:
                    Debug.Log("State: Doors close");
                    if (TimerUtility.RunTimer(ref timer, deltaTime))
                    {
                        // TODO: Doors actually close ?

                        // ===== CHANGE STATE =====
                        timer.duration = 2f;
                        UpdatePlatform(ref nextPlatformComponent, bezierBuffer, currentPos, entity, bezierPath);
                        TSC.value = TrainStateDOTS.DEPARTING;
                        // stateDelay = Metro.INSTANCE.Train_delay_departure;
                    }


                    break;
                case TrainStateDOTS.DEPARTING:
                    Debug.Log("State: Departing");
                    if (TimerUtility.RunTimer(ref timer, deltaTime))
                    {
                        // ===== CHANGE STATE =====
                        TSC.value = TrainStateDOTS.EN_ROUTE;
                    }
                    // Get_NextPlatform
                    //1 Second Delay
                    //Update Next Platform train needs to go to

                    break;
                case TrainStateDOTS.EMERGENCY_STOP:
                    Debug.LogError("State: Departing");

                    break;

                default:
                    break;
            }
        }

        private void UpdatePlatform(ref TrainsNextPlatformComponent nextPlatformComponent,
            DynamicBuffer<BezierPoint> bezierBuffer,
            float currentPos, Entity trainEntity, BezierPathComponent path)
        {
            int totalPoints = bezierBuffer.Length;
            int currentRegionIndex = Get_RegionIndex(currentPos, path, bezierBuffer);

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
                        Debug.Log("Update platform ahead");
                        
                        platforms.GetRefRW(platformEnt, false).ValueRW.currentTrain = trainEntity;
                        return;
                        // TODO; Also add Train to platform
                    }
                }
            }
        }
        
        public int Get_RegionIndex(float _proportion, BezierPathComponent path, DynamicBuffer<BezierPoint> bezierBuffer)
        {
            return BezierUtility.GetRegionIndex(BezierUtility.Get_proportionAsDistance(_proportion, path.distance), bezierBuffer);
        }

    }
}