using DOTS.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

[UpdateAfter(typeof(SetupTrainsSystem))]
public partial struct SetupCarriageColorSystem : ISystem
{
    EntityCommandBuffer ecb;
    private EntityQuery carriageQuery;

    public void OnCreate(ref SystemState state)
    {
        carriageQuery =
         new EntityQueryBuilder(Allocator.Temp).WithAll<CarriageIDComponent>().Build(ref state);
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {
        ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        //var carriagesColorJob = new SetupCarriagesColorJob
        //{
        //    EM = state.EntityManager,
        //    ECB = ecb
        //};

        //var carriagesJob = carriagesColorJob.Schedule(state.Dependency);


        //var carriages =
        //    carriageQuery.ToComponentDataListAsync<CarriageIDComponent>(Allocator.Persistent,
        //        out var railMarkerJobHandle);

        ////var carriageParentJob = new CarriageParentJob
        ////{
        ////    EM = state.EntityManager,
        ////    carriages = carriages
        ////};
        ////var jobHandle = JobHandle.CombineDependencies(carriagesJob, state.Dependency);

        ////state.Dependency = carriageParentJob.Schedule(jobHandle);
        //state.Dependency.Complete();
    }

    [WithAll(typeof(CarriageTag))]
    public partial struct SetupCarriagesColorJob : IJobEntity
    {
        public EntityManager EM;
        public EntityCommandBuffer ECB;
        public void Execute(in Entity ent, ref EnableComponent enable, in ColorComponent color)
        {
            if (enable.value)
                return;

            var children = EM.GetBuffer<LinkedEntityGroup>(ent);
            //Child

            for (int i = 0; i < children.Length; i++)
            {
                if (EM.HasComponent<ChangeColorTag>(children[i].Value))
                {
                    ECB.AddComponent(children[i].Value, new URPMaterialPropertyBaseColor { Value = new float4(color.value) });
                }
            }

            enable.value = true;
        }
    }

    [UpdateAfter(typeof(TrainTag))]
    public partial struct CarriageParentJob : IJobEntity
    {
        public EntityManager EM;
        public NativeList<CarriageIDComponent> carriages;
        public void Execute(in Entity ent, TrainIDComponent trainID)
        {
            for (int i = 0; i < carriages.Length; i++)
            {
                if (trainID.LineIndex != carriages[i].lineIndex) return;
                if (trainID.TrainIndex != carriages[i].trainIndex) return;
                //EM.AddComponentData(carriages[i], new Parent { Value = ent });
            }
        }
    }

}