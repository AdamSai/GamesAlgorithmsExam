using Unity.Entities;

namespace DOTS.Authoring
{
    public class MetroLineCarriageDataAuthoring : UnityEngine.MonoBehaviour
    {
        public byte Carriages;
        public float CarriagesSpeed;
        public UnityEngine.GameObject Carriage;
    }

    public class MetroLineCarriageDataBaker : Baker<MetroLineCarriageDataAuthoring>
    {
        public override void Bake(MetroLineCarriageDataAuthoring authoring)
        {
            AddComponent(new DOTS.Components.MetroLineCarriageDataComponent
            {
                carriages = authoring.Carriages,
                carriagesSpeed = authoring.CarriagesSpeed,
                carriage = GetEntity(authoring.Carriage)
            });
        }
    }
}