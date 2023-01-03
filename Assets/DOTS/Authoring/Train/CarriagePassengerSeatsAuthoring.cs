using Assets.DOTS.Components.Train;
using DOTS.Authoring;
using DOTS.Components;
using System.Collections;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Assets.DOTS.Authoring.Train
{
    public class CarriagePassengerSeatsAuthoring : MonoBehaviour
    {

    }

    public class CarriagePassengerSeatsBaker : Baker<CarriagePassengerSeatsAuthoring>
    {
        // Get the passenger seat entities in a list, how???

        public override void Bake(CarriagePassengerSeatsAuthoring authoring)
        {
            AddComponent(new CarriagePassengerSeatsComponent
            {
                seats = new NativeList<Entity>(Allocator.Persistent)
            });
        }
    }
}