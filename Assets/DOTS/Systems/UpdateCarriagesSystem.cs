using DOTS.Components;
using DOTS.Utility;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

// Run the system after  SetupTrainsSystem
[UpdateAfter(typeof(SetupTrainsSystem))]
public partial struct UpdateCarriagesSystem : ISystem
{
    private EntityQuery trainQuery;
    private EntityQuery metroLineQuery;
    private EntityCommandBuffer ECB;

    public void OnCreate(ref SystemState state)
    {
        trainQuery =
            new EntityQueryBuilder(Allocator.Temp).WithAll<TrainPositionComponent, TrainIDComponent, TrainSpeedComponent>().Build(ref state);
        metroLineQuery =
            new EntityQueryBuilder(Allocator.Temp).WithAll<BezierPathComponent, MetroLineComponent>().Build(ref state);
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {
        ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        var trains =
            trainQuery.ToEntityArray(Allocator.Persistent);


        var metroLines =
            metroLineQuery.ToEntityArray(Allocator.Persistent);

        var trainJob = new UpdateTrainsPositionsJob { deltaTime = SystemAPI.Time.DeltaTime };
        state.Dependency = trainJob.Schedule(state.Dependency);
        state.Dependency.Complete();

        var carriageJob = new UpdateCarriageJob
        {
            trains = trains,
            ECB = ECB,
            metroLines = metroLines,
            EM = state.EntityManager,
            tPos = state.GetComponentLookup<TrainPositionComponent>()
        };
        state.Dependency = carriageJob.Schedule(state.Dependency);
        state.Dependency.Complete();

        // var dependency = JobHandle.CombineDependencies(trainJobHandle, state.Dependency);
        // state.Dependency = new UpdateCarriageJob {trains = trains}.Schedule(dependency);
    }
}

public partial struct UpdateTrainsPositionsJob : IJobEntity
{
    public float deltaTime;

    public void Execute(in Entity ent, ref TrainPositionComponent tpos, in TrainSpeedComponent speed)
    {
        // Set initial speed and position
        var pos = tpos.value;
        var Speed = speed.speed;
        var Friction = speed.friction;

        pos = ((pos += Speed * deltaTime) % 1f);
        Speed *= Friction; // TODO: See Train_railFriction on Metro.cs

        tpos.value = pos;
    }
}

public partial struct UpdateCarriageJob : IJobEntity
{
    public EntityManager EM;
    public EntityCommandBuffer ECB;
    public NativeArray<Entity> trains;
    public NativeArray<Entity> metroLines;
    public ComponentLookup<TrainPositionComponent> tPos;

    public void Execute(Entity ent, CarriageIDComponent carriageIDComponent, ref CarriagePositionComponent carriagePos)
    {

        Entity trainEntity = default;
        Entity metroLine = default;

        // Find the correct Train
        for (var i = 0; i < trains.Length; i++)
        {
            if (EM.GetComponentData<TrainIDComponent>(trains[i]).LineIndex == carriageIDComponent.lineIndex)
            {
                trainEntity = trains[i];
                break;
            }
        }

        // Find the correct MetroLine
        for (var i = 0; i < metroLines.Length; i++)
        {
            if (EM.GetComponentData<MetroLineComponent>(metroLines[i]).MetroLineID == carriageIDComponent.lineIndex)
            {
                metroLine = metroLines[i];
                break;
            }
        }

        // UpdateCarriages
        // Update position on the bezier
        var bezier = EM.GetComponentData<BezierPathComponent>(metroLine);
        float carriageOffset = 10f;
        float pos = tPos[trainEntity].value + carriageIDComponent.id * carriageOffset / bezier.distance;

        carriagePos.value = pos;
        var posOnRail = BezierUtility.Get_Position(pos, bezier.distance, bezier.points);
        var rotOnRail = BezierUtility.Get_NormalAtPosition(pos, bezier.distance, bezier.points);

        // Set rotation and position
        var transform = LocalTransform.FromPosition(posOnRail);
        var rot = Quaternion.LookRotation(transform.Position - (transform.Position - rotOnRail), Vector3.up);
        transform.Rotation = rot;
        ECB.SetComponent(ent, transform);
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
    }
}
