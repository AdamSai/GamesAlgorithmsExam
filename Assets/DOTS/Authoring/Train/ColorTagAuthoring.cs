using DOTS.Components;
using Unity.Entities;
using UnityEngine;

namespace DOTS.Authoring
{

    public class ColorTagAuthoring : MonoBehaviour
    {
    }

    public class ColorTagBaker : Baker<ColorTagAuthoring>
    {
        public override void Bake(ColorTagAuthoring authoring)
        {
            AddComponent(new CarriageColorTag { });
        }
    }
}