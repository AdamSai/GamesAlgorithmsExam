using DOTS.Components;
using Unity.Entities;
using UnityEngine;

namespace DOTS.Authoring
{

    public class AmountOfTrainsInLineAuthoring : MonoBehaviour
    {
    }

    public class AmountOfTrainsInLineBaker : Baker<AmountOfTrainsInLineAuthoring>
    {
        public override void Bake(AmountOfTrainsInLineAuthoring authoring)
        {
            AddComponent(new AmountOfTrainsInLineComponent { });
        }
    }
}