
using DOTS.Components;
using Unity.Entities;
using UnityEngine;

namespace DOTS.Authoring
{

    public class TrainSpeedAuthoring : MonoBehaviour
    {
        public float Speed;
        public float Friction;
    }
    public class TrainSpeedBaker : Baker<TrainSpeedAuthoring>
    {
        public override void Bake(TrainSpeedAuthoring authoring)
        {
            AddComponent(new TrainSpeedComponent
            {
                speed = authoring.Speed,
                friction = authoring.Friction
            });
        }
    }
}