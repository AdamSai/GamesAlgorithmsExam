using Unity.Entities;
using UnityEngine;
public class TrainStateAuthoring : MonoBehaviour
{

}

public class TrainStateBaker : Baker<TrainStateAuthoring>
{
    public override void Bake(TrainStateAuthoring authoring)
    {
        AddComponent(new TrainStateComponent
        {
            value = TrainStateDOTS.EN_ROUTE
        });
    }
}
