using Unity.Entities;

namespace DOTS.Components
{
    public class CarriageIDComponent : IComponentData
    {
        public int ID;
        public int LineIndex;
        public int TrainIndex;
    }
}