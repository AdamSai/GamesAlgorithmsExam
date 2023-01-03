using DOTS.Components;
using Unity.Entities;

namespace DOTS.Authoring
{
    public class MetroLineTrainDataAuthoring : UnityEngine.MonoBehaviour
    {
        public byte MaxTrains;
        public float MaxTrainSpeed;
        public float Friction;
        public UnityEngine.GameObject TrainPrefab;
    }

    public class MetroLineTrainDataBaker : Baker<MetroLineTrainDataAuthoring>
    {
        public override void Bake(MetroLineTrainDataAuthoring authoring)
        {
            AddComponent(new MetroLineTrainDataComponent
            {
                maxTrains = authoring.MaxTrains,
                maxTrainSpeed = authoring.MaxTrainSpeed,
                friction = authoring.Friction,
                trainPrefab = GetEntity(authoring.TrainPrefab)
            });
        }
    }
}