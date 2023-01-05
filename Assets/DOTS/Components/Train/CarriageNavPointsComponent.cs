using System.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.DOTS.Components.Train
{
    public struct CarriageNavPointsComponent : IComponentData
    {
        public Entity entrancePointEntity;
    }
}