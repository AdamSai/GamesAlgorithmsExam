﻿using Unity.Collections;
using Unity.Entities;

namespace DOTS.Components
{
    public struct BezierPathComponent : IComponentData
    {
        public NativeArray<BezierPoint> Points;
        public float BezierHandleReach;
    }
}