using Unity.Entities;

namespace DOTS.Components.Train
{
    public struct CarriageAheadOfMeComponent : IComponentData
    {
        public Entity Value;
    }
}