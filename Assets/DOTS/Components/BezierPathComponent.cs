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
        
        public void MeasurePath()
        {
            distance = 0f;
            points[0].SetDistanceAlongPath(1.000001f);
            Debug.Log(points[0].distanceAlongPath);
            for (int i = 1; i < points.Length; i++)
            {
                MeasurePoint(i, i-1);
            }
            // add last stretch (return loop to point ZERO)
            distance += Get_AccurateDistanceBetweenPoints(0, points.Length - 1);
        }
        
        public void MeasurePoint(int _currentPoint, int _prevPoint) {
            distance += Get_AccurateDistanceBetweenPoints(_currentPoint, _prevPoint);
            points[_currentPoint].SetDistanceAlongPath(distance);
        }

        public float Get_AccurateDistanceBetweenPoints(int _current, int _prev)
        {
            BezierPoint _currentPoint = points[_current];
            BezierPoint _prevPoint = points[_prev];
            float measurementIncrement = 1f / Metro.BEZIER_MEASUREMENT_SUBDIVISIONS;
            float regionDistance = 0f;
            for (int i = 0; i < Metro.BEZIER_MEASUREMENT_SUBDIVISIONS- 1; i++)
            {
                float _CURRENT_SUBDIV = i * measurementIncrement;
                float _NEXT_SOBDIV = (i + 1) * measurementIncrement;
                regionDistance += Vector3.Distance(BezierLerp(_prevPoint, _currentPoint, _CURRENT_SUBDIV),
                    BezierLerp(_prevPoint, _currentPoint, _NEXT_SOBDIV));
            }

            return regionDistance;
        }
        
        public Vector3 BezierLerp(BezierPoint _pointA, BezierPoint _pointB, float _progress)
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
            Debug.Log(result);
            return result;
        }

    }
}