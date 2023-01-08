using Assets.DOTS.Components;
using Assets.DOTS.Components.Train;
using DOTS.Components;
using Unity.Collections;
using Unity.Entities;

namespace DOTS.Jobs
{
    [WithAll(typeof(CarriageIDComponent))]
    public partial struct SetupTrainSeatsJob : IJobEntity
    {
        public EntityManager EM;
        public void Execute(in Entity ent)
        {
            var children = EM.GetBuffer<LinkedEntityGroup>(ent);
            var seats = new NativeList<Entity>(Allocator.Persistent);
            for (var i = 0; i < children.Length; i++)
            {
                if (EM.HasComponent<CarriageSeatComponent>(children[i].Value))
                {
                    seats.Add(children[i].Value);
                }
            }

            EM.SetComponentData(ent, new CarriagePassengerSeatsComponent
            {
                init = true,
                seats = seats
            });
        }
    }
}