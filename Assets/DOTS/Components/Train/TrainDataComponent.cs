using Unity.Entities;

public struct TrainDataComponent : IComponentData
{
    public int TotalCarriages;
    public float Position;
    public float Speed;
}
