using Unity.Entities;

namespace DOTS.Components
{
    public struct CarriageIDComponent : IComponentData
    {
        public int id;
        public int lineIndex;
        public int trainIndex;
    }
}