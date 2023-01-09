using Assets.DOTS.Components;
using System.Collections;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.DOTS.Systems
{
    [UpdateAfter(typeof(SetupTrainsSystem))]
    [BurstCompile]
    public partial struct PlatformNavSystem : ISystem
    {
        public ComponentLookup<WorldTransform> worldTransforms;
        public ComponentLookup<LocalTransform> localTransforms;
        public ComponentLookup<NavPointComponent> navPoints;
        public EntityCommandBuffer ecbb;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            worldTransforms = state.GetComponentLookup<WorldTransform>();
            localTransforms = state.GetComponentLookup<LocalTransform>();
            navPoints = state.GetComponentLookup<NavPointComponent>();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (SystemAPI.Time.ElapsedTime < 2)
                return;

            // Setting nav points for all platforms
            EntityCommandBuffer ECB = new EntityCommandBuffer(Allocator.Persistent);

            worldTransforms.Update(ref state);
            localTransforms.Update(ref state);
            navPoints.Update(ref state);
            // Set nav points
            var platformNavJob = new PlatformNavJob
            {
                ECB = ECB,
                worldTransforms = worldTransforms,
                localTransforms = localTransforms,
                navPoints = navPoints,
                EM = state.EntityManager
            };
            state.Dependency = platformNavJob.Schedule(state.Dependency);

            state.Dependency.Complete();
            ECB.Playback(state.EntityManager);
            ECB.Dispose();

            // Spawn commuters
            ecbb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);
            var spawnJob = new SpawnCommutersJob {ECB = ecbb};

            state.Dependency = spawnJob.Schedule(state.Dependency);
            state.Dependency.Complete();
        }
    }

    [BurstCompile]
    public partial struct SpawnCommutersJob : IJobEntity
    {
        public EntityCommandBuffer ECB;

        public void Execute(ref CommuterSpawnComponent spawner,
            in Entity entity, in LocalTransform transform, in PlatformComponent platform, QueueComponent queueC)
        {
            // TODO make so only run once
            if (spawner.hasSpawned)
                return;

            for (int i = 0; i < spawner.amount; i++)
            {
                Entity commuter = ECB.Instantiate(spawner.commuter);

                ECB.SetComponent<LocalTransform>(commuter, LocalTransform.FromPosition(platform.platform_entrance0));
                ECB.SetComponent<CommuterComponent>(commuter, new CommuterComponent
                {
                    tasks = new NativeList<CommuterComponentTask>(Allocator.Persistent),
                    currentPlatform = entity
                });
                ECB.SetComponent(commuter, new WalkComponent
                {
                    speed = 8f,
                    velocity = new float3(0),
                    destinations = new NativeList<float3>(Allocator.Persistent),
                });

                ECB.SetName(commuter, $"Commuter: ({platform.metroLineID}/{platform.platformIndex}/{i})");
            }

            spawner.hasSpawned = true;

            ECB.RemoveComponent<CommuterSpawnComponent>(entity);
        }
    }

    [BurstCompile]
    public partial struct PlatformNavJob : IJobEntity
    {
        public EntityCommandBuffer ECB;
        public ComponentLookup<WorldTransform> worldTransforms;
        public ComponentLookup<LocalTransform> localTransforms;
        public ComponentLookup<NavPointComponent> navPoints;
        public EntityManager EM;

        public void Execute(ref NavTag tag, in Entity entity, ref PlatformComponent platform, ref QueueComponent queueC)
        {
            // TODO make so only run once
            if (!platform.init)
                return;

            if (tag.init)
            {
                ECB.RemoveComponent<NavTag>(entity);
                return;
            }

            tag.init = true;

            var buffer = EM.GetBuffer<LinkedEntityGroup>(entity);
            foreach (var item in buffer)
            {
                Entity e = item.Value;

                if (EM.HasComponent<NavPointComponent>(e))
                {
                    switch (navPoints[e].pointID)
                    {
                        case 0:
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

                        case 100:
                            queueC.queuePoint0 = worldTransforms[e].Position;
                            break;
                        case 101:
                            queueC.queuePoint0_1 = worldTransforms[e].Position;
                            break;
                        case 110:
                            queueC.queuePoint1 = worldTransforms[e].Position;
                            break;
                        case 111:
                            queueC.queuePoint1_1 = worldTransforms[e].Position;
                            break;
                        case 120:
                            queueC.queuePoint2 = worldTransforms[e].Position;
                            break;
                        case 121:
                            queueC.queuePoint2_1 = worldTransforms[e].Position;
                            break;
                        case 130:
                            queueC.queuePoint3 = worldTransforms[e].Position;
                            break;
                        case 131:
                            queueC.queuePoint3_1 = worldTransforms[e].Position;
                            break;
                        case 140:
                            queueC.queuePoint4 = worldTransforms[e].Position;
                            break;
                        case 141:
                            queueC.queuePoint4_1 = worldTransforms[e].Position;
                            break;


                        default:
                            break;
                    }
                }
            }
        }
    }
}