using DOTS.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DOTS.Jobs
{
    [WithAll(typeof(PlatformComponent))]
    public partial struct ConnectPlatformNeighbours : IJobEntity
    {
        public ComponentLookup<PlatformComponent> platformLookUp;
        public NativeList<Entity> PlatformsEntities;
        private int counter;
        public void Execute(in Entity entity) // Pass in Platform component
        {
            counter = 0;
            Debug.Log("Add missing platforms");
            Debug.Log(PlatformsEntities.Length);
            
                var _PA = platformLookUp.GetRefRO(entity).ValueRO;
                var _PA_START = _PA.point_platform_START.location;
                var _PA_END = _PA.point_platform_END.location;
                foreach (var _PB_ENT in PlatformsEntities)
                {
                    var _PB = platformLookUp.GetRefRO(_PB_ENT).ValueRO;
                    if (_PB_ENT.Index != entity.Index)
                    {
                        var _PB_START = _PB.point_platform_START.location;
                        var _PB_END = _PB.point_platform_END.location;
                        bool aSTART_to_bSTART = Positions_Are_Adjacent(_PA_START, _PB_START);
                        bool aSTART_to_bEND = Positions_Are_Adjacent(_PA_START, _PB_END);
                        bool aEND_to_bEND = Positions_Are_Adjacent(_PA_END, _PB_END);
                        bool aEND_to_bSTART = Positions_Are_Adjacent(_PA_END, _PB_START);
                        
                        if ((aSTART_to_bSTART && aEND_to_bEND) || (aEND_to_bSTART && aSTART_to_bEND))
                        {
                            Add_AdjacentPlatform(entity, _PB_ENT);
                        }
                    }
                }
            Debug.Log("Counter: " + counter);
        }

        public void Add_AdjacentPlatform(Entity invokerEntity, Entity _platform)
        {
            Debug.Log("Add adjacent");
            // Add the platform
            AddAdjacentIfNotPresent(invokerEntity, _platform);
        }

        public void AddAdjacentIfNotPresent(Entity original, Entity _platform)
        {

            var originalPlatform = platformLookUp.GetRefRW(original, false).ValueRW;
            if (!originalPlatform.neighborPlatforms.Contains(_platform) && _platform.Index != original.Index)
            {
                counter++;
                Debug.Log("Adding adjacent platform");
                originalPlatform.neighborPlatforms.Add(_platform);
            }
        }
        
        private bool Positions_Are_Adjacent(float3 _A, float3 _B)
        {
            return math.distance(_A, _B) <= 27f; // TODO; 12f see Metro.PLATFORM_ADJACENCY_LIMIT
        }
    }
}