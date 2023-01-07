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
                currentPlatform = Entity.Null,
                r = 0,
            });

            AddComponent(new PassengerComponent
            {
                currentCarriage = Entity.Null,
                carriageSeat = Entity.Null
            });
        }
    }
}