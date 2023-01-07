using System.Collections;
using Unity.Entities;
using UnityEngine;

namespace Assets.DOTS.Components
{
    public struct CommuterSpawnComponent : IComponentData
    {
        public int amount;
        public Entity commuter;
        public bool hasSpawned;
    }
}