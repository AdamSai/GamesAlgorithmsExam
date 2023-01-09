using Assets.DOTS.Components;
using Assets.DOTS.Components.Train;
using DOTS.Components;
using DOTS.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

namespace Assets.DOTS.Systems
{
    [UpdateAfter(typeof(SetupTrainsSystem))]
    [BurstCompile]
    public partial struct SetupSeatsSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var setupSeatsJob = new SetupSeatsJob 
            { EM = state.EntityManager };
            //state.GetComponentLookup<CarriageSeatComponent>()

            state.Dependency = setupSeatsJob.Schedule(state.Dependency);

            state.Dependency.Complete();

            state.Enabled = false;
        }
    }

    [BurstCompile]
    public partial struct SetupSeatsJob : IJobEntity
    {
        public EntityManager EM;

        public void Execute(in Entity entity, ref CarriagePassengerSeatsComponent seats)
        {
            if (seats.init)
                return;


            var buffer = EM.GetBuffer<LinkedEntityGroup>(entity);

            foreach (var item in buffer)
            {
                Entity e = item.Value;
                
                if (EM.HasComponent<CarriageSeatComponent>(e))
                {
                    seats.seats.Add(e);
                }
            }

            seats.init = true;
        }
    }
}