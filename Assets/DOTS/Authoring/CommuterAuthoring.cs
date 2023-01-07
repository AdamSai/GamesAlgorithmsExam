using System.Collections;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.DOTS.Authoring
{
    public class CommuterAuthoring : MonoBehaviour
    {

    }

    public class CommuterBaker : Baker<CommuterAuthoring>
    {
        public override void Bake(CommuterAuthoring authoring)
        {
            NativeList<float3> destinations = new NativeList<float3>(Allocator.Persistent);
            destinations.Add(new float3(5, 5, 0));
            destinations.Add(new float3(5, 0, 0));

            AddComponent(new WalkComponent
            {
                destinations = destinations,
                speed = 2f,
                velocity = new float3(0, 0, 0)
            });

            AddComponent(new CommuterComponent
            {
                tasks = new NativeList<CommuterComponentTask>(Allocator.Persistent),
                //currentCarriage = Entity.Null,
                currentPlatform = Entity.Null
            });

            AddComponent(new PassengerComponent
            {
                currentCarriage = Entity.Null,
                carriageSeat = Entity.Null
            });
        }
    }
}