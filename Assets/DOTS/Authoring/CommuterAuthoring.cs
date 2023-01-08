using Assets.DOTS.Components;
using System.Collections;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEditor.Rendering;
using UnityEngine;

namespace Assets.DOTS.Authoring
{
    public class CommuterAuthoring : MonoBehaviour
    {
        public float speed = 4f;
    }

    public class CommuterBaker : Baker<CommuterAuthoring>
    {
        public override void Bake(CommuterAuthoring authoring)
        {
            NativeList<float3> destinations = new NativeList<float3>(Allocator.Persistent);

            AddComponent(new WalkComponent
            {
                destinations = destinations,
                speed = authoring.speed,
                velocity = new float3(0, 0, 0)
            });

            AddComponent(new CommuterComponent
            {
                tasks = new NativeList<CommuterComponentTask>(Allocator.Persistent),
                //currentCarriage = Entity.Null,
                currentPlatform = Entity.Null,
                r = 0,
            });

            AddComponent(new PassengerComponent
            {
                currentCarriage = Entity.Null,
                carriageSeat = Entity.Null
            });

            AddComponent(new CommuterQueuerComponent { 
                inQueue = false,
                queueIndex = 0,
                readyForBoarding = false,
                finishedTask = false,
                queueStartPosition = new float3(0),
                queueDirection = new float3(0),
                state = QueueState.None,
            });
        }
    }
}