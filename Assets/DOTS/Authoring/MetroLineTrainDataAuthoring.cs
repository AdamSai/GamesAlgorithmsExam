using DOTS.Components;
using Unity.Entities;

namespace DOTS.Authoring
{
    public class MetroLineTrainDataAuthoring : UnityEngine.MonoBehaviour
    {
        public byte MaxTrains;
        public byte Carriages;
        public float MaxTrainSpeed;
        public float carriagesSpeed;
        public UnityEngine.GameObject entity;
    }

    public class MetroLineTrainDataBaker : Baker<MetroLineTrainDataAuthoring>
    {
        public override void Bake(MetroLineTrainDataAuthoring authoring)
        {
            AddComponent(new MetroLineTrainDataComponent
            {
                maxTrains = authoring.MaxTrains,
                carriages = authoring.Carriages,
                maxTrainSpeed = authoring.MaxTrainSpeed,
                carriagesSpeed = authoring.carriagesSpeed,
                entity = GetEntity(authoring.entity)
            });
        }
    }
}