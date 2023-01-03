using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DOTS.Jobs
{
    public partial struct CommuterMovementJob : IJobEntity
    {
        public void Execute(in LocalToWorld localToWorld, in CommuterComponent commuter, ref WalkComponent walk)
        {
            if (walk.destinations.IsEmpty)
                return;
            
            if (DOT(walk.velocity, walk.destinations[walk.destinations.Length - 1] - localToWorld.Position) < 0)
            {
                // Reached intermediate destination
                walk.destinations.RemoveAt(walk.destinations.Length - 1);
                walk.velocity = new float3(0f, 0f, 0f);
            }
            else
            {
                // Didn't reach intermediate destination, instead change velocity
                walk.velocity =
                    math.normalize(walk.destinations[walk.destinations.Length - 1] - localToWorld.Position) * walk.speed;
            }
        }

        public static float DOT(float3 a, float3 b)
        {
            return a[0] * b[0] + a[1] * b[1] + a[2] * b[2];
        }
    }
}

/*
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DOTS.Jobs
{
    public partial struct CommuterMovementJob : IJobEntity
    {
        public void Execute(in LocalToWorld localToWorld, ref CommuterComponent commuter)
        {
            CommuterComponentTask currentTask = commuter.tasks[commuter.tasks.Length - 1];

            switch (currentTask.state)
            {
                case CommuterState.WALK:
                    if (commuter.destinations.IsEmpty)
                    {
                        // Reached last destination, therefore change
                    }
                    else if (DOT(commuter.velocity, commuter.destinations[commuter.destinationIndex - 1] - localToWorld.Position) < 0)
                    {
                        // Reached intermediate destination
                        commuter.destinations.RemoveAt(commuter.destinations.Length - 1);
                        commuter.velocity = new float3(0f,0f,0f);
                    } else
                    {
                        // Didn't reach intermediate destination, instead change velocity
                        commuter.velocity = 
                            math.normalize(commuter.destinations[commuter.destinationIndex - 1] - localToWorld.Position) * commuter.speed;
                    }
                    break;
                case CommuterState.QUEUE:
                    // Get in queue: how?
                    break;
                case CommuterState.GET_ON_TRAIN:
                    // Get on train: how?
                    break;
                case CommuterState.GET_OFF_TRAIN:
                    // Get off train: how?
                    break;
                case CommuterState.WAIT_FOR_STOP:
                    // Wait for stop: how?
                    break;
            }
        }

        public static float DOT(float3 a, float3 b)
        {
            return a[0] * b[0] + a[1] * b[1] + a[2] * b[2];
        }
    }
}
*/