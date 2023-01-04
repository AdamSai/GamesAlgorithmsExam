using DOTS.Components;
using Unity.Entities;
using UnityEngine;

namespace DOTS.Authoring
{

    public class MetroLineTagAuthoring : MonoBehaviour
    {
    }

    public class MetroLineTagBaker : Baker<MetroLineTagAuthoring>
    {
        public override void Bake(MetroLineTagAuthoring authoring)
        {
            AddComponent(new MetroLineTag { });
        }
    }
}