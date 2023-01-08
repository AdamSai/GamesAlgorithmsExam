using Assets.DOTS.Components.Train;
using Assets.DOTS.Components;
using System.Collections;
using Unity.Entities;
using UnityEngine;

namespace Assets.DOTS.Systems
{
    [UpdateAfter(typeof(SetupSeatsSystem))]
    public partial struct SetupCarriageNavPoint : ISystem
    {
        public void OnCreate(ref SystemState state)
        {

        }

        public void OnDestroy(ref SystemState state)
        {

        }

        public void OnUpdate(ref SystemState state)
        {
            var setupCarriageNavPointJob = new SetupCarriageNavPointJob
            { EM = state.EntityManager };
            //state.GetComponentLookup<CarriageSeatComponent>()

            state.Dependency = setupCarriageNavPointJob.Schedule(state.Dependency);

            state.Dependency.Complete();

            state.Enabled = false;
        }

        public partial struct SetupCarriageNavPointJob : IJobEntity
        {
            public EntityManager EM;

            public void Execute(in Entity entity, ref CarriageNavPointsComponent navPoint)
            {
                if (navPoint.init)
                    return;

                Debug.Log("Running SetupCarriageNavPointJob!");

                var buffer = EM.GetBuffer<LinkedEntityGroup>(entity);

                foreach (var item in buffer)
                {
                    Entity e = item.Value;

                    if (EM.HasComponent<NavPointComponent>(e))
                    {
                        navPoint.entrancePointEntity = e;
                    }
                }

                navPoint.init = true;
            }
        }
    }
}