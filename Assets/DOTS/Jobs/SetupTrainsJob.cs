using DOTS.Components;
using DOTS.Components.Train;
using Unity.Entities;

namespace DOTS.Jobs
{
    public partial struct SetupTrainsJob : IJobEntity
    {
        public EntityCommandBuffer ECB;

        public void Execute(MetroLineTrainDataComponent MLTDC, MetroLineComponent MLA)
        {
            float trainSpacing = 1f / MLTDC.maxTrains;

            for (byte i = 0; i < MLTDC.maxTrains; i++)
            {
                //Spawn empty Entity
                //Add TrainIDComponent, TrainData and State

                Entity train = ECB.Instantiate(MLTDC.trainPrefab);
                ECB.SetComponent(train, new TrainIDComponent
                {
                    LineIndex = MLA.MetroLineID,
                    TrainIndex = i
                });

                float pos = trainSpacing * i;
                ECB.SetName(train, $"Train_{MLA.MetroLineID}:{i}");
                ECB.SetComponent(train, new TrainPositionComponent
                {
                    value = pos
                });

                ECB.SetComponent(train, new TrainSpeedComponent
                {
                    speed = MLTDC.maxTrainSpeed,
                    friction = MLTDC.friction
                });

                ECB.SetComponent(train, new MaxTrainSpeedComponent
                {
                    value = MLTDC.maxTrainSpeed
                });

                ECB.SetComponent(train, new AmountOfTrainsInLineComponent
                {
                    value = MLTDC.maxTrains
                });

                ECB.SetComponent(train, new TrainStateComponent
                {
                    value = TrainStateDOTS.DEPARTING
                });

                ECB.AddComponent<TrainAheadComponent>(train);

                ECB.AddComponent<TimerComponent>(train);
            }
        }
    }
}