using DOTS.Components;
using Unity.Entities;
using UnityEngine;

namespace DOTS.Authoring
{

    public class TrainsNextPlatformAuthoring : MonoBehaviour
    {
    }

    public class TrainsNextPlatformBaker : Baker<TrainsNextPlatformAuthoring>
    {
        public override void Bake(TrainsNextPlatformAuthoring authoring)
        {
            AddComponent(new TrainsNextPlatformComponent { });
        }
    }
}