using System.Collections;
using Unity.Collections;
using UnityEngine;

namespace Assets.DOTS.Utility.Queue
{
    public static class QueueLib
    {
        public static void Enqueue<T>(this NativeList<T> queue, T element) where T : unmanaged
        {
            queue.Add(element);
        }

        public static T Dequeue<T>(this NativeList<T> queue) where T : unmanaged
        {
            T element = queue[0];
            queue.RemoveAt(0);
            return element;
        }

        public static T NextElement<T>(this NativeList<T> queue) where T : unmanaged
        {
            return queue[0];
        }
    }
}