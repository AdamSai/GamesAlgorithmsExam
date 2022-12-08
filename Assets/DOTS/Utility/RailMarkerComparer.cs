using System.Collections.Generic;
using DOTS.Components;

namespace DOTS.Utility
{
    public struct RailMarkerComparer : IComparer<RailMarkerComponent>
    {
        public int Compare(RailMarkerComponent x, RailMarkerComponent y)
        {
            if (x.PointIndex > y.PointIndex)
                return 1;
            if (x.PointIndex < y.PointIndex)
                return -1;
            return 0;
        }
    }
}