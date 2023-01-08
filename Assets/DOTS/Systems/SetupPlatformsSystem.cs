using Unity.Collections;
using Unity.Entities;

namespace Assets.DOTS.Systems
{
    [UpdateAfter(typeof(SetupRailSystem))]
    public partial struct SetupPlatformsSystem : ISystem
    {
        private EntityQuery platformQuery2;
        private ComponentLookup<PlatformComponent> platformComponentLookup;

        public void OnCreate(ref SystemState state)
        {
            platformQuery2 =
                new EntityQueryBuilder(Allocator.Temp).WithAll<PlatformComponent>().Build(ref state);
            platformComponentLookup = state.GetComponentLookup<PlatformComponent>();

        }

        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            var platformEnts = platformQuery2.ToEntityArray(Allocator.Persistent);
            var connecterECB = new EntityCommandBuffer(Allocator.Persistent);
            platformComponentLookup.Update(ref state);
            var connectPlatformsJob = new ConnectPlatformsJob
            {
                ECB = connecterECB,
                platformLookUp = platformComponentLookup,
                platformEntities = platformEnts
            };

            
            connectPlatformsJob.Run();
            connecterECB.Playback(state.EntityManager);
            connecterECB.Dispose();
            state.Enabled = false;
        }
    }
}