using Unity.Entities;

namespace DOTS.Components
{
    public enum RailMarkerTypes : byte
    {
        NONE,
        PLATFORM_START,
        PLATFORM_END,
    }

    public struct RailMarkerComponent : IComponentData
    {
        public byte MetroLineID;
        public byte PointIndex;
        public RailMarkerTypes RailMarkerType;
    }
    
    public struct RailMarkerStruct
    {
        public byte MetroLineID;
        public byte PointIndex;
        public RailMarkerTypes RailMarkerType;

        public RailMarkerStruct(byte metroLineID, byte pointIndex, RailMarkerTypes railMarkerType)
        {
            MetroLineID = metroLineID;
            PointIndex = pointIndex;
            RailMarkerType = railMarkerType;
        }
    }
}