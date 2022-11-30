using System.Collections;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public struct PlatformComponent : IComponentData
{
    // Dummy component
    public NativeArray<Entity> neighborPlatforms;
}