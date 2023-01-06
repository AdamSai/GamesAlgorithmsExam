using DOTS.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DOTS.Utility
{
    public class BezierUtility
    {
        public static void AddPoint(float3 location, ref DynamicBuffer<BezierPoint> path)
        {
            BezierPoint result = new BezierPoint(path.Length, location, location, location);
            path.Add(result);
            if (path.Length > 1)
            {
                BezierPoint prev = path[path.Length - 2];
                var currentIdx = path.Length - 1;
                path[currentIdx] = SetHandles(currentIdx, prev.location, path);
            }
        }

        static BezierPoint SetHandles(int _currentIdx, float3 _prevPointLocation, DynamicBuffer<BezierPoint> points)
        {
            float3 distPrevCurrent = math.normalize(points[_currentIdx].location - _prevPointLocation);
        
            return points[_currentIdx].SetHandles(distPrevCurrent);
        }
        
        public static float MeasurePath(ref DynamicBuffer<BezierPoint> points)
        {
            float distance = 0f;
            var point = points[0];
            point.distanceAlongPath = 0.000001f;
            points[0] = point;

            for (int i = 1; i < points.Length; i++)
            {
                var currentPoint = points[i];
                currentPoint.distanceAlongPath = MeasurePoint(i, i - 1, points, ref distance);
                points[i] = currentPoint;
            }

            // add last stretch (return loop to point ZERO)
            return distance + Get_AccurateDistanceBetweenPoints(0, points.Length - 1, points);
        }

        public static float MeasurePoint(int _currentPoint, int _prevPoint, DynamicBuffer<BezierPoint> points, ref float distance)
        {
            distance += Get_AccurateDistanceBetweenPoints(_currentPoint, _prevPoint, points);
            return distance;
        }

        public static float Get_AccurateDistanceBetweenPoints(int _current, int _prev, DynamicBuffer<BezierPoint> points)
        {
            BezierPoint _currentPoint = points[_current];
            BezierPoint _prevPoint = points[_prev];
            float measurementIncrement = 1f / 2; // TODO: for 2f see Metro.BEZIER_MEASUREMENT_SUBDIVISIONS
            float regionDistance = 0f;
            for (int i = 0; i < 2 - 1; i++) // TODO: for 2f see Metro.BEZIER_MEASUREMENT_SUBDIVISIONS
            {
                float _CURRENT_SUBDIV = i * measurementIncrement;
                float _NEXT_SOBDIV = (i + 1) * measurementIncrement;
                regionDistance += math.distance(BezierLerp(_prevPoint, _currentPoint, _CURRENT_SUBDIV),
                    BezierLerp(_prevPoint, _currentPoint, _NEXT_SOBDIV));
            }

            return regionDistance;
        }

        public static float3 BezierLerp(BezierPoint _pointA, BezierPoint _pointB, float _progress)
        {
            // Round 1 --> Origins to handles, handle to handle
            var l1_a_aOUT = math.lerp(_pointA.location, _pointA.handle_out, _progress);
            var l2_bIN_b = math.lerp(_pointB.handle_in, _pointB.location, _progress);
            var l3_aOUT_bIN = math.lerp(_pointA.handle_out, _pointB.handle_in, _progress);
            // Round 2 
            var l1_to_l3 = math.lerp(l1_a_aOUT, l3_aOUT_bIN, _progress);
            var l3_to_l2 = math.lerp(l3_aOUT_bIN, l2_bIN_b, _progress);
            // Final Round
            var result = math.lerp(l1_to_l3, l3_to_l2, _progress);

            return result;
        }
        
        public static float3 GetPoint_PerpendicularOffset(BezierPoint _point, float _offset, float distance, DynamicBuffer<BezierPoint> points)
        {
            return _point.location + Get_TangentAtPosition(_point.distanceAlongPath / distance, distance, points) * _offset;
        }
        
        public static float3 Get_TangentAtPosition(float _position, float distance, DynamicBuffer<BezierPoint> points)
        {
            float3 normal = Get_NormalAtPosition(_position, distance, points);
            return new float3(-normal.z, normal.y, normal.x);
        }
        
        public static float3 Get_NormalAtPosition(float _position, float distance, DynamicBuffer<BezierPoint> points)
        {
            var _current = Get_Position(_position, distance, points);
            var _ahead = Get_Position((_position + 0.0001f) % 1f, distance, points);
            return (_ahead - _current) / math.distance(_ahead, _current);
        }
        
        public static float3 Get_Position(float _progress, float distance, DynamicBuffer<BezierPoint> points)
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

        private static int i = 0;
        public static int GetRegionIndex(float _progress, DynamicBuffer<BezierPoint> points)
        {
            int result = 0;
            int totalPoints = points.Length;
            for (int i = 0; i < totalPoints; i++)
            {
                BezierPoint _PT = points[i];
                // i++;
                // if (i > 5000)
                // {
                //     Debug.Log(_PT.distanceAlongPath + " " + _progress);
                // }
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
        
        public static float Get_proportionAsDistance(float _proportion, float distance)
        {
            return distance * _proportion;
        }
    }
}