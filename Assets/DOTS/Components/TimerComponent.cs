using Unity.Entities;

namespace DOTS.Components
{
    public struct TimerComponent : IComponentData
    {
        public float time;
        public float duration;
        public bool isRunning;
    }
}