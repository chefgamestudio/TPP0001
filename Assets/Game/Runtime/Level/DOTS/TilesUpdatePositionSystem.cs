using gs.chef.game.tile;
using Synthesis.SystemGroups;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace gs.chef.game.level
{
    [UpdateInGroup(typeof(SynthesisFirstSimulationGroup))]
    [UpdateAfter(typeof(CreateLevelTilesSystem))]
    public partial struct TilesUpdatePositionSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;

            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();

            var job = new UpdateTilePositionJob()
            {
                CommandBuffer = ecb.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
                DeltaTime = deltaTime
            };

            job.ScheduleParallel();


            /*foreach (var (itemComponent, localTransform, movingComponent, tileEntity) in SystemAPI
                         .Query<RefRW<TileItemComponent>, RefRW<LocalTransform>, RefRW<TileMovingComponent>>()
                         .WithEntityAccess())
            {
                var isActiveMoving = SystemAPI.IsComponentEnabled<TileMovingComponent>(tileEntity);
                if (isActiveMoving)
                {
                    var position = itemComponent.ValueRW.Position;
                    var targetPosition = movingComponent.ValueRW.TargetPosition;
                    var speed = movingComponent.ValueRW.Speed;
                    var distance = math.distance(position, targetPosition);
                    var direction = math.normalize(targetPosition - position);
                    var move = direction * speed * deltaTime;

                    var newPos = position + move;

                    position = newPos;
                    itemComponent.ValueRW.Position = position;

                    if (distance < speed * deltaTime)
                    {
                        localTransform.ValueRW.Position = targetPosition;
                        itemComponent.ValueRW.Position = targetPosition;
                        SystemAPI.SetComponentEnabled<TileMovingComponent>(tileEntity, false);
                        SystemAPI.SetComponentEnabled<ClickableComponent>(tileEntity, true);
                    }
                    else
                    {
                        localTransform.ValueRW.Position = newPos;
                        itemComponent.ValueRW.Position = newPos;
                        SystemAPI.SetComponentEnabled<ClickableComponent>(tileEntity, false);
                    }
                    //itemComponent.ValueRW.Position = localTransform.ValueRW.Position;

                    //localTransform.ValueRW.Position = position;
                }
                else
                {
                    localTransform.ValueRW.Position = itemComponent.ValueRW.Position;
                }
            }*/
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }

    [BurstCompile]
    public partial struct UpdateTilePositionJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter CommandBuffer;
        public float DeltaTime;

        private void Execute(Entity entity, ref TileItemComponent itemComponent, ref LocalTransform localTransform,
            ref TileMovingComponent movingComponent,
            [ChunkIndexInQuery] int sortKey)
        {
            var position = itemComponent.Position;
            var targetPosition = movingComponent.TargetPosition;
            var speed = movingComponent.Speed;
            var distance = math.distance(position, targetPosition);
            var direction = math.normalize(targetPosition - position);
            var move = direction * speed * DeltaTime;

            var newPos = position + move;

            position = newPos;
            itemComponent.Position = position;

            if (distance < speed * DeltaTime)
            {
                localTransform.Position = targetPosition;
                itemComponent.Position = targetPosition;
                CommandBuffer.SetComponentEnabled<TileMovingComponent>(sortKey, entity, false);
                if (itemComponent.Address.y <= 10)
                    CommandBuffer.SetComponentEnabled<ClickableComponent>(sortKey, entity, true);
                else
                {
                    CommandBuffer.SetComponentEnabled<ClickableComponent>(sortKey, entity, false);
                }
            }
            else
            {
                localTransform.Position = newPos;
                itemComponent.Position = newPos;
                CommandBuffer.SetComponentEnabled<ClickableComponent>(sortKey, entity, false);
            }
        }
    }
}