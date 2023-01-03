using System.Collections;
using Unity.Entities;
using UnityEngine;

namespace Assets.DOTS.Components
{
    public struct CarriageSeatComponent : IComponentData
    {
        public bool available;
    }
}