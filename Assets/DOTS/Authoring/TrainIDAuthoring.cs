using Unity.Entities;
using UnityEngine;

//namespace DOTS.Authoring
public class TrainIDAuthoring : MonoBehaviour
{

}

public class TrainIDBaker : Baker<TrainIDAuthoring>
{
    public override void Bake(TrainIDAuthoring authoring)
    {
        AddComponent(new TrainIDComponent
        {

        });
    }
}
