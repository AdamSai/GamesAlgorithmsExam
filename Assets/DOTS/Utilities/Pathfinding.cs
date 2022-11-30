using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public static class Pathfinding
{
    public static NativeArray<CommuterComponentTask> GetPath(NativeArray<Entity> platforms, 
        ComponentLookup<PlatformComponent> myTypeFromEntity, 
        Entity startPlatform, Entity endPlatform)
    {


        throw new System.Exception("Not implemented exception.");
    }
}