using Unity.Entities;

namespace DOTS.Components
{
    public struct MetroLineTrainDataComponent : IComponentData
    {
        public byte maxTrains;
        public float maxTrainSpeed;
        public float friction;
        public Entity trainPrefab;
    }
}