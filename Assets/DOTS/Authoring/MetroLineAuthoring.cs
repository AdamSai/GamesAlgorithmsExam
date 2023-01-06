using DOTS.Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace DOTS.Authoring
{
    public class MetroLineAuthoring : MonoBehaviour
    {
        // [Range(0f, 1f)]
        // public float BezierHandleReach = 0.15f;
        public byte MetroLineID;
        public GameObject RailPrefab;
        public GameObject PlatformPrefab;
        public char metroName;
    }

    public class MetroLineBaker : Baker<MetroLineAuthoring>
    {
        public override void Bake(MetroLineAuthoring authoring)
        {
            AddComponent(new MetroLineComponent
            {
                MetroLineID = authoring.MetroLineID,
                railPrefab = GetEntity(authoring.RailPrefab),
                platformPrefab = GetEntity(authoring.PlatformPrefab),
                metroLineName = authoring.metroName
            });
            //var data = authoring.GetComponentsInChildren<RailMarkerAuthoring>();
            //var railMarkers = new NativeArray<RailMarkerStruct>(data.Length, Allocator.Persistent);

            //for (int i = 0; i < data.Length; i++)
            //{
            //    var railMarker = data[i];
            //    railMarkers[i] = new RailMarkerStruct(railMarker.MetroLineID, railMarker.PointIndex,
            //        railMarker.RailMarkerType, railMarker.transform.position);
            //}

            //AddComponent(new RailMarkerContainer
            //{
            //    Value = railMarkers
            //});



            AddComponent(new BezierPathComponent
            {
            });

            AddBuffer<BezierPoint>();
        }
    }
}