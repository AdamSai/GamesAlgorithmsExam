using DOTS.Components;
using UnityEngine;

namespace DOTS.Utility
{
    public class TimerUtility
    {
        public static bool RunTimer(ref TimerComponent timerComponent, float deltaTime)
        {
            timerComponent.isRunning = true;
            if (timerComponent.isRunning)
            {
                timerComponent.time += deltaTime;
                if (timerComponent.time >= timerComponent.duration)
                {
                    timerComponent.time = 0;
                    timerComponent.isRunning = false;
                    Debug.Log("Timer stopped");

                    return true;
                }
            }

            return false;
        }
    }
}