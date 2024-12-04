using Unity.Burst;
using Unity.Entities;

namespace gs.chef.game.tile
{
    public partial struct TileRendering : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            
            new TileRenderingJob()
            {
                CommandBuffer = ecb.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
            }.ScheduleParallel();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
    
    [BurstCompile]
    
    public partial struct TileRenderingJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter CommandBuffer;


        [BurstCompile]
        private void Execute(Entity entity, in TileItemComponent addressComponent, [ChunkIndexInQuery] int sortKey)
        {
            
        }

    }
}