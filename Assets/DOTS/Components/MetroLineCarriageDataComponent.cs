using Unity.Entities;

namespace DOTS.Components
{
    public struct MetroLineCarriageDataComponent : IComponentData
    {
        public byte carriages;
        public float carriagesSpeed;
        public Entity carriage;
    }
}