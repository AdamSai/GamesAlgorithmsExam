using Unity.Collections;
using Unity.Entities;

namespace DOTS.Components
{
    public struct RailMarkerContainer : IComponentData
    {
        public NativeArray<RailMarkerStruct> Value;
    }
}