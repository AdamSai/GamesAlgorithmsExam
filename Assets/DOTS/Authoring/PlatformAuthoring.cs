using Assets.DOTS.Components;
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
                neighborPlatforms = new NativeList<Entity>( Allocator.Persistent),
                init = false
            });
            AddComponent(new OppositePlatformComponent());
            AddComponent(new NavTag { init = false });
        }
    }
}