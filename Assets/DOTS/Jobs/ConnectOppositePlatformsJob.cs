using DOTS.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DOTS.Jobs
{
    public partial struct ConnectOppositePlatformsJob : IJobEntity
    {
        public void Execute(in OppositePlatformComponent oppositePlatformComponent,
            ref PlatformComponent platformComponent)
        {
            Debug.Log("Connect opposite platforms");
            platformComponent.oppositePlatform = oppositePlatformComponent.OppositePlatform.entity;
            platformComponent.init = true;
        }

        public static Vector3 ToEulerAngles(Quaternion q)
        {
            float3 angles;

            // roll (x-axis rotation)
            double sinr_cosp = 2 * (q.w * q.x + q.y * q.z);
            double cosr_cosp = 1 - 2 * (q.x * q.x + q.y * q.y);
            angles.x = (float) math.atan2(sinr_cosp, cosr_cosp);

            // pitch (y-axis rotation)
            double sinp = 2 * (q.w * q.y - q.z * q.x);
            if (math.abs(sinp) >= 1)
                angles.y = (float) CopySign(math.PI / 2, sinp); // use 90 degrees if out of range
            else
                angles.y = (float) math.asin(sinp);

            // yaw (z-axis rotation)
            double siny_cosp = 2 * (q.w * q.z + q.x * q.y);
            double cosy_cosp = 1 - 2 * (q.y * q.y + q.z * q.z);
            angles.z = (float) math.atan2(siny_cosp, cosy_cosp);

            return angles;
        }

        private static double CopySign(double a, double b)
        {
            return math.abs(a) * math.sign(b);
        }
    }
}