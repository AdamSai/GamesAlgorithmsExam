
using DOTS.Components;
using Unity.Entities;
using UnityEngine;

namespace DOTS.Authoring
{

    public class MaxTrainSpeedAuthoring : MonoBehaviour
    {
    }
    public class MaxTrainSpeedBaker : Baker<MaxTrainSpeedAuthoring>
    {
        public override void Bake(MaxTrainSpeedAuthoring authoring)
        {
            AddComponent(new MaxTrainSpeedComponent { });
        }
    }
}