using DOTS.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace DOTS.Jobs
{
    [BurstCompile]
    public partial struct FixOutboundHandlesJob : IJobEntity
    {
        public void Execute(ref BezierPathComponent path)
        {
            for (var i = 0; i <= path.points.Length - 1; i++)
            {
                if (i == 0)
                {
                    path.points[i] = path.points[i].SetHandles(path.points[1].location - path.points[i].location);
                }
                else if (i == path.points.Length - 1)
                {
                    path.points[i] = path.points[i].SetHandles(path.points[i].location - path.points[i - 1].location);
                }
                else
                {
                    path.points[i] =
                        path.points[i].SetHandles(path.points[i + 1].location - path.points[i - 1].location);
                }
            }

            // MeasurePath(path);
        }


    }
}