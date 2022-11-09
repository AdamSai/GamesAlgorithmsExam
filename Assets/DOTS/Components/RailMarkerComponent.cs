using Unity.Entities;
using Unity.Mathematics;

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
        public float3 Position;
    }
    
    public struct RailMarkerStruct
    {
        public byte MetroLineID;
        public byte PointIndex;
        public RailMarkerTypes RailMarkerType;
        public float3 Position;


        public RailMarkerStruct(byte metroLineID, byte pointIndex, RailMarkerTypes railMarkerType, float3 position)
        {
            MetroLineID = metroLineID;
            PointIndex = pointIndex;
            RailMarkerType = railMarkerType;
            Position = position;
        }
    }
}