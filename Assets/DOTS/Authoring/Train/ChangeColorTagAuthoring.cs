using DOTS.Components;
using Unity.Entities;
using UnityEngine;

namespace DOTS.Authoring
{

    public class ChangeColorTagAuthoring : MonoBehaviour
    {
    }

    public class ChangeColorTagBaker : Baker<ChangeColorTagAuthoring>
    {
        public override void Bake(ChangeColorTagAuthoring authoring)
        {
            AddComponent(new ChangeColorTag { });
        }
    }
}