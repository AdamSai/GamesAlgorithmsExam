using Unity.Entities;

public struct TrainIDComponent : IComponentData
{
    public int TrainIndex;
    public int LineIndex;
}
