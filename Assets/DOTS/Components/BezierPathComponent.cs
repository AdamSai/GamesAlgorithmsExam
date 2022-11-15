using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace DOTS.Components
{
    public struct BezierPathComponent : IComponentData
    {
        public NativeList<BezierPoint> points;
        private float pathLength;
        private float distance;
        
        // TODO: Optimize this if startup is slow
        // Maybe this should take in an Array/List instead and we process all of the points
        // Febrice talked about moving the loops up in the hierarchy to avoid the overhead of looping
        public BezierPoint AddPoint(float3 _location)
        {
            BezierPoint result = new BezierPoint(points.Length, _location, _location, _location);
            points.Add(result);
            if (points.Length > 1)
            {
                BezierPoint _prev = points[points.Length - 2];
                var _currentIdx = points.Length - 1;
                SetHandles(_currentIdx, _prev.location);
            }
        
            return result;
        }

        void SetHandles(int _currentIdx, float3 _prevPointLocation)
        {
            float3 _dist_PREV_CURRENT = math.normalize(points[_currentIdx].location - _prevPointLocation);
        
            points[_currentIdx].SetHandles(_dist_PREV_CURRENT);
        }

    }
}