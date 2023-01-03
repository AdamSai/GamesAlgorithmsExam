using Unity.Entities;

namespace DOTS.Components
{
    public struct MetroLineComponent : IComponentData
    {
        public byte MetroLineID;
        public Entity railPrefab;
    }
}