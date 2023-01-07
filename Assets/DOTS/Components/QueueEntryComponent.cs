using System.Collections;
using Unity.Entities;
using UnityEngine;

namespace Assets.DOTS.Components
{
    public struct QueueEntryComponent : IBufferElementData
    {
        public Entity val;
    }
}