using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DOTS.Components
{
    public struct BezierPathComponent : IComponentData
    {
        public NativeList<BezierPoint> points;
        public float pathLength;
        public float distance;
    }
}