using DOTS.Components;
using Unity.Entities;
using UnityEngine;

namespace DOTS.Authoring
{

    public class PlatformTagAuthoring : MonoBehaviour
    {
    }

    public class PlatformTagBaker : Baker<PlatformTagAuthoring>
    {
        public override void Bake(PlatformTagAuthoring authoring)
        {
            AddComponent(new PlatformTag { });
        }
    }
}