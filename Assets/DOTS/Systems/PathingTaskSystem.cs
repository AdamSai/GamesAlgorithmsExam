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
            //return; // TODO: remove this and make it work
            var platformComponents = state.GetComponentLookup<PlatformComponent>();

            var platformEntities =
            platformQuery.ToEntityArray(Allocator.Persistent);

            var job = new PathingTaskJob { 
                platformComponents = state.GetComponentLookup<PlatformComponent>(),
                platformEntities = platformEntities
            };

            state.Dependency = job.Schedule(state.Dependency);

            state.Dependency.Complete();
            platformEntities.Dispose();
            //platformQuery.Dispose();
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
                Debug.Log("This far: 0");
                // No task: get random destination?
                int r = RNG(entity.Index, platformEntities.Length-1);
                Debug.Log("This far: 1. r is " + r);
                Debug.Log(platformEntities[r]);
                Entity e = platformEntities[r];
                Debug.Log("This far: 2");
                NativeList<Entity> path = Pathfinding.GetPath(platformEntities, platformComponents, commuter.currentPlatform, e);

                NativeList<CommuterComponentTask> tasks = commuter.tasks;
                Debug.Log("This far: 3");
                for (int i = 0; i < path.Length - 2; i++)
                {
                    Entity from = path[i];
                    Entity to = path[i + 1];
                    tasks.Push(new CommuterComponentTask(CommuterState.GET_OFF_TRAIN, from, to));
                    tasks.Push(new CommuterComponentTask(CommuterState.WAIT_FOR_STOP, from, to));
                    tasks.Push(new CommuterComponentTask(CommuterState.GET_ON_TRAIN, from, to));
                    tasks.Push(new CommuterComponentTask(CommuterState.QUEUE, from, to));
                    if (platformComponents[from].neighborPlatforms.Contains(to))
                    {
                        // Change platform
                        tasks.Push(new CommuterComponentTask(CommuterState.WALK, from, to));
                    }
                }
                path.Dispose();
                Debug.Log("This far: 4");
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
            return r;
        }
    }
}