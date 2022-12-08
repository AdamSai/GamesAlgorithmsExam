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
        
        // TODO: Optimize this if startup is slow
        // Maybe this should take in an Array/List instead and we process all of the points
        // Febrice talked about moving the loops up in the hierarchy to avoid the overhead of looping
        public BezierPoint AddPoint(float3 location)
        {
            BezierPoint result = new BezierPoint(points.Length, location, location, location);
            points.Add(result);
            if (points.Length > 1)
            {
                BezierPoint prev = points[points.Length - 2];
                var currentIdx = points.Length - 1;
                points[currentIdx] = SetHandles(currentIdx, prev.location);
            }
        
            return result;
        }

        BezierPoint SetHandles(int _currentIdx, float3 _prevPointLocation)
        {
            float3 distPrevCurrent = math.normalize(points[_currentIdx].location - _prevPointLocation);
        
            return points[_currentIdx].SetHandles(distPrevCurrent);
        }
    }
}