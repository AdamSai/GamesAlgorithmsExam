using System.Collections;
using Unity.Entities;
using UnityEngine;

namespace Assets.DOTS.Components
{
    public struct NavPointComponent : IComponentData
    {
        public int pointID;
    }
}