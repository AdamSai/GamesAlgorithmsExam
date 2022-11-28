using Unity.Collections;
using Unity.Mathematics;

namespace DOTS
{
    public struct BezierPath
    {
        public NativeList<BezierPoint> points;
        private float pathLength;
        private float distance;

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