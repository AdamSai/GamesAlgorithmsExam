using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public static class Pathfinding
{
    /// <summary>
    /// An implementation of dijkstra's algorithm, modified to stop when the goal is reached.
    /// https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm
    /// </summary>
    /// <param name="platformEntities"></param>
    /// <param name="platformComponents"></param>
    /// <param name="startPlatform"></param>
    /// <param name="endPlatform"></param>
    /// <returns></returns>
    public static NativeList<Entity> GetPath(NativeArray<Entity> platformEntities, 
        ComponentLookup<PlatformComponent> platformComponents, 
        Entity startPlatform, Entity endPlatform)
    {
        NativeHashMap<Entity, float> dist = new NativeHashMap<Entity, float>(platformEntities.Length, Allocator.Temp);
        NativeHashMap<Entity, Entity> previous = new NativeHashMap<Entity, Entity>(platformEntities.Length, Allocator.Temp);

        NativeList<Entity> unexplored = new NativeList<Entity>(Allocator.Temp);
        NativeHashMap<Entity, bool> explored = new NativeHashMap<Entity, bool>(platformEntities.Length, Allocator.Temp);

        foreach (var item in platformEntities)
        {
            // Add or set value here?
            dist.Add(item, float.MaxValue);
            previous.Add(item, item);
            explored.Add(item, false);
            unexplored.Add(item);
        }

        dist[startPlatform] = 0;
        unexplored.Add(startPlatform);

        bool done = false;
        int iterations = 100;
        while (!done && iterations > 0)
        {
            iterations++;
            Entity u = GetShortest(unexplored, dist);
            int i = unexplored.IndexOf(u);
            unexplored.RemoveAt(i);
            explored[u] = true;

            if (u == endPlatform)
                break;

            // Add neighbors of platform
            foreach (Entity neighbor in platformComponents[u].neighborPlatforms)
            {
                if (explored[neighbor])
                    continue;

                float alt = dist[u] + 1;
                if (alt < dist[neighbor])
                {
                    dist[neighbor] = alt;
                    previous[neighbor] = u;
                }
            }

            // Add next platform
            Entity next = platformComponents[u].nextPlatform;
            if (!explored[next])
            {
                float alt = dist[u] + 10;
                if (alt < dist[next])
                {
                    dist[next] = alt;
                    previous[next] = u;
                }
            }
        }

        Entity t = endPlatform;
        NativeList<Entity> path = new NativeList<Entity>(Allocator.Temp);
        while(t != startPlatform)
        {
            path.Add(t);
            t = previous[t];
        }
        path.Add(startPlatform);

        return path;
        //return (NativeList<Entity>)path.Reverse(); // Is reverse in-place? Otherwise this doesn't work.
    }

    private static Entity GetShortest(NativeList<Entity> unexplored, NativeHashMap<Entity, float> dist)
    {
        float d = float.MaxValue;
        Entity retval = unexplored[0];
        for (int i = 0; i < unexplored.Length; i++)
        {
            if (dist[unexplored[i]] < d)
            {
                d = dist[unexplored[i]];
                retval = unexplored[i];
            }
        }
        return retval;
    }
}