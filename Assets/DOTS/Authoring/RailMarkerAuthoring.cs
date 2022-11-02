using DOTS.Components;
using Unity.Entities;

namespace DOTS.Authoring
{
    public class RailMarkerAuthoring : UnityEngine.MonoBehaviour
    {
        public byte MetroLineID;
        public byte PointIndex;
        public RailMarkerTypes RailMarkerType;
    }

    public class RailMarkerBaker : Baker<RailMarkerAuthoring>
    {
        public override void Bake(RailMarkerAuthoring authoring)
        {
            AddComponent(new RailMarkerComponent
            {
                MetroLineID = authoring.MetroLineID,
                PointIndex = authoring.PointIndex,
                RailMarkerType = authoring.RailMarkerType
            });
        }
    }
}