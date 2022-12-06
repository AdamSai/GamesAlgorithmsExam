using Unity.Entities;

namespace DOTS.Components
{
    public struct MetroLineTrainDataComponent : IComponentData
    {
        public byte maxTrains;
        public byte carriages;
        public float maxTrainSpeed;
    }
}