using DOTS.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DOTS.Jobs
{
    public partial struct RotatePlatformsJob : IJobEntity
    {
        public EntityManager EM;
        public void Execute(Entity entity, in OppositePlatformComponent oppositePlatformComponent, ref LocalTransform localTransform, ref PlatformComponent platformComponent)
        {
            Debug.Log("Rotate platforms job");

            if (!oppositePlatformComponent.EulorRotation.Equals(float3.zero))
            {
                Debug.Log("Rotating");
                var rotation = EM.GetComponentData<LocalTransform>(oppositePlatformComponent.OppositePlatform).Rotation;
                localTransform.Rotation = Quaternion.Euler(ToEulerAngles(rotation) + new float3(0f, 180f, 0f));
                // EM.SetComponentData(entity, localTransform);
            }
            platformComponent.oppositePlatform = oppositePlatformComponent.OppositePlatform;
            
            // _oppositePlatform.transform.eulerAngles =
            //     _ouboundPlatform.transform.rotation.eulerAngles + new Vector3(0f, 180f, 0f);
            // ;
            //
            // // pair these platforms as opposites
            // _ouboundPlatform.PairWithOppositePlatform(_oppositePlatform);
            // _oppositePlatform.PairWithOppositePlatform(_ouboundPlatform);
        }
        
        public static float3 ToEulerAngles(quaternion q) {
            float3 angles;
 
            // roll (x-axis rotation)
            double sinr_cosp = 2 * (q.value.w * q.value.x + q.value.y * q.value.z);
            double cosr_cosp = 1 - 2 * (q.value.x * q.value.x + q.value.y * q.value.y);
            angles.x = (float)math.atan2(sinr_cosp, cosr_cosp);
 
            // pitch (y-axis rotation)
            double sinp = 2 * (q.value.w * q.value.y - q.value.z * q.value.x);
            if (math.abs(sinp) >= 1)
                angles.y = (float)CopySign(math.PI / 2, sinp); // use 90 degrees if out of range
            else
                angles.y = (float)math.asin(sinp);
 
            // yaw (z-axis rotation)
            double siny_cosp = 2 * (q.value.w * q.value.z + q.value.x * q.value.y);
            double cosy_cosp = 1 - 2 * (q.value.y * q.value.y + q.value.z * q.value.z);
            angles.z = (float)math.atan2(siny_cosp, cosy_cosp);
 
            return angles;
        }
 
        private static double CopySign(double a, double b) {
            return math.abs(a) * math.sign(b);
        }
    }
}