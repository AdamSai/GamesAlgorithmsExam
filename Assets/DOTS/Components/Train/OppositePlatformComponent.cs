using DOTS.Jobs;
using Unity.Entities;
using Unity.Mathematics;

namespace DOTS.Components
{
    public struct OppositePlatformComponent : IComponentData
    {
        public EntityWithRotation OppositePlatform;
        public float3 EulorRotation;
    }
}