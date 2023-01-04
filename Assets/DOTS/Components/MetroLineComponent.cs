using Unity.Entities;

namespace DOTS.Components
{
    public struct MetroLineComponent : IComponentData
    {
        public byte MetroLineID;
        public float SpeedRatio;
        public Entity railPrefab;
        public Entity platformPrefab;
    }
}