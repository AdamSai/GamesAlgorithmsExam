using Assets.DOTS.Components;
using Assets.DOTS.Components.Train;
using System.Collections;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Assets.DOTS.Authoring.Train
{
    public class CarriageSeatAuthoring : MonoBehaviour
    {

    }

    public class CarriageSeatBaker : Baker<CarriageSeatAuthoring>
    {
        public override void Bake(CarriageSeatAuthoring authoring)
        {
            AddComponent(new CarriageSeatComponent
            {
                available = true
            });
        }
    }
}