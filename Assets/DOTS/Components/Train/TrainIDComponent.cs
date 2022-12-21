using Unity.Entities;

namespace DOTS.Components
{
    public struct TrainIDComponent : IComponentData
    {
        public byte TrainIndex;
        public byte LineIndex;
    }
}