using Unity.Entities;
using Unity.Mathematics;

namespace DOTS.Components
{
    public struct ColorComponent : IComponentData
    {
        public float4 value;
    }
}