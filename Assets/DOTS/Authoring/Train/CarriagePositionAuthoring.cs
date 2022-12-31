
using DOTS.Components;
using Unity.Entities;
using UnityEngine;

namespace DOTS.Authoring
{

    public class CarriagePositionAuthoring : MonoBehaviour
    {
        public float Value;
    }
    public class CarriagePositionBaker : Baker<CarriagePositionAuthoring>
    {
        public override void Bake(CarriagePositionAuthoring authoring)
        {
            AddComponent(new CarriagePositionComponent
            {
                value = authoring.Value
            });
        }
    }
}