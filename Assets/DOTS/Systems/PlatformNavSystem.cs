using Assets.DOTS.Components;
using System.Collections;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.DOTS.Systems
{
    [UpdateAfter(typeof(SetupSeatsSystem))]
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
            if (SystemAPI.Time.ElapsedTime < 2)
                return;

            // Setting nav points for all platforms
            Debug.Log("Nav System!");
            EntityCommandBuffer ECB = new EntityCommandBuffer(Allocator.Persistent);

            var platformNavJob = new PlatformNavJob
            {
                ECB = ECB,
                worldTransforms = state.GetComponentLookup<WorldTransform>(),
                localTransforms = state.GetComponentLookup<LocalTransform>(),
                navPoints = state.GetComponentLookup<NavPointComponent>(),
                EM = state.EntityManager
            };
            state.Dependency = platformNavJob.Schedule(state.Dependency);

            state.Dependency.Complete();
            ECB.Playback(state.EntityManager);
            ECB.Dispose();

            ECB = new EntityCommandBuffer(Allocator.Persistent);
            var spawnJob = new SpawnCommutersJob { ECB = ECB };

            state.Dependency = spawnJob.Schedule(state.Dependency);
            state.Dependency.Complete();
            ECB.Playback(state.EntityManager);
            ECB.Dispose();
        }
    }

    public partial struct SpawnCommutersJob : IJobEntity
    {
        public EntityCommandBuffer ECB;

        public void Execute(ref CommuterSpawnComponent spawner, in Entity entity, in LocalTransform transform, in PlatformComponent platform)
        {
            Debug.Log("Spawning commuter job!");
            // TODO make so only run once
            if (spawner.hasSpawned)
                return;

            for (int i = 0; i < spawner.amount; i++)
            {
                Entity commuter = ECB.Instantiate(spawner.commuter);
                Debug.Log("Spawning commuter: " + commuter);

                ECB.SetComponent<LocalTransform>(commuter, LocalTransform.FromPosition(platform.platform_exit0));
                ECB.SetComponent<CommuterComponent>(commuter, new CommuterComponent
                {
                    tasks = new NativeList<CommuterComponentTask>(Allocator.Persistent),
                    currentPlatform = entity
                });
            }

            spawner.hasSpawned = true;

            ECB.RemoveComponent<CommuterSpawnComponent>(entity);
        }
    }

    public partial struct PlatformNavJob : IJobEntity
    {
        public EntityCommandBuffer ECB;
        public ComponentLookup<WorldTransform> worldTransforms;
        public ComponentLookup<LocalTransform> localTransforms;
        public ComponentLookup<NavPointComponent> navPoints;
        public EntityManager EM;

        public void Execute(ref NavTag tag, in Entity entity, ref PlatformComponent platform)
        {
            // TODO make so only run once
            Debug.Log("Nav Job! before");
            if (!platform.init)
                return;

            if (tag.init)
            {
                ECB.RemoveComponent<NavTag>(entity);
                return;
            }

            tag.init = true;

            var buffer = EM.GetBuffer<LinkedEntityGroup>(entity);
            Debug.Log("Nav Job! after");
            foreach (var item in buffer)
            {
                Entity e = item.Value;

                if (EM.HasComponent<NavPointComponent>(e))
                {
                    Debug.Log("Setting nav point: " + navPoints[e].pointID);
                    switch (navPoints[e].pointID)
                    {
                        case 0:
                            Debug.Log($"{worldTransforms[e].Position} vs {localTransforms[entity].Position}");
                            platform.platform_entrance0 = worldTransforms[e].Position;
                            break;
                        case 1:
                            platform.platform_entrance1 = worldTransforms[e].Position;
                            break;
                        case 2:
                            platform.platform_entrance2 = worldTransforms[e].Position;
                            break;
                        case 10:
                            // Exit nav points IDs are offset by 10
                            platform.platform_exit0 = worldTransforms[e].Position;
                            break;
                        case 11:
                            platform.platform_exit1 = worldTransforms[e].Position;
                            break;
                        case 12:
                            platform.platform_exit2 = worldTransforms[e].Position;
                            break;
                        default:
                            Debug.Log("ERROR: Nav points ID don't match!");
                            break;
                    }
                }
            }
        }
    }
}