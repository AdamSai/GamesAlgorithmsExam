using System.Linq;
using DOTS.Components;
using DOTS.Utility;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DOTS.Jobs
{
    [BurstCompile]
    public partial struct AddOutboundPointsJob : IJobEntity
    {
        public NativeList<RailMarkerComponent> railMarkers;
        public EntityCommandBuffer ECB;
        
        private int totalOutboundPoints;

        public void Execute(ref BezierPathComponent path, ref MetroLineComponent metroLine)
        {
            var lineRailMarkers = new NativeList<RailMarkerComponent>(Allocator.Temp);

            AddOutboundHandles(path, metroLine, lineRailMarkers);
            totalOutboundPoints = path.points.Length;

            // Fix outbound handles
            var _POINTS = path.points;
            FixOutboundHandles(ref _POINTS);
            path.distance = BezierUtility.MeasurePath(path);
            
            // Return points
            var returnPoints = new NativeList<BezierPoint>(Allocator.Temp);
            AddReturnPoints(path, returnPoints, _POINTS);
            
            // Fix return handles
            FixReturnHandles(returnPoints);
            path.distance = BezierUtility.MeasurePath(path);
            
            // now that the rails have been laid - let's put the platforms on
            // int totalPoints = path.points.Length;
            // for (int i = 1; i < lineRailMarkers.Length; i++)
            // {
            //     int _plat_END = i;
            //     int _plat_START = i - 1;
            //     if (lineRailMarkers[_plat_END].RailMarkerType == RailMarkerTypes.PLATFORM_END &&
            //         lineRailMarkers[_plat_START].RailMarkerType == RailMarkerTypes.PLATFORM_START)
            //     {
            //         Platform _ouboundPlatform = AddPlatform(_plat_START, _plat_END);
            //         // now add an opposite platform!
            //         int opposite_START = totalPoints - (i + 1);
            //         int opposite_END = totalPoints - i;
            //         Platform _oppositePlatform = AddPlatform(opposite_START, opposite_END);
            //         _oppositePlatform.transform.eulerAngles =
            //             _ouboundPlatform.transform.rotation.eulerAngles + new Vector3(0f, 180f, 0f);
            //         ;
            //
            //         // pair these platforms as opposites
            //         _ouboundPlatform.PairWithOppositePlatform(_oppositePlatform);
            //         _oppositePlatform.PairWithOppositePlatform(_ouboundPlatform);
            //     }
            // }
            
            // Now, let's lay the rail meshes
            float _DIST = 0f;
            while (_DIST < path.distance)
            {
                float _DIST_AS_RAIL_FACTOR = Get_distanceAsRailProportion(_DIST, path.distance);
                Vector3 _RAIL_POS = Get_PositionOnRail(_DIST_AS_RAIL_FACTOR, path.distance, path.points);
                Vector3 _RAIL_ROT = Get_RotationOnRail(_DIST_AS_RAIL_FACTOR, path.distance, path.points);
                var rail = ECB.Instantiate(metroLine.railPrefab);
                //            _RAIL.GetComponent<Renderer>().material.color = lineColour;
                var ltw = new WorldTransform();
                ltw.Position = _RAIL_POS;
                ltw.Rotation = Quaternion.LookRotation(_RAIL_POS - _RAIL_ROT);
                ECB.SetComponent(rail, LocalTransform.FromPosition(_RAIL_POS));
                
                Debug.Log("Pos: " + ltw.Position);
                _DIST += Metro.RAIL_SPACING;
            }
        }

        private void FixReturnHandles(NativeList<BezierPoint> returnPoints)
        {
            for (int i = 0; i <= totalOutboundPoints - 1; i++)
            {
                BezierPoint _currentPoint = returnPoints[i];
                if (i == 0)
                {
                    returnPoints[i] = returnPoints[i].SetHandles(returnPoints[1].location - _currentPoint.location);
                }
                else if (i == totalOutboundPoints - 1)
                {
                    returnPoints[i] = returnPoints[i].SetHandles(_currentPoint.location - returnPoints[i - 1].location);
                }
                else
                {
                    returnPoints[i] = returnPoints[i].SetHandles(returnPoints[i + 1].location - returnPoints[i - 1].location);
                }
            }
        }

        private void AddReturnPoints(BezierPathComponent path, NativeList<BezierPoint> returnPoints, NativeList<BezierPoint> _POINTS)
        {
            // TODO: See BEZIER_PLATFORM_OFFSET from Metro.cs
            float platformOffset = 3f;
            for (int i = totalOutboundPoints - 1; i >= 0; i--)
            {
                float3 _targetLocation =
                    BezierUtility.GetPoint_PerpendicularOffset(path.points[i], platformOffset, path.distance, path.points);
                path.AddPoint(_targetLocation);
                returnPoints.Add(_POINTS[_POINTS.Length - 1]);
            }
        }

        private void AddOutboundHandles(BezierPathComponent path, MetroLineComponent metroLine, NativeList<RailMarkerComponent> lineRailMarkers)
        {
            for (var i = 0; i < railMarkers.Length; i++)
            {
                if (railMarkers[i].MetroLineID == metroLine.MetroLineID)
                {
                    path.AddPoint(railMarkers[i].Position);
                    lineRailMarkers.Add(railMarkers[i]);
                }
            }
        }

        private void FixOutboundHandles(ref NativeList<BezierPoint> points)
        {
            for (var i = 0; i <= totalOutboundPoints - 1; i++)
            {
                if (i == 0)
                {
                    points[i] = points[i].SetHandles(points[1].location - points[i].location);
                }
                else if (i == totalOutboundPoints - 1)
                {
                    points[i] = points[i].SetHandles(points[i].location - points[i - 1].location);
                }
                else
                {
                    points[i] =
                        points[i].SetHandles(points[i + 1].location - points[i - 1].location);
                }
            }
        }
        
        // Platform AddPlatform(int _index_platform_START, int _index_platform_END, BezierPathComponent path)
        // {
        //     BezierPoint _PT_START = path.points[_index_platform_START];
        //     BezierPoint _PT_END = path.points[_index_platform_END];
        //     GameObject platform_OBJ =
        //         (GameObject)Metro.Instantiate(Metro.INSTANCE.prefab_platform, _PT_END.location, Quaternion.identity);
        //     Platform platform = platform_OBJ.GetComponent<Platform>();
        //     platform.SetupPlatform(this, _PT_START, _PT_END);
        //     platform_OBJ.transform.LookAt(BezierUtility.GetPoint_PerpendicularOffset(_PT_END, -3f, path.distance, path.points));
        //     platforms.Add(platform);
        //     return platform;
        // }

        public float Get_distanceAsRailProportion(float _realDistance, float distance)
        {
            return _realDistance / distance;
        }
        
        public Vector3 Get_PositionOnRail(float _pos, float distance, NativeList<BezierPoint> points)
        {
            return BezierUtility.Get_Position(_pos, distance, points);
        }
        
        public Vector3 Get_RotationOnRail(float _pos, float distance, NativeList<BezierPoint> points)
        {
            return BezierUtility.Get_NormalAtPosition(_pos, distance, points);
        }

    }
}