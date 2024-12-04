using EntitiesEvents;
using gs.chef.game.Grid;
using gs.chef.game.tile.events;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace gs.chef.game.tile
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct TileMatchingAnimationSystem : ISystem
    {
        private EventWriter<AddMatchedTilesEvent> _addMatchedTilesEvent;
        private EntityQuery _tileQuery;
        private EntityQuery _gridQuery;
        
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginPresentationEntityCommandBufferSystem.Singleton>();
            _addMatchedTilesEvent = state.GetEventWriter<AddMatchedTilesEvent>();
            _tileQuery = state.EntityManager.CreateEntityQuery(typeof(TileItemComponent));
            _gridQuery = state.EntityManager.CreateEntityQuery(typeof(GridCellComponent));
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<BeginPresentationEntityCommandBufferSystem.Singleton>();
            var deltaTime = SystemAPI.Time.DeltaTime;
            NativeQueue<int2> destroyedTileAddresses = new NativeQueue<int2>(state.WorldUpdateAllocator);
            NativeQueue<TileType> destroyedTileTypes = new NativeQueue<TileType>(state.WorldUpdateAllocator);

            var job = new TileMatchingAnimationJob()
            {
                DestroyedTileTypes = destroyedTileTypes,
                DestroyedTileAddresses = destroyedTileAddresses,
                DeltaTime = deltaTime,
                CommandBuffer = ecb.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
            };

            state.Dependency = job.ScheduleParallel(state.Dependency);

            state.Dependency.Complete();
            
            NativeList<int> matchedColumns = new NativeList<int>(state.WorldUpdateAllocator);
            

            

            if (destroyedTileAddresses.Count > 0)
            {
                var destroyedTileAddressesArray = destroyedTileAddresses.ToArray(Allocator.Temp);
                var tileType = destroyedTileTypes.Dequeue();
                var eventData = new AddMatchedTilesEvent
                {
                    TileType = tileType,
                    Count = destroyedTileAddresses.Count
                };

                
                
                var gridEntities = _gridQuery.ToEntityArray(state.WorldUpdateAllocator);
                var gridCellComponents = _gridQuery.ToComponentDataArray<GridCellComponent>(state.WorldUpdateAllocator);
                foreach (var address in destroyedTileAddressesArray)
                {
                    for (int j = 0; j < gridEntities.Length; j++)
                    {
                        if (gridCellComponents[j].Address.Equals(address))
                        {
                            var gridEntity = gridEntities[j];
                            var gridCellComponent = gridCellComponents[j];
                            gridCellComponent.IsEmpty = true;
                            state.EntityManager.SetComponentData(gridEntity, gridCellComponent);
                        }
                    }
                }
                
                _addMatchedTilesEvent.Write(eventData);
                
                
                
                /*while (destroyedTileAddresses.TryDequeue(out var address))
                {
                    var tileEntities = _tileQuery.ToEntityArray(state.WorldUpdateAllocator);
                    for (int i = 0; i < tileEntities.Length; i++)
                    {
                        var entity = tileEntities[i];
                        var tileItemComponent = state.EntityManager.GetComponentData<TileItemComponent>(entity);
                        if (tileItemComponent.Address.Equals(address))
                        {
                            state.EntityManager.DestroyEntity(entity);
                            
                            var movingTileEntities = _tileQuery.ToEntityArray(state.WorldUpdateAllocator);
                            for (int j = 0; j < movingTileEntities.Length; j++)
                            {
                                if(movingTileEntities[j] == entity) continue;
                                var movingItemComponent = state.EntityManager.GetComponentData<TileItemComponent>(movingTileEntities[j]);
                                var diffY = math.abs(tileItemComponent.Address.y - movingItemComponent.Address.y);
                                if (movingItemComponent.Address.x == tileItemComponent.Address.x && movingItemComponent.Address.y > tileItemComponent.Address.y && diffY == 1)
                                {
                                    var copy = movingItemComponent;
                                    movingItemComponent.Address.y = tileItemComponent.Address.y;
                                    state.EntityManager.SetComponentData(movingTileEntities[j], movingItemComponent);
                                    var targetPosition = tileItemComponent.Position;
                                    var movingComp = state.EntityManager.GetComponentData<TileMovingComponent>(movingTileEntities[j]);
                                    movingComp.TargetPosition = targetPosition;
                                    state.EntityManager.SetComponentData(movingTileEntities[j], movingComp);
                                    tileItemComponent = copy;
                                }
                            }
                        }

                        
                    }
                }*/
            }
            
            // Find matched columns
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }

    [BurstCompile]
    public partial struct TileMatchingAnimationJob : IJobEntity
    {
        public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter CommandBuffer;
        [NativeDisableParallelForRestriction] public NativeQueue<int2> DestroyedTileAddresses;
        [NativeDisableParallelForRestriction] public NativeQueue<TileType> DestroyedTileTypes;


        [BurstCompile]
        private void Execute(Entity entity, ref LocalTransform localTransform, ref TileItemComponent tileItemComponent,
            ref TileMatchAnimationTag tileMatchAnimationTag, [ChunkIndexInQuery] int sortKey)
        {
            //if (tileItemComponent.IsMatched) return;
            //var currentScale = localTransform.Scale;
            var targetScale = tileMatchAnimationTag.TargetScale;
            CommandBuffer.SetComponentEnabled<ClickableComponent>(sortKey, entity, false);

            if (tileMatchAnimationTag.Timer >= tileMatchAnimationTag.Duration)
            {
                tileMatchAnimationTag.CurrentScale = targetScale;
                tileMatchAnimationTag.Timer = 0;
                tileItemComponent.IsMatched = true;
                CommandBuffer.DestroyEntity(sortKey, entity);
                DestroyedTileAddresses.Enqueue(tileItemComponent.Address);
                DestroyedTileTypes.Enqueue(tileItemComponent.TileType);
                //CommandBuffer.SetEnabled(sortKey, entity, false);
                //CommandBuffer.AddComponent<DisableRendering>(sortKey, entity);
                //CommandBuffer.SetComponentEnabled<TileMatchAnimationTag>(sortKey, entity, false);
            }
            else
            {
                tileMatchAnimationTag.Timer += DeltaTime;
                var newScale = math.lerp(tileMatchAnimationTag.CurrentScale, targetScale, 0.02f);
                //currentScale = newScale;
                tileMatchAnimationTag.CurrentScale = newScale;
                localTransform.Scale = newScale;
            }
        }
    }
}