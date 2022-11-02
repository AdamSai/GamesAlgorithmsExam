using System.Numerics;

namespace DOTS
{
    public struct BezierPoint
    {
        public int index;
        public Vector3 location, handle_in, handle_out;
        public float distanceAlongPath;

        public BezierPoint(int _index, Vector3 _location, Vector3 _handle_in, Vector3 _handle_out)
        {
            index = _index;
            location = _location;
            handle_in = _handle_in;
            handle_out = _handle_out;
            distanceAlongPath = 0f;
        }

        public void SetHandles(Vector3 _distance)
        {
            _distance *= Metro.BEZIER_HANDLE_REACH;
            handle_in = location - _distance;
            handle_out = location + _distance;
        }
    }
}