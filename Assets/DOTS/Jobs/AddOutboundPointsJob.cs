using DOTS.Components;
using Unity.Entities;
using Unity.Jobs;

namespace DOTS.Jobs
{
    public partial struct AddOutboundPointsJob : IJobEntity
    {
        public void Execute(ref BezierPathComponent bezierPath, in RailMarkerContainer railMarkers)
        {
            for (var i = 0; i < bezierPath.points.Length; i++)
                bezierPath.AddPoint(railMarkers.Value[i].Position);
        }
    }
}