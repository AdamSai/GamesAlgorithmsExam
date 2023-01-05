using DOTS.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

[UpdateAfter(typeof(SetupTrainsSystem))]
public partial struct SetupCarriageColorSystem : ISystem
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


        var carriagesColorJob = new SetupCarriagesColorJob
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

        //ecb.Playback(state.EntityManager);



        state.CompleteDependency();

        //state.Enabled = false;
    }

    [WithAny(typeof(CarriageTag), (typeof(PlatformTag)))]
    public partial struct SetupCarriagesColorJob : IJobEntity
    {
        public EntityManager EM;
        public EntityCommandBuffer ECB;
        public void Execute(in Entity ent, ref EnableComponent enable, in ColorComponent color)
        {
            if (enable.value)
                return;

            var children = EM.GetBuffer<LinkedEntityGroup>(ent);
            //Child

            for (int i = 0; i < children.Length; i++)
            {
                if (EM.HasComponent<ChangeColorTag>(children[i].Value))
                {
                    ECB.AddComponent(children[i].Value, new URPMaterialPropertyBaseColor { Value = new float4(color.value) });
                }
            }

            enable.value = true;
        }
    }

    //[WithAll(typeof(TrainTag))]

    public partial struct CarriageParentJob : IJobEntity
    {
        public EntityCommandBuffer ECB;
        public ComponentLookup<CarriageIDComponent> carriageLookUp;
        public NativeList<Entity> carriages;
        //public ComponentLookup<CarriageIDComponent> carriages;
        public void Execute(in Entity ent, TrainIDComponent trainID, EnableComponent enable)
        {
            if (enable.value)
                return;

            //var platformToAdd = carriages.GetRefRO(_platform).ValueRO;
            for (int i = 0; i < carriages.Length; i++)
            {
                var _CA_ENT = carriages[i];
                var _PA = carriageLookUp.GetRefRO(_CA_ENT).ValueRO;

                if (trainID.LineIndex != _PA.lineIndex) return;
                if (trainID.TrainIndex != _PA.trainIndex) return;
                ECB.AddComponent(_CA_ENT, new Parent { Value = ent });
            }

            enable.value = true;
        }
    }

}