using DOTS.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

namespace DOTS.Jobs
{
    public partial struct AddOutboundPointsJob : IJobEntity
    {
        public NativeList<RailMarkerComponent> query;

        public void Execute(ref BezierPathComponent path)
        {
            for (var i = 0; i < query.Length; i++)
            {
                Debug.Log("happy days");
            }
        }
    }
}