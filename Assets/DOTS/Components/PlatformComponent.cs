using System.Collections;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct PlatformComponent : IComponentData
{
    public NativeArray<Entity> neighborPlatforms; // Platforms that commuter can walk to
    public Entity oppositePlatform; // Opposite platform, perhaps obsolete
    public Entity nextPlatform; // Next station
    public float3 platform_entrance2; // Foot of the stairs, closest to platform: commuters walk here
    public float3 platform_entrance1; // Top of the stairs, middle point: commuters walk here
    public float3 platform_entrance0; // Entrance to the platform from the outside: commuters walk here
    public float3 carriage_entrance; // Entrance to the platform from the carriages
    public DOTS.BezierPoint point_platform_START; // Start of the platform on the bezier
    public DOTS.BezierPoint point_platform_END; // End of the platform on the bezier
    public byte carriageCount; // Number of carriages
    public int platformIndex; // Number of carriages
    
}