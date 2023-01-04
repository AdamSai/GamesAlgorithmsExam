using Unity.Entities;
using Unity.Mathematics;

namespace DOTS.Components
{
    public struct OppositePlatformComponent : IComponentData
    {
        public Entity OppositePlatform;
        public float3 EulorRotation;
    }
}