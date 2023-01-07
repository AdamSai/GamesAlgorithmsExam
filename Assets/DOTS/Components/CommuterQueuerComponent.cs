using System.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.DOTS.Components
{
    public struct CommuterQueuerComponent : IComponentData
    {
        public bool inQueue;
        public int queueIndex;
        public bool readyForBoarding;
        public bool finishedTask;
        public QueueState state;
        public float3 queueStartPosition;
        public float3 queueDirection;
    }

    public enum QueueState
    {
        None,
        Entry,
        InQueue,
        ReadyForBoarding,
        Boarding
    }
}