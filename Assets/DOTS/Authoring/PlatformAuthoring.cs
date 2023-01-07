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

            // Queue stuff
            AddComponent(new QueueComponent { 
                queue0 = new NativeList<Entity>(Allocator.Persistent),
                queue1 = new NativeList<Entity>(Allocator.Persistent),
                queue2 = new NativeList<Entity>(Allocator.Persistent),
                queue3 = new NativeList<Entity>(Allocator.Persistent),
                queue4 = new NativeList<Entity>(Allocator.Persistent),
            });
            AddBuffer<QueueEntryComponent>();
        }
    }
}