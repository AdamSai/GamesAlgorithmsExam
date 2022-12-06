
using Unity.Entities;
using UnityEngine;

public class TrainDataAuthoring : MonoBehaviour
{

}
public class TrainDataBaker : Baker<TrainDataAuthoring>
{
    public override void Bake(TrainDataAuthoring authoring)
    {
        AddComponent(new TrainDataComponent
        {

        });
    }
}