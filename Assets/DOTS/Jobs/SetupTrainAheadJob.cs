using DOTS.Components;
using DOTS.Components.Train;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace DOTS.Jobs
{
    [BurstCompile]
    public partial struct SetupTrainAheadJob : IJobEntity
    {
        public ComponentLookup<TrainIDComponent> trainIdLookup;
        public NativeArray<Entity> trains;

        public void Execute(in Entity entity, ref TrainAheadComponent trainAheadComponent,
            in AmountOfTrainsInLineComponent maxTrains)
        {
            foreach (var train in trains)
            {
                var trainID = trainIdLookup.GetRefRO(entity).ValueRO;
                var other = trainIdLookup.GetRefRO(train).ValueRO;
                if (other.LineIndex == trainID.LineIndex)
                {
                    if (entity.Index == train.Index)
                        continue;

                    if (trainID.TrainIndex == maxTrains.value - 1 && other.TrainIndex == 0)
                    {
                        trainAheadComponent.Value = train;
                        return;
                    }

                    if (other.TrainIndex == trainID.TrainIndex + 1)
                    {
                        trainAheadComponent.Value = train;
                        return;
                    }
                }
            }
        }
    }
}