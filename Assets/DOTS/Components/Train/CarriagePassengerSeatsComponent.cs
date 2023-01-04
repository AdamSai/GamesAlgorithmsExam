﻿using System.Collections;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Assets.DOTS.Components.Train
{
    public struct CarriagePassengerSeatsComponent : IComponentData
    {
        public bool init;
        public NativeList<Entity> seats;
    }
}