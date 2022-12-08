using DOTS.Components;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace DOTS.Utility
{
    public class BezierUtility
    {
        public static float MeasurePath(BezierPathComponent path)
        {
            float distance = 0f;
            var point = path.points[0];
            point.distanceAlongPath = 0.000001f;
            path.points[0] = point;

            for (int i = 1; i < path.points.Length; i++)
            {
                var x = path.points[i].distanceAlongPath;
                var currentPoint = path.points[i];
                currentPoint.distanceAlongPath = MeasurePoint(i, i - 1, path.points, ref distance);
                path.points[i] = currentPoint;
            }

            // add last stretch (return loop to point ZERO)
            return distance + Get_AccurateDistanceBetweenPoints(0, path.points.Length - 1, path.points);
        }

        public static float MeasurePoint(int _currentPoint, int _prevPoint, NativeList<BezierPoint> points, ref float distance)
        {
            distance += Get_AccurateDistanceBetweenPoints(_currentPoint, _prevPoint, points);
            return distance;
        }

        public static float Get_AccurateDistanceBetweenPoints(int _current, int _prev, NativeList<BezierPoint> points)
        {
            BezierPoint _currentPoint = points[_current];
            BezierPoint _prevPoint = points[_prev];
            float measurementIncrement = 1f / Metro.BEZIER_MEASUREMENT_SUBDIVISIONS;
            float regionDistance = 0f;
            for (int i = 0; i < Metro.BEZIER_MEASUREMENT_SUBDIVISIONS - 1; i++)
            {
                float _CURRENT_SUBDIV = i * measurementIncrement;
                float _NEXT_SOBDIV = (i + 1) * measurementIncrement;
                regionDistance += Vector3.Distance(BezierLerp(_prevPoint, _currentPoint, _CURRENT_SUBDIV),
                    BezierLerp(_prevPoint, _currentPoint, _NEXT_SOBDIV));
            }

            return regionDistance;
        }

        public static Vector3 BezierLerp(BezierPoint _pointA, BezierPoint _pointB, float _progress)
        {
            // Round 1 --> Origins to handles, handle to handle
            Vector3 l1_a_aOUT = Vector3.Lerp(_pointA.location, _pointA.handle_out, _progress);
            Vector3 l2_bIN_b = Vector3.Lerp(_pointB.handle_in, _pointB.location, _progress);
            Vector3 l3_aOUT_bIN = Vector3.Lerp(_pointA.handle_out, _pointB.handle_in, _progress);
            // Round 2 
            Vector3 l1_to_l3 = Vector3.Lerp(l1_a_aOUT, l3_aOUT_bIN, _progress);
            Vector3 l3_to_l2 = Vector3.Lerp(l3_aOUT_bIN, l2_bIN_b, _progress);
            // Final Round
            Vector3 result = Vector3.Lerp(l1_to_l3, l3_to_l2, _progress);

            return result;
        }
        
        public static float3 GetPoint_PerpendicularOffset(BezierPoint _point, float _offset, float distance, NativeList<BezierPoint> points)
        {
            return _point.location + Get_TangentAtPosition(_point.distanceAlongPath / distance, distance, points) * _offset;
        }
        
        public static float3 Get_TangentAtPosition(float _position, float distance, NativeList<BezierPoint> points)
        {
            float3 normal = Get_NormalAtPosition(_position, distance, points);
            return new Vector3(-normal.z, normal.y, normal.x);
        }
        
        public static float3 Get_NormalAtPosition(float _position, float distance, NativeList<BezierPoint> points)
        {
            Vector3 _current = Get_Position(_position, distance, points);
            Vector3 _ahead = Get_Position((_position + 0.0001f) % 1f, distance, points);
            return (_ahead - _current) / Vector3.Distance(_ahead, _current);
        }
        
        public static Vector3 Get_Position(float _progress, float distance, NativeList<BezierPoint> points)
        {
            float progressDistance = distance * _progress;
            int pointIndex_region_start = GetRegionIndex(progressDistance, points);
            int pointIndex_region_end = (pointIndex_region_start + 1) % points.Length;

            // get start and end bez points
            BezierPoint point_region_start = points[pointIndex_region_start];
            BezierPoint point_region_end = points[pointIndex_region_end];
            // lerp between the points to arrive at PROGRESS
            float pathProgress_start = point_region_start.distanceAlongPath / distance;
            float pathProgress_end = (pointIndex_region_end != 0) ?  point_region_end.distanceAlongPath / distance : 1f;
            float regionProgress = (_progress - pathProgress_start) / (pathProgress_end - pathProgress_start);

            // do your bezier lerps
            // Round 1 --> Origins to handles, handle to handle
            return BezierLerp(point_region_start, point_region_end, regionProgress);
        }
        
        public static int GetRegionIndex(float _progress, NativeList<BezierPoint> points)
        {
            int result = 0;
            int totalPoints = points.Length;
            for (int i = 0; i < totalPoints; i++)
            {
                BezierPoint _PT = points[i];
                if (_PT.distanceAlongPath <= _progress)
                {
                    if (i == totalPoints - 1)
                    {
                        // end wrap
                        result = i;
                        break;
                    }
                    else if (points[i + 1].distanceAlongPath >= _progress)
                    {
                        // start < progress, end > progress <-- thats a match
                        result = i;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            return result;
        }
    }
}