using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct WalkComponent : IComponentData
{
    public NativeList<float3> destinations;
    public float speed;
    public float3 velocity;
    //public float acceleration;
}
