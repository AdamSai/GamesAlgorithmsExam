using DOTS.Components;
using Unity.Entities;
using UnityEngine;

namespace DOTS.Authoring
{
    public class MetroLineAuthoring : MonoBehaviour
    {
        [Range(0f, 1f)]
        public float BezierHandleReach = 0.15f;
    }


    public class MetroLineBaker : Baker<MetroLineAuthoring>
    {
        public override void Bake(MetroLineAuthoring authoring)
        {
            AddComponent(new BezierPathComponent
            {
                BezierHandleReach = authoring.BezierHandleReach
            });
        }
    }
}