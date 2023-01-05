using System.Collections.Generic;

namespace DOTS.Utility
{
    public class PlatformComparer : IComparer<PlatformComponent>
    {
        public int Compare(PlatformComponent x, PlatformComponent y)
        {
            if (x.point_platform_START.index > y.point_platform_START.index)
                return 1;
            if (x.point_platform_START.index < y.point_platform_START.index)
                return -1;
            return 0;
        }
    }
}