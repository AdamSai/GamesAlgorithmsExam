using DOTS.Components;
using Unity.Entities;

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

        var carriagesColorJob = new SetupCarriagesColorJob
        {
            EM = state.EntityManager
        };
        state.Dependency = carriagesColorJob.Schedule(state.Dependency);

        state.Dependency.Complete();

    }
    [WithAll(typeof(CarriageTag))]
    public partial struct SetupCarriagesColorJob : IJobEntity
    {
        public EntityManager EM;
        public void Execute(in Entity ent, ref ColorComponent carriageColor, ref EnableComponent enable)
        {
            if (enable.value)
                return;

            var children = EM.GetBuffer<LinkedEntityGroup>(ent);
            //Child

            for (int i = 0; i < children.Length; i++)
            {
                // if (EM.GetComponentData<CarriageColorTag>(children[i].Value))
                // {
                // }
            }

            enable.value = true;
        }
    }
}