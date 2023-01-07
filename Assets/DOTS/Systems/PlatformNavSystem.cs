using Assets.DOTS.Components;
using System.Collections;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Assets.DOTS.Systems
{
    [UpdateAfter(typeof(SetupRailSystem))]
    public partial struct PlatformNavSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {

        }

        public void OnDestroy(ref SystemState state)
        {
            
        }

        public void OnUpdate(ref SystemState state)
        {
            // Setting nav points for all platforms
            Debug.Log("Nav System!");
            EntityCommandBuffer ECB = new EntityCommandBuffer(Allocator.Persistent);

            var platformNavJob = new PlatformNavJob
            {
                ECB = ECB,
                navTransforms = state.GetComponentLookup<LocalToWorld>(),
                navPoints = state.GetComponentLookup<NavPointComponent>(),
                EM = state.EntityManager
            };
            state.Dependency = platformNavJob.Schedule(state.Dependency);

            state.Dependency.Complete();
            ECB.Dispose();
        }
    }

    public partial struct PlatformNavJob : IJobEntity
    {
        public EntityCommandBuffer ECB;
        public ComponentLookup<LocalToWorld> navTransforms;
        public ComponentLookup<NavPointComponent> navPoints;
        public EntityManager EM;

        public void Execute(in NavTag tag, in Entity entity, ref PlatformComponent platform)
        {
            Debug.Log("Nav Job! before");
            if (!platform.init)
                return;

            var buffer = EM.GetBuffer<LinkedEntityGroup>(entity);
            bool done = false;
            Debug.Log("Nav Job! after");
            foreach (var item in buffer)
            {
                Entity e = item.Value;

                if (EM.HasComponent<NavPointComponent>(e))
                {
                    done = true;
                    Debug.Log("Setting nav point: " + navPoints[e].pointID);
                    switch (navPoints[e].pointID)
                    {
                        case 0:
                            platform.platform_entrance0 = navTransforms[e].Position;
                            break;
                        case 1:
                            platform.platform_entrance1 = navTransforms[e].Position;
                            break;
                        case 2:
                            platform.platform_entrance2 = navTransforms[e].Position;
                            break;
                        case 10:
                            // Exit nav points IDs are offset by 10
                            platform.platform_exit0 = navTransforms[e].Position;
                            break;
                        case 11:
                            platform.platform_exit1 = navTransforms[e].Position;
                            break;
                        case 12:
                            platform.platform_exit2 = navTransforms[e].Position;
                            break;
                        default:
                            Debug.Log("ERROR: Nav points ID don't match!");
                            break;
                    }
                }
            }
            if (done)
                ECB.RemoveComponent<NavTag>(entity);
        }
    }
}