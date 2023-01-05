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

        var carriageJob = new UpdateCarriageJob { trains = trains, ECB = ECB, metroLines = metroLines, EM = state.EntityManager };
        carriageJob.Run();

        // var dependency = JobHandle.CombineDependencies(trainJobHandle, state.Dependency);
        // state.Dependency = new UpdateCarriageJob {trains = trains}.Schedule(dependency);
    }
}


public partial struct UpdateCarriageJob : IJobEntity
{
    public EntityManager EM;
    public EntityCommandBuffer ECB;
    public NativeArray<Entity> trains;
    public NativeArray<Entity> metroLines;

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


        // Set initial speed and position
        var pos = EM.GetComponentData<TrainPositionComponent>(trainEntity).value;
        var Speed = EM.GetComponentData<TrainSpeedComponent>(trainEntity).speed;
        var Friction = EM.GetComponentData<TrainSpeedComponent>(trainEntity).friction;



        pos = ((pos += Speed) % 1f);
        Speed *= Friction; // TODO: See Train_railFriction on Metro.cs

        ECB.SetComponent(trainEntity, new TrainPositionComponent { value = pos });
        ECB.SetComponent(trainEntity, new TrainSpeedComponent()
        {
            friction = Friction,
            speed = Speed
        });

        // UpdateCarriages
        // Update position on the bezier
        carriagePos.value = pos;
        var bezier = EM.GetComponentData<BezierPathComponent>(metroLine);
        var posOnRail = BezierUtility.Get_Position(pos, bezier.distance, bezier.points);
        var rotOnRail = BezierUtility.Get_NormalAtPosition(pos, bezier.distance, bezier.points);

        // Set rotation and position
        var transform = LocalTransform.FromPosition(posOnRail);
        var rot = Quaternion.LookRotation(transform.Position - rotOnRail, Vector3.up);
        transform.Rotation = rot;
        ECB.SetComponent(ent, transform);
    }
}
