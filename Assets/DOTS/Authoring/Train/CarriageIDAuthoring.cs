
using DOTS.Components;
using Unity.Entities;
using UnityEngine;

namespace DOTS.Authoring
{

    public class CarriageIDAuthoring : MonoBehaviour
    {
        public int ID;
        public int LineIndex;
        public int TrainIndex;
    }
    public class CarriageIDBaker : Baker<CarriageIDAuthoring>
    {
        public override void Bake(CarriageIDAuthoring authoring)
        {
            AddComponent(new CarriageIDComponent
            {
                id = authoring.ID,
                lineIndex = authoring.LineIndex,
                trainIndex = authoring.TrainIndex
            });
        }
    }
}