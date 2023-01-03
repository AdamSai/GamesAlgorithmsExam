
using DOTS.Components;
using Unity.Entities;
using UnityEngine;

namespace DOTS.Authoring
{
    public class TrainDataAuthoring : MonoBehaviour
    {

    }
    public class TrainDataBaker : Baker<TrainDataAuthoring>
    {
        public override void Bake(TrainDataAuthoring authoring)
        {
            AddComponent(new TrainDataComponent
            {

            });
            AddComponent(new TrainPositionComponent());
        }
    }
}