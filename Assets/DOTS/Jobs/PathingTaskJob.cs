using DOTS.Components;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace DOTS.Jobs
{
    public partial struct PathingTaskJob : IJobEntity
    {
        public NativeArray<Entity> platformEntities;
        public ComponentLookup<PlatformComponent> platformComponents;
        public Entity startPlatform; 
        public Entity endPlatform;

        public void Execute(ref CommuterComponent commuter)
        {
            if (commuter.tasks.IsEmpty)
            {
                // No task: get random destination?
                int r = UnityEngine.Random.Range(0, commuter.tasks.Length);
                Entity e = platformEntities[r];

                NativeList<Entity> path = Pathfinding.GetPath(platformEntities, platformComponents, commuter.currentPlatform, e);

                NativeList<CommuterComponentTask> tasks = commuter.tasks;

                for (int i = 0; i < path.Length - 2; i++)
                {
                    Entity from = path[i];
                    Entity to = path[i + 1];
                    if (platformComponents[from].neighborPlatforms.Contains(to))
                    {
                        // Change platform
                        tasks.Add(new CommuterComponentTask(CommuterState.WALK, from, to));
                    }
                    tasks.Add(new CommuterComponentTask(CommuterState.QUEUE, from, to));
                    tasks.Add(new CommuterComponentTask(CommuterState.GET_ON_TRAIN, from, to));
                    tasks.Add(new CommuterComponentTask(CommuterState.WAIT_FOR_STOP, from, to));
                    tasks.Add(new CommuterComponentTask(CommuterState.GET_OFF_TRAIN, from, to));
                }
            }
        }
    }
}