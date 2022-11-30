using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public enum CommuterComponentState
{
    WALK,
    QUEUE,
    GET_ON_TRAIN,
    GET_OFF_TRAIN,
    WAIT_FOR_STOP,
}

public struct CommuterComponentTask
{
    // A list of CommuterTasks compose the shortest path from start to end PLATFORM
    // The distinations in CommuterTask is calculated when the task starts, therefore we don't need it here
    public CommuterState state;
    public Entity startPlatform, endPlatform;

    public CommuterComponentTask(CommuterState _state, Entity startPlatform, Entity endPlatform)
    {
        state = _state;
        this.startPlatform = startPlatform;
        this.endPlatform = endPlatform;
    }
}

public struct CommuterComponent : IComponentData
{
    public NativeArray<Vector3> destinations;
    public NativeArray<CommuterComponentTask> tasks;
    public int destinationIndex;
}