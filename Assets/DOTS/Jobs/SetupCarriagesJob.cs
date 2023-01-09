using Assets.DOTS.Components.Train;
using DOTS.Components;
using DOTS.Components.Train;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.Jobs
{
    [BurstCompile]
    public partial struct SetupCarriagesJob : IJobEntity
    {
        public EntityCommandBuffer ECB;
        public EntityManager EM;

        public void Execute(MetroLineCarriageDataComponent MLCarriage, MetroLineTrainDataComponent MLTrain,
                in MetroLineComponent MLID, ColorComponent color)
        {
            for (int i = 0; i < MLTrain.maxTrains; i++)
            {
                Entity previousCarriage = Entity.Null;
                for (int j = 0; j < MLCarriage.carriages; j++)
                {
                    //Instantiate Carriages
                    Entity carriage = ECB.Instantiate(MLCarriage.carriage);


                    ECB.SetComponent(carriage, new CarriageIDComponent
                    {
                        id = j,
                        trainIndex = i,
                        lineIndex = MLID.MetroLineID
                    });

                    ECB.SetComponent(carriage, new ColorComponent
                    {
                        value = color.value
                    });

                    ECB.AddComponent(carriage, new CarriageAheadOfMeComponent
                    {
                        Value = previousCarriage
                    });

                    ECB.AddComponent(carriage, new CarriagePassengerSeatsComponent
                    {
                        init = false
                    });


                    previousCarriage = carriage;
                }
            }
        }
    }


}
