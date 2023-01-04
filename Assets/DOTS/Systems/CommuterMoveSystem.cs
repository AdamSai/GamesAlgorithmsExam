using JetBrains.Annotations;
using System.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.DOTS.Systems
{
    public partial struct CommuterMoveSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {

        }

        public void OnDestroy(ref SystemState state)
        {

        }

        public void OnUpdate(ref SystemState state)
        {
            var job = new CommuterMovementJob();

            state.Dependency = job.Schedule(state.Dependency);

            state.Dependency.Complete();
        }

        public partial struct CommuterMovementJob : IJobEntity
        {
            public void Execute(ref LocalTransform transform, in CommuterComponent commuter, ref WalkComponent walk)
            {
                if (walk.destinations.IsEmpty)
                    return;

                if (DOT(walk.velocity, walk.destinations[walk.destinations.Length - 1] - transform.Position) < 0)
                {
                    // Reached intermediate destination
                    walk.destinations.RemoveAt(walk.destinations.Length - 1);
                    walk.velocity = new float3(0f, 0f, 0f);
                }
                else
                {
                    // Didn't reach intermediate destination, instead change velocity
                    walk.velocity =
                        math.normalize(walk.destinations[walk.destinations.Length - 1] - transform.Position) * walk.speed;
                }

                float3 newPos = transform.Position + walk.velocity;

                transform.Position = newPos;
            }

            public static float DOT(float3 a, float3 b)
            {
                return a[0] * b[0] + a[1] * b[1] + a[2] * b[2];
            }
        }
    }
}