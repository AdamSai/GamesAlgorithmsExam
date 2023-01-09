﻿using System.Collections;
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

            var job = new PathingTaskJob
            {
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
                // No task: get random destination?
                int r = RNG(entity.Index + commuter.r, platformEntities.Length - 1);
                if (commuter.currentPlatform == platformEntities[r])
                {
                    r = RNG(r, platformEntities.Length - 1);
                    commuter.r = r;
                    return;
                }
                Entity e = platformEntities[r];
                NativeList<Entity> path = Pathfinding.GetPath(platformEntities, platformComponents, commuter.currentPlatform, e);

                // Complex task list - almost finalised but cursed
                //for (int i = 0; i < path.Length - 1; i++)
                //{
                //    Entity to = path[i];
                //    Entity from = path[i + 1];

                //    if (platformComponents[from].neighborPlatforms.Contains(to))
                //    {
                //        //if (i == path.Length - 2)
                //        //{
                //        //    // First task: is to move
                //        //    commuter.tasks.Push(new CommuterComponentTask(CommuterState.WALK, from, to));
                //        //}
                //        //else 
                //        if (!commuter.tasks.IsEmpty)
                //        {
                //            var nextTask = commuter.tasks.NextStackElement();

                //            if (nextTask.state == CommuterState.WAIT_FOR_STOP)
                //            {
                //                commuter.tasks.Push(new CommuterComponentTask(CommuterState.GET_ON_TRAIN, nextTask.startPlatform, nextTask.endPlatform));
                //                commuter.tasks.Push(new CommuterComponentTask(CommuterState.QUEUE, nextTask.startPlatform, nextTask.endPlatform));
                //            }

                //            // Change platform
                //            commuter.tasks.Push(new CommuterComponentTask(CommuterState.WALK, from, to));
                //        }
                //        else
                //        {
                //            commuter.tasks.Push(new CommuterComponentTask(CommuterState.WALK, from, to));
                //        }
                //    }
                //    else
                //    {
                //        if (i == path.Length - 2)
                //        {
                //            commuter.tasks.Push(new CommuterComponentTask(CommuterState.WAIT_FOR_STOP, from, to));
                //            commuter.tasks.Push(new CommuterComponentTask(CommuterState.GET_ON_TRAIN, from, to));
                //            commuter.tasks.Push(new CommuterComponentTask(CommuterState.QUEUE, from, to));
                //        }
                //        else if (!commuter.tasks.IsEmpty)
                //        {
                //            var nextTask = commuter.tasks.NextStackElement();

                //            if (nextTask.state == CommuterState.WALK)
                //            {
                //                commuter.tasks.Push(new CommuterComponentTask(CommuterState.GET_OFF_TRAIN, from, to));
                //            }

                //            // Get on train
                //            commuter.tasks.Push(new CommuterComponentTask(CommuterState.WAIT_FOR_STOP, from, to));
                //        }
                //        else
                //        {
                //            commuter.tasks.Push(new CommuterComponentTask(CommuterState.GET_OFF_TRAIN, from, to));
                //            commuter.tasks.Push(new CommuterComponentTask(CommuterState.WAIT_FOR_STOP, from, to));
                //        }


                //    }
                //}
                //for (int i = 0; i < path.Length - 1; i++)
                //{
                //    Entity to = path[i];
                //    Entity from = path[i + 1];
                //    if (platformComponents[from].neighborPlatforms.Contains(to))
                //    {
                //        // Change platform
                //        commuter.tasks.Push(new CommuterComponentTask(CommuterState.WALK, from, to));
                //    }
                //    else
                //    {
                //        // Get on train
                //        commuter.tasks.Push(new CommuterComponentTask(CommuterState.GET_OFF_TRAIN, from, to));
                //        commuter.tasks.Push(new CommuterComponentTask(CommuterState.WAIT_FOR_STOP, from, to));
                //        commuter.tasks.Push(new CommuterComponentTask(CommuterState.GET_ON_TRAIN, from, to));
                //        commuter.tasks.Push(new CommuterComponentTask(CommuterState.QUEUE, from, to));
                //    }
                //}
                for (int i = 0; i < 10000; i++)
                {
                    commuter.tasks.Push(new CommuterComponentTask(CommuterState.WAIT_FOR_STOP, Entity.Null, Entity.Null));
                }
                var from = path.Pop();
                var to = path.Pop();
                commuter.tasks.Push(new CommuterComponentTask(CommuterState.GET_ON_TRAIN, from, to));
                commuter.tasks.Push(new CommuterComponentTask(CommuterState.QUEUE, from, to));
                commuter.tasks.Push(new CommuterComponentTask(CommuterState.START, Entity.Null, Entity.Null));
                //commuter.tasks.Push(new CommuterComponentTask(CommuterState.SPAWN_WALK, commuter.currentPlatform, commuter.currentPlatform));
                //if (!commuter.tasks.IsEmpty)

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