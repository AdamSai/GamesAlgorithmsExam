using DOTS.Components;
using Unity.Entities;
using UnityEngine;

namespace DOTS.Authoring
{

    public class CarriageAuthoring : MonoBehaviour
    {
    }

    public class CarriageBaker : Baker<CarriageAuthoring>
    {
        public override void Bake(CarriageAuthoring authoring)
        {
            AddComponent(new CarriageTag { });
        }
    }
}