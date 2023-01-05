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
    public partial struct AddOutboundPointsJob : IJobEntity
    {
        public NativeList<RailMarkerComponent> railMarkers; // Contains ALL railMarkers
        public EntityCommandBuffer ECB;
        // public NativeList<RailMarkerComponent> lineRailMarkers; // Contains railMarkers that are part of the line

        private int totalOutboundPoints;

        public void Execute(ref BezierPathComponent path, ref MetroLineComponent metroLine, in MetroLineCarriageDataComponent metroLineCarriageData, in MetroLineTrainDataComponent trainData)
        {
            var lineRailMarkers = new NativeList<RailMarkerComponent>(Allocator.Temp);
            var platforms = new NativeList<PlatformComponent>(Allocator.Temp);
            var platformEntities = new NativeList<Entity>(Allocator.Temp);
            Debug.Log("Running on metroLine: " + metroLine.MetroLineID);
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
            int totalPoints = path.points.Length;

            for (int i = 1; i < lineRailMarkers.Length; i++)
            {
                int _plat_END = i;
                int _plat_START = i - 1;
                if (lineRailMarkers[_plat_END].RailMarkerType == RailMarkerTypes.PLATFORM_END &&
                    lineRailMarkers[_plat_START].RailMarkerType == RailMarkerTypes.PLATFORM_START)
                {
                    var _outboundPlatform = AddPlatform(_plat_START, _plat_END, path, metroLine, metroLineCarriageData, platforms, platformEntities);
                    // now add an opposite platform!
                    int opposite_START = totalPoints - (i + 1);
                    int opposite_END = totalPoints - i;
                    var _oppositePlatform = AddPlatform(opposite_START, opposite_END, path, metroLine, metroLineCarriageData, platforms, platformEntities);
                    
                    ECB.SetComponent(_outboundPlatform.entity, new OppositePlatformComponent
                    {
                        OppositePlatform = _oppositePlatform,
                        EulorRotation = float3.zero,
                    });
                    
                    ECB.SetComponent(_oppositePlatform.entity, new OppositePlatformComponent
                    {
                        OppositePlatform = _outboundPlatform,
                        EulorRotation = new float3(0f, 180f, 0f)
                    });
                }
            }
            
            // Sorting platforms
            platforms.Sort(new PlatformComparer());
            // TODO: If platform driving fucks up look at this
            for (int i = 0; i < platforms.Length; i++)
            {
                var p = platforms[i];
                p.platformIndex = i;
                p.nextPlatform = platformEntities[(i + 1) % platforms.Length];
                ECB.SetComponent(platformEntities[i], p);
            }

            metroLine.SpeedRatio = path.distance * trainData.maxTrainSpeed;
            
            // Now, let's lay the rail meshes
            InstantiateRails(path, metroLine);
        }

        private EntityWithRotation AddPlatform(int platStart, int platEnd, BezierPathComponent path, MetroLineComponent metroLine, MetroLineCarriageDataComponent metroLineCarriageData, NativeList<PlatformComponent> platforms, NativeList<Entity> platformEntities)
        {
            var _PT_START = path.points[platStart];
            var _PT_END = path.points[platEnd];

            var plat = ECB.Instantiate(metroLine.platformPrefab);
            var transform = LocalTransform.FromPosition(_PT_END.location);
            var rot = quaternion.LookRotation(
                transform.Position - BezierUtility.GetPoint_PerpendicularOffset(_PT_END, 3f, path.distance, path.points), Vector3.up);
            transform.Rotation = rot;
            ECB.SetComponent(plat, transform);
            
            var platformComponent = new PlatformComponent
            {
                point_platform_START = _PT_START,
                point_platform_END = _PT_END,
                carriageCount = metroLineCarriageData.carriages,
                neighborPlatforms = new NativeList<Entity>(Allocator.Persistent),
                parentMetroName = metroLine.metroLineName
            };
            platforms.Add(platformComponent);
            // ECB.SetComponent(plat, platformComponent);
            // TODO: setup color, queues, walkways, neighborPlatforms
            
            // ECB.SetComponent(plat, platformComponent);

            platformEntities.Add(plat);
            return new EntityWithRotation(plat, transform.Rotation);
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
                    returnPoints[i] = returnPoints[i].SetHandles(returnPoints[returnStartIndex + 1].location - returnPoints[i].location);
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
    
    public struct EntityWithRotation
    {
        public Entity entity;
        public Quaternion rotation;

        public EntityWithRotation(Entity entity, quaternion rotation)
        {
            this.entity = entity;
            this.rotation = rotation;
        }
    }
}