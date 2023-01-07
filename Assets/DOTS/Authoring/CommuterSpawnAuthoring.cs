using Assets.DOTS.Components;
using System.Collections;
using Unity.Entities;
using UnityEngine;

namespace Assets.DOTS.Authoring
{
    public class CommuterSpawnAuthoring : MonoBehaviour
    {
        public UnityEngine.GameObject prefab;
        public int amount;
    }

    public class CommuterSpawnBaker : Baker<CommuterSpawnAuthoring>
    {
        public override void Bake(CommuterSpawnAuthoring authoring)
        {
            AddComponent<CommuterSpawnComponent>(new CommuterSpawnComponent
            {
                commuter = GetEntity(authoring.prefab),
                amount = authoring.amount
            });
        }
    }
}