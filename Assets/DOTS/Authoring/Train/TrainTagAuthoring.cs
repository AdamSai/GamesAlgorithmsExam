using DOTS.Components;
using Unity.Entities;
using UnityEngine;

namespace DOTS.Authoring
{

    public class TrainTagAuthoring : MonoBehaviour
    {
    }

    public class TrainTagBaker : Baker<TrainTagAuthoring>
    {
        public override void Bake(TrainTagAuthoring authoring)
        {
            AddComponent(new TrainTag { });
        }
    }
}