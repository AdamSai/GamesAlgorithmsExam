using System.Numerics;
using Unity.Mathematics;

namespace DOTS
{
    public struct BezierPoint
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

        public void SetHandles(float3 _distance)
        {
            _distance *= Metro.BEZIER_HANDLE_REACH;
            handle_in = location - _distance;
            handle_out = location + _distance;
        }
    }
}