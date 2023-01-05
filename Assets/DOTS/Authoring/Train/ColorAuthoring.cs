using DOTS.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DOTS.Authoring
{

    public class ColorAuthoring : MonoBehaviour
    {
        public float4 Value;
    }

    public class ColorBaker : Baker<ColorAuthoring>
    {
        public override void Bake(ColorAuthoring authoring)
        {
            AddComponent(new ColorComponent { value = authoring.Value });
        }
    }
}