using DOTS.Components;
using Unity.Entities;
using UnityEngine;

namespace DOTS.Authoring
{

    public class CarriageColorAuthoring : MonoBehaviour
    {
    }

    public class CarriageColorBaker : Baker<CarriageColorAuthoring>
    {
        public override void Bake(CarriageColorAuthoring authoring)
        {
            AddComponent(new CarriageColorTag { });
        }
    }
}