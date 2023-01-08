using DOTS.Components;
using DOTS.Jobs;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

[UpdateAfter(typeof(SetupTrainsSystem))]
public partial struct SetupCarriagesSystem : ISystem
{
    EntityCommandBuffer ecb;
    private ComponentLookup<CarriageIDComponent> carriageComponentLookup;
    private EntityQuery carriageQuery;
    [DeallocateOnJobCompletionAttribute]
    private NativeList<Entity> _carriages;

    public void OnCreate(ref SystemState state)
    {
        carriageQuery =
    new EntityQueryBuilder(Allocator.Temp).WithAll<CarriageIDComponent>().Build(ref state);
        carriageComponentLookup = state.GetComponentLookup<CarriageIDComponent>();
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {
        //Entity Command Buffer
        ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        var carriagesColorJob = new ChangeEntitiesColorsJob
        {
            EM = state.EntityManager,
            ECB = ecb
        };
        state.Dependency = carriagesColorJob.Schedule(state.Dependency);
        state.Dependency.Complete();


        #region ParentCarriages
        //Cariages query to list of components
        _carriages =
        carriageQuery.ToEntityListAsync(Allocator.Persistent, out var carriagesJobHandle);

        var dependency0 = JobHandle.CombineDependencies(carriagesJobHandle, state.Dependency);
        dependency0.Complete();
        carriageComponentLookup.Update(ref state);
        var carriageParentJob = new CarriageParentJob
        {
            ECB = ecb,
            carriageLookUp = carriageComponentLookup,
            carriages = _carriages
        }.Schedule(dependency0);

        carriageParentJob.Complete();
        #endregion

        state.CompleteDependency();
    }
}