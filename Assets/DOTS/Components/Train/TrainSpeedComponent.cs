using Unity.Entities;

namespace DOTS.Components
{
    public struct TrainSpeedComponent : IComponentData
    {
        public float speed;
        public float speedOnPlatformArriving;
        public float friction;
    }
}
