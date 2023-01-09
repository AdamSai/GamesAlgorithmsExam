using DOTS.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace DOTS.Jobs
{
    [BurstCompile]
    public partial struct CarriageParentJob : IJobEntity
    {
        public EntityCommandBuffer ECB;
        public ComponentLookup<CarriageIDComponent> carriageLookUp;
        public NativeList<Entity> carriages;
        //public ComponentLookup<CarriageIDComponent> carriages;
        public void Execute(in Entity ent, TrainIDComponent trainID, EnableComponent enable)
        {
            if (enable.value)
                return;

            //var platformToAdd = carriages.GetRefRO(_platform).ValueRO;
            for (int i = 0; i < carriages.Length; i++)
            {
                var _CA_ENT = carriages[i];
                var _PA = carriageLookUp.GetRefRO(_CA_ENT).ValueRO;

                if (trainID.LineIndex != _PA.lineIndex) return;
                if (trainID.TrainIndex != _PA.trainIndex) return;
                ECB.AddComponent(_CA_ENT, new Parent { Value = ent });
            }

            enable.value = true;
        }
    }
}