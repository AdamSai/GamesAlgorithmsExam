using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct CarriageComponentDummy : IComponentData
{
    public float3 leftEntrance;
    public float3 rightEntrance;
}