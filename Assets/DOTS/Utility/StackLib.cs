using System;
using System.Collections;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Assets.DOTS.Utility.Stack
{
    public static class StackLib
    {
        public static void Push<T>(this NativeList<T> stack, T element) where T : unmanaged
        {
            stack.Add(element);
        }

        public static T Pop<T>(this NativeList<T> stack) where T : unmanaged
        {
            T element = stack[stack.Length - 1];
            stack.RemoveAt(stack.Length - 1);
            return element;
        }

        public static T NextStackElement<T>(this NativeList<T> stack) where T : unmanaged
        {
            return stack[stack.Length - 1];
        }
    }
}