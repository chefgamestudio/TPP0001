using EntitiesEvents;
using gs.chef.game.input;
using gs.chef.game.level;
using gs.chef.game.tile.events;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Logging;
using Unity.Mathematics;

namespace gs.chef.game.tile
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(GameInputSystem))]
    public partial struct ClickTileSystem : ISystem
    {
        private EventReader<ClickedTileEvent> _clickedTileEventReader;

        private EntityQuery _tileQuery;

        //[BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<LevelConfigSystemAuthoring.SystemIsEnabledTag>();
            _tileQuery = state.EntityManager.CreateEntityQuery(typeof(TileItemComponent));
            _clickedTileEventReader = state.GetEventReader<ClickedTileEvent>();
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            Entity clickedTileEntity = Entity.Null;

            foreach (var eventData in _clickedTileEventReader.Read())
            {
                clickedTileEntity = eventData.ClickedTile;
            }

            if (clickedTileEntity == Entity.Null)
            {
                // Do something with the clicked tile
                return;
            }

            var levelConfigEntity = SystemAPI.GetSingletonEntity<LevelConfigSystemAuthoring.SystemIsEnabledTag>();
            var levelConfigData = SystemAPI.GetComponent<LevelConfigComponent>(levelConfigEntity);

            var tileItemComponent = SystemAPI.GetComponent<TileItemComponent>(clickedTileEntity);
            Log.Warning($"Clicked tile: {(int)tileItemComponent.TileType}, {tileItemComponent.Address}");

            NativeArray<Entity> tileEntities = _tileQuery.ToEntityArray(state.WorldUpdateAllocator);
            NativeList<Entity> foundTiles = new NativeList<Entity>(state.WorldUpdateAllocator);
            NativeArray<TileItemComponent> tileItemComponents =
                _tileQuery.ToComponentDataArray<TileItemComponent>(state.WorldUpdateAllocator);
            NativeQueue<int2> addressQueue = new NativeQueue<int2>(state.WorldUpdateAllocator);
            NativeHashSet<int2> visitedAddresses = new NativeHashSet<int2>(4, state.WorldUpdateAllocator);


            var job = new FindNeighborTilesJob
            {
                TileEntities = tileEntities,
                FoundTiles = foundTiles,
                StartAddress = tileItemComponent.Address,
                TargetTileType = tileItemComponent.TileType,
                StartingTileEntity = clickedTileEntity,
                TileItemComponents = tileItemComponents,
                AddressQueue = addressQueue,
                VisitedAddresses = visitedAddresses,
                Rows = levelConfigData.VistaRows,
                Columns = levelConfigData.Columns,
            };

            state.Dependency = job.Schedule(state.Dependency);

            state.Dependency.Complete();

            if (foundTiles.Length > 2)
            {
                foreach (Entity tileEntity in foundTiles)
                {
                    // Do something with the tile entity, e.g., change its color
                    /*var localTransform = SystemAPI.GetComponent<TileMatchAnimationTag>(tileEntity);
                    localTransform.Scale = 0.3f;
                    SystemAPI.SetComponent<LocalTransform>(tileEntity, localTransform);*/
                    SystemAPI.SetComponentEnabled<ClickableComponent>(tileEntity, false);
                    SystemAPI.SetComponentEnabled<TileMatchAnimationTag>(tileEntity, true);
                }
            }


            foundTiles.Dispose();
            tileEntities.Dispose();
            tileItemComponents.Dispose();
            addressQueue.Dispose();
            visitedAddresses.Dispose();
        }
    }

    [BurstCompile]
    public struct FindNeighborTilesJob : IJob
    {
        [ReadOnly] public NativeArray<TileItemComponent> TileItemComponents;
        [ReadOnly] public NativeArray<Entity> TileEntities;
        [ReadOnly] public int2 StartAddress;
        [ReadOnly] public TileType TargetTileType;
        [ReadOnly] public Entity StartingTileEntity;
        [ReadOnly] public int Rows;
        [ReadOnly] public int Columns;

        public NativeList<Entity> FoundTiles;
        public NativeQueue<int2> AddressQueue;
        public NativeHashSet<int2> VisitedAddresses;

        [BurstCompile]
        public void Execute()
        {
            AddressQueue.Enqueue(StartAddress);
            VisitedAddresses.Add(StartAddress);
            FoundTiles.Add(StartingTileEntity);

            NativeList<int2> neighborOffsets = new NativeList<int2>(4, Allocator.Temp);
            neighborOffsets.Add(new int2(1, 0));
            neighborOffsets.Add(new int2(-1, 0));
            neighborOffsets.Add(new int2(0, 1));
            neighborOffsets.Add(new int2(0, -1));

            while (AddressQueue.Count > 0)
            {
                int2 currentAddress = AddressQueue.Dequeue();

                foreach (var offset in neighborOffsets)
                {
                    int2 neighbor = currentAddress + offset;

                    if (neighbor.x < 0 || neighbor.x >= Columns || neighbor.y < 0 || neighbor.y >= Rows)
                        continue;

                    if (!VisitedAddresses.Contains(neighbor))
                    {
                        for (int i = 0; i < TileItemComponents.Length; i++)
                        {
                            if (TileItemComponents[i].Address.Equals(neighbor) &&
                                TileItemComponents[i].TileType == TargetTileType)
                            {
                                var neighborEntity = TileEntities[i];
                                FoundTiles.Add(neighborEntity);
                                AddressQueue.Enqueue(neighbor);
                                VisitedAddresses.Add(neighbor);
                                break;
                            }
                        }
                    }
                }
            }

            neighborOffsets.Dispose();
        }
    }
}