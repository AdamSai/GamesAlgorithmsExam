using DOTS.Components;
using System.Collections;
using Unity.Entities;
using UnityEngine;
using Assets.DOTS;
using Assets.DOTS.Components;

namespace Assets.DOTS.Authoring
{
    public class NavPointAuthoring : MonoBehaviour
    {
        public int pointID;
    }

    public class NavPointBaker : Baker<NavPointAuthoring>
    {
        public override void Bake(NavPointAuthoring authoring)
        {
            AddComponent(new NavPointComponent { pointID = authoring.pointID } );
        }
    }
}