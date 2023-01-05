using DOTS.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DOTS.Jobs
{
    public partial struct AddMissingPlatformsJob : IJobEntity
    {
        public ComponentLookup<PlatformComponent> platformLookUp;
        public NativeList<Entity> PlatformsEntities;
        private int counter;
        public void Execute(in ConfigTag config) // Pass in Platform component
        {
            counter = 0;
            Debug.Log("Add missing platforms");
            Debug.Log(PlatformsEntities.Length);
            
            for (int i = 0; i < PlatformsEntities.Length; i++)
            {
                var _PA_ENT = PlatformsEntities[i];
                var _PA = platformLookUp.GetRefRO(_PA_ENT).ValueRO;
                var _PA_START = _PA.point_platform_START.location;
                var _PA_END = _PA.point_platform_END.location;

                foreach (var _PB_ENT in PlatformsEntities)
                {
                    var _PB = platformLookUp.GetRefRO(_PB_ENT).ValueRO;
                    if (!_PB.Equals(_PA))
                    {
                        Debug.Log("not equals");
                        var _PB_START = _PB.point_platform_START.location;
                        var _PB_END = _PB.point_platform_END.location;
                        bool aSTART_to_bSTART = Positions_Are_Adjacent(_PA_START, _PB_START);
                        bool aSTART_to_bEND = Positions_Are_Adjacent(_PA_START, _PB_END);
                        bool aEND_to_bEND = Positions_Are_Adjacent(_PA_END, _PB_END);
                        bool aEND_to_bSTART = Positions_Are_Adjacent(_PA_END, _PB_START);
                        
                        if ((aSTART_to_bSTART && aEND_to_bEND) || (aEND_to_bSTART && aSTART_to_bEND))
                        {
                            Debug.Log("inside if");

                            Add_AdjacentPlatform(_PA_ENT, _PB_ENT);
                            Add_AdjacentPlatform(_PA.oppositePlatform, _PB_ENT);
                        }
                    }
                }
            }
            Debug.Log("Counter: " + counter);
        }
        
        public void Add_AdjacentPlatform(Entity invokerEntity, Entity entityToAdd)
        {
            Debug.Log("Add adjacent");
            // Add the platform
            AddAdjacentIfNotPresent(invokerEntity, entityToAdd);
            // Add the opposite platform
            var platformToAdd = platformLookUp.GetRefRO(entityToAdd).ValueRO;
            var oppositePlatformEntity = platformToAdd.oppositePlatform;
            AddAdjacentIfNotPresent(invokerEntity, oppositePlatformEntity);
            
            // Loop and find adjacent platforms
            foreach (var _ADJ in platformToAdd.neighborPlatforms)
            {
                // Add the adjacent platform
                AddAdjacentIfNotPresent(invokerEntity, _ADJ);
                // Add the original to the adjacent platform
                AddAdjacentIfNotPresent(_ADJ, invokerEntity);
                // Add the originals opposite platform to the adjacent platform
                var invokerPlatform = platformLookUp.GetRefRO(invokerEntity).ValueRO;
                AddAdjacentIfNotPresent(_ADJ, invokerPlatform.oppositePlatform);
            }
            
            var oppositePlatform = platformLookUp.GetRefRO(oppositePlatformEntity).ValueRO;
            // Do the same but for the opposites neighbouring platforms
            foreach (var _ADJ in oppositePlatform.neighborPlatforms)
            {
                // Add the adjacent platform
                AddAdjacentIfNotPresent(invokerEntity, _ADJ);
                // Add the original to the adjacent platform
                AddAdjacentIfNotPresent(_ADJ, invokerEntity);
                // Add the originals opposite platform to the adjacent platform
                var invokerPlatform = platformLookUp.GetRefRO(invokerEntity).ValueRO;
                AddAdjacentIfNotPresent(_ADJ, invokerPlatform.oppositePlatform);
            }
        }

        public void AddAdjacentIfNotPresent(Entity original, Entity entityToAdd)
        {
            var originalPlatform = platformLookUp.GetRefRW(original, false).ValueRW;
            counter++;

            if (!originalPlatform.neighborPlatforms.Contains(entityToAdd) && !entityToAdd.Equals(original))
            {
                Debug.Log("Adding adjacent platform");
                originalPlatform.neighborPlatforms.Add(entityToAdd);
            }
        }
        
        private bool Positions_Are_Adjacent(float3 _A, float3 _B)
        {
            return math.distance(_A, _B) <= 12f; // TODO; 12f see Metro.PLATFORM_ADJACENCY_LIMIT
        }
    }
}