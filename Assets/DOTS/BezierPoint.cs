using System.Numerics;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DOTS
{
    public struct BezierPoint : IBufferElementData
    {
        public int index;
        public float3 location, handle_in, handle_out;
        public float distanceAlongPath;

        public BezierPoint(int _index, float3 _location, float3 _handle_in, float3 _handle_out)
        {
            index = _index;
            location = _location;
            handle_in = _handle_in;
            handle_out = _handle_out;
            distanceAlongPath = 0f;
        }

        public BezierPoint SetHandles(float3 _distance)
        {
            // TODO: See BEZIER_HANDLE_REACH from Metro.cs
            _distance *= 0.15f;
            handle_in = location - _distance;
            handle_out = location + _distance;
            return this;
        }
    }
}