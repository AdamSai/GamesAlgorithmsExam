using Unity.Entities;
using UnityEngine;

namespace DOTS.Authoring
{
    public class TrainIDAuthoring : MonoBehaviour
    {
        public byte Carriages;
        public float CarriagesSpeed;
        public Entity Carriage;
    }

    public class TrainIDBaker : Baker<TrainIDAuthoring>
    {
        public override void Bake(TrainIDAuthoring authoring)
        {
            AddComponent(new DOTS.Components.TrainIDComponent
            {

            });
        }
    }
}