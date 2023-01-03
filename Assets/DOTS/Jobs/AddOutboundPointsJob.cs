using System.Linq;
using DOTS.Components;
using DOTS.Utility;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering.UI;

namespace DOTS.Jobs
{
    [BurstCompile]
    public partial struct AddOutboundPointsJob : IJobEntity
    {
        public NativeList<RailMarkerComponent> railMarkers;
        public EntityCommandBuffer ECB;
        
        private int totalOutboundPoints;

        public void Execute(ref BezierPathComponent path, in MetroLineComponent metroLine)
        {
            var lineRailMarkers = new NativeList<RailMarkerComponent>(Allocator.Temp);

            // Sort the rail markers by PointIndex
            railMarkers.Sort(new RailMarkerComparer());

            // Add outbound handles
            AddOutboundHandles(ref path, metroLine, lineRailMarkers);
            totalOutboundPoints = path.points.Length;

            // Fix outbound handles
            FixOutboundHandles(path.points);
            path.distance = BezierUtility.MeasurePath(ref path);
            
            // Return points
            var returnStartIndex = path.points.Length; // Stores index of when return points start in the nativelist
            AddReturnPoints(ref path);
            
            // Fix return handles
            FixReturnHandles(ref path.points, returnStartIndex);
            path.distance = BezierUtility.MeasurePath(ref path);
            
            // TODO: Platforms
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

            InstantiateRails(path, metroLine);
        }



        private void AddOutboundHandles(ref BezierPathComponent path, MetroLineComponent metroLine, NativeList<RailMarkerComponent> lineRailMarkers)
        {
            for (var i = 0; i < railMarkers.Length; i++)
            {
                if (railMarkers[i].MetroLineID == metroLine.MetroLineID)
                {
                    BezierUtility.AddPoint(railMarkers[i].Position, ref path);
                    lineRailMarkers.Add(railMarkers[i]);
                }
            }
        }

        private void FixOutboundHandles(NativeList<BezierPoint> points)
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
        
                
        private void AddReturnPoints(ref BezierPathComponent path)
        {
            // TODO: See BEZIER_PLATFORM_OFFSET from Metro.cs
            float platformOffset = 8f;
            for (int i = totalOutboundPoints - 1; i >= 0; i--)
            {
                float3 _targetLocation =
                    BezierUtility.GetPoint_PerpendicularOffset(path.points[i], platformOffset, path.distance, path.points);
                BezierUtility.AddPoint(_targetLocation, ref path);
            }
        }
        
        private void FixReturnHandles(ref NativeList<BezierPoint> returnPoints, int returnStartIndex)
        {
            for (int i = returnStartIndex; i < returnPoints.Length; i++)
            {
                if (i == returnStartIndex)
                {
                    returnPoints[i] = returnPoints[i].SetHandles(returnPoints[returnStartIndex].location - returnPoints[i].location);
                }
                else if (i == returnPoints.Length - 1)
                {
                    returnPoints[i] = returnPoints[i].SetHandles(returnPoints[i].location - returnPoints[i - 1].location);
                }
                else
                {
                    returnPoints[i] = returnPoints[i].SetHandles(returnPoints[i + 1].location - returnPoints[i - 1].location);
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
        
        private void InstantiateRails(BezierPathComponent path, MetroLineComponent metroLine)
        {
            float _DIST = 0f;
            while (_DIST < path.distance)
            {
                float _DIST_AS_RAIL_FACTOR = Get_distanceAsRailProportion(_DIST, path.distance);
                float3 _RAIL_POS = Get_PositionOnRail(_DIST_AS_RAIL_FACTOR, path.distance, path.points);
                float3 _RAIL_ROT = Get_RotationOnRail(_DIST_AS_RAIL_FACTOR, path.distance, path.points);
               
                var rail = ECB.Instantiate(metroLine.railPrefab);
                var transform = LocalTransform.FromPosition(_RAIL_POS);
                var rot = Quaternion.LookRotation(transform.Position - (_RAIL_POS - _RAIL_ROT), Vector3.up);
                transform.Rotation = rot;
                ECB.SetComponent(rail, transform);
                
                // TODO: See Metro.RAIL_SPACING
                _DIST += 0.5f;
            }
        }

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