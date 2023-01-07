using System.Collections;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct PlatformComponent : IComponentData
{
    public NativeList<Entity> neighborPlatforms; // Platforms that commuter can walk to
    public Entity oppositePlatform; // Opposite platform, perhaps obsolete
    public Entity nextPlatform; // Next station
    public Entity currentTrain; // The current train that is on the platform
    public float3 platform_entrance2; // Foot of the stairs, closest to platform: commuters walk here
    public float3 platform_entrance1; // Top of the stairs, middle point: commuters walk here
    public float3 platform_entrance0; // Entrance to the platform from other platforms: commuters walk here
    public float3 platform_exit2; // Foot of the stairs, closest to platform: commuters walk here
    public float3 platform_exit1; // Top of the stairs, middle point: commuters walk here
    public float3 platform_exit0; // Entrance to the platform from other platforms: commuters walk here
    public float3 carriage_entrance; // Entrance to the platform from the carriages
    public DOTS.BezierPoint point_platform_START; // Start of the platform on the bezier
    public DOTS.BezierPoint point_platform_END; // End of the platform on the bezier
    public byte carriageCount; // Number of carriages
    public int platformIndex; // Number of carriages
    public char parentMetroName;
    public int metroLineID;

}