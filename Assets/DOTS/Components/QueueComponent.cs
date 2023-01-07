using System.Collections;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Assets.DOTS.Components
{
    public struct QueueComponent : IComponentData
    {
        public NativeList<Entity> toQueue;

        public NativeList<Entity> queue0;
        public NativeList<Entity> queue1;
        public NativeList<Entity> queue2;
        public NativeList<Entity> queue3;
        public NativeList<Entity> queue4;
    }
}