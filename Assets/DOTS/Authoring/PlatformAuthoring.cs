using DOTS.Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace DOTS.Authoring
{
    public class PlatformAuthoring : MonoBehaviour
    {
    }

    public class PlatformBaker : Baker<PlatformAuthoring>
    {
        public override void Bake(PlatformAuthoring authoring)
        {
            AddComponent(new PlatformComponent
            {
                neighborPlatforms = new NativeList<Entity>(10, Allocator.Persistent)
            });
            AddComponent(new OppositePlatformComponent());
        }
    }
}