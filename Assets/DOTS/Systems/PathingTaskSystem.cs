using System.Collections;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using System.Linq;
using DOTS.Components;
using static UnityEngine.EventSystems.EventTrigger;
using Assets.DOTS.Utility.Stack;

namespace Assets.DOTS.Systems
{
    public partial struct PathingTaskSystem : ISystem
    {
        private EntityQuery platformQuery;

        public void OnCreate(ref SystemState state)
        {
            platformQuery =
            new EntityQueryBuilder(Allocator.Persistent).WithAll<PlatformComponent>().Build(ref state);
        }

        public void OnDestroy(ref SystemState state)
        {

        }

        public void OnUpdate(ref SystemState state)
        {
            if (SystemAPI.Time.ElapsedTime < 2.6f)
                return;

            //return; // TODO: remove this and make it work
            var platformComponents = state.GetComponentLookup<PlatformComponent>();
            platformComponents.Update(ref state);

            var platformEntities =
            platformQuery.ToEntityArray(Allocator.Persistent);

            var job = new PathingTaskJob { 
                platformComponents = platformComponents,
                platformEntities = platformEntities
            };

            state.Dependency = job.Schedule(state.Dependency);

            state.Dependency.Complete();
        }
    }

    public partial struct PathingTaskJob : IJobEntity
    {
        public NativeArray<Entity> platformEntities;
        public ComponentLookup<PlatformComponent> platformComponents;
        //public ComponentLookup<CarriageComponentDummy> carriageComponents;
        //public Entity startPlatform;
        //public Entity endPlatform;

        public void Execute(in Entity entity, ref CommuterComponent commuter)
        {
            if (commuter.tasks.IsEmpty && platformEntities.IsCreated)
            {
                //Debug.Log("This far: 0");
                // No task: get random destination?
                int r = RNG(entity.Index + commuter.r, platformEntities.Length-1);
                if (commuter.currentPlatform == platformEntities[r])
                {
                    r = RNG(r, platformEntities.Length - 1);
                    commuter.r = r;
                    return;
                }
                //Debug.Log("This far: 1. r is " + r);
                //Debug.Log(platformEntities[r]);
                Entity e = platformEntities[r];
                //Debug.Log("This far: 2");
                NativeList<Entity> path = Pathfinding.GetPath(platformEntities, platformComponents, commuter.currentPlatform, e);

                //Debug.Log("This far: 3. Path length: " + path.Length);
                for (int i = 0; i < path.Length - 1; i++)
                {
                    Entity to = path[i];
                    Entity from = path[i + 1];
                    if (platformComponents[from].neighborPlatforms.Contains(to))
                    {
                        // Change platform
                        commuter.tasks.Push(new CommuterComponentTask(CommuterState.WALK, from, to));
                    } else
                    {
                        // Get on train
                        commuter.tasks.Push(new CommuterComponentTask(CommuterState.GET_OFF_TRAIN, from, to));
                        commuter.tasks.Push(new CommuterComponentTask(CommuterState.WAIT_FOR_STOP, from, to));
                        commuter.tasks.Push(new CommuterComponentTask(CommuterState.GET_ON_TRAIN, from, to));
                        commuter.tasks.Push(new CommuterComponentTask(CommuterState.QUEUE, from, to));
                    }
                }
                //commuter.tasks.Push(new CommuterComponentTask(CommuterState.SPAWN_WALK, commuter.currentPlatform, commuter.currentPlatform));
                //Debug.Log("This far: 4. Task list length: " + commuter.tasks.Length + ". Path length: " + path.Length);
                //if (!commuter.tasks.IsEmpty)
                //    Debug.Log("This far: 5. Start entity: " + path.NextStackElement() + ". First task entity: " + commuter.tasks.NextStackElement().endPlatform);
                
                path.Dispose();
            }
        }

        public static int RNG(int seed, int size)
        {
            // Random number generator
            int a = 42;
            int r = (a + seed * a + 1) % size;
            r = (r * a + 1) % size;
            r = (r * a + 1) % size;
            r = (r * a + 1) % size;
            r = (r * a + 1) % size;
            r = (r * a + 1) % size;
            r = (r * a + 1) % size;
            r = (r * a + 1) % size;
            r = (r * a + 1) % size;
            return r;
        }
    }
}