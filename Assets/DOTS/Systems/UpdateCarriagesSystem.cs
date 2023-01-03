using DOTS.Components;
using Unity.Collections;
using Unity.Entities;

public partial struct UpdateCarriagesSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {
        //Loop all trains
        //Update all carriages of each train

        //Add all Carriages To array
        var trainArray = new EntityQueryBuilder(Allocator.Temp)
        .WithAny<TrainIDComponent>().Build(ref state).ToEntityArray(Allocator.Temp);

        for (int i = 0; i < trainArray.Length; i++)
        {
            //Add all carriages To array, only loop 
            var carriageArray = new EntityQueryBuilder(Allocator.Temp)
            .WithAny<CarriageIDComponent>().Build(ref state).ToEntityArray(Allocator.Temp);

            for (int j = 0; j < carriageArray.Length; j++)
            {

            }
        }
    }
}
