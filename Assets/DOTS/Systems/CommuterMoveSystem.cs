﻿using Assets.DOTS.Utility;
using JetBrains.Annotations;
using System.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Assets.DOTS.Utility.Stack;
using Unity.Burst;

namespace Assets.DOTS.Systems
{
    [BurstCompile]
    public partial struct CommuterMoveSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {

        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            //Time.ElapsedTime
            var job = new CommuterMovementJob { deltaTime = SystemAPI.Time.DeltaTime };

            state.Dependency = job.Schedule(state.Dependency);

            state.Dependency.Complete();
        }

        [BurstCompile]
        public partial struct CommuterMovementJob : IJobEntity
        {
            public float deltaTime;

            public void Execute(ref LocalTransform transform, in CommuterComponent commuter, ref WalkComponent walk)
            {
                if (walk.destinations.IsEmpty)
                    return;

                if (DOT(walk.velocity, walk.destinations.NextStackElement() - transform.Position) < 0f && 
                    math.distance(transform.Position, walk.destinations.NextStackElement()) < 0.5f)
                {
                    // Reached intermediate destination
                    walk.destinations.Pop();
                    walk.velocity = new float3(0f, 0f, 0f);
                }
                else
                {
                    // Didn't reach intermediate destination, instead change velocity
                    walk.velocity =
                        math.normalize(walk.destinations.NextStackElement() - transform.Position) * walk.speed;
                }

                float3 newPos = transform.Position + walk.velocity * deltaTime;

                // Update transform
                transform.Position = newPos;
            }

            public static float DOT(float3 a, float3 b)
            {
                return a[0] * b[0] + a[1] * b[1] + a[2] * b[2];
            }
        }
    }
}