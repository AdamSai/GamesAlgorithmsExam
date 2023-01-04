using DOTS.Components;
using Unity.Entities;
using UnityEngine;


namespace DOTS.Authoring
{

    public class EnableAuthoring : MonoBehaviour
    {
    }

    public class EnableBaker : Baker<EnableAuthoring>
    {
        public override void Bake(EnableAuthoring authoring)
        {
            AddComponent(new EnableComponent { value = false });
        }
    }
}