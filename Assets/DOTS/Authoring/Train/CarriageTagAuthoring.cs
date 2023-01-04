using DOTS.Components;
using Unity.Entities;
using UnityEngine;

namespace DOTS.Authoring
{

    public class CarriageTagAuthoring : MonoBehaviour
    {
    }

    public class CarriageTagBaker : Baker<CarriageTagAuthoring>
    {
        public override void Bake(CarriageTagAuthoring authoring)
        {
            AddComponent(new CarriageTag { });
        }
    }
}