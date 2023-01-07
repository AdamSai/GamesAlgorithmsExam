using System.Collections;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.DOTS.Components
{
    public struct QueueComponent : IComponentData
    {
        public float3 queuePoint0;
        public float3 queuePoint0_1;

        public float3 queuePoint1;
        public float3 queuePoint1_1;

        public float3 queuePoint2;
        public float3 queuePoint2_1;

        public float3 queuePoint3;
        public float3 queuePoint3_1;

        public float3 queuePoint4;
        public float3 queuePoint4_1;

        public NativeList<Entity> queue0;
        public NativeList<Entity> queue1;
        public NativeList<Entity> queue2;
        public NativeList<Entity> queue3;
        public NativeList<Entity> queue4;
    }
}