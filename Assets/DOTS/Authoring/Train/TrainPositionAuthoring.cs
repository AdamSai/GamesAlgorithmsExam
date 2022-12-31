
using DOTS.Components;
using Unity.Entities;
using UnityEngine;

namespace DOTS.Authoring
{

    public class TrainPositionAuthoring : MonoBehaviour
    {
        public float Value;
    }
    public class TrainPositionBaker : Baker<TrainPositionAuthoring>
    {
        public override void Bake(TrainPositionAuthoring authoring)
        {
            AddComponent(new TrainPositionComponent
            {
                value = authoring.Value
            });
        }
    }
}