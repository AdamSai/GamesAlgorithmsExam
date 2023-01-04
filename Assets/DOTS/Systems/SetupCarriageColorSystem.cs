using DOTS.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

[UpdateAfter(typeof(SetupTrainsSystem))]
public partial struct SetupCarriageColorSystem : ISystem
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
        var carriagesColorJob = new SetupCarriagesColorJob
        {
            EM = state.EntityManager,
            ECB = ecb
        };

        state.Dependency = carriagesColorJob.Schedule(state.Dependency);
        state.Dependency.Complete();

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
}