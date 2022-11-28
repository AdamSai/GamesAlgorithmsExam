using DOTS.Components;
using Unity.Collections;
using Unity.Entities;

namespace DOTS.Jobs
{
    public partial struct AddOutboundPointsJob : IJobEntity
    {
        public NativeList<RailMarkerComponent> railMarkers;

        public void Execute(ref BezierPathComponent path, ref MetroLineComponent metroLine)
        {
            for (var i = 0; i < railMarkers.Length; i++)
            {
                if (railMarkers[i].MetroLineID == metroLine.MetroLineID)
                {
                    path.AddPoint(railMarkers[i].Position);
                }
            }
        }
    }
}