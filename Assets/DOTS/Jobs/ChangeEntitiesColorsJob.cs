using DOTS.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

namespace DOTS.Jobs
{
    [WithAny(typeof(CarriageTag), (typeof(PlatformTag)))]
    [BurstCompile]
    public partial struct ChangeEntitiesColorsJob : IJobEntity
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