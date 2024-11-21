using EntitiesEvents;
using gs.chef.game.tile.events;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Logging;
using Unity.Mathematics;
using Unity.Transforms;

namespace gs.chef.game.tile
{
    public partial struct ClickTileSystem : ISystem
    {
        private EventReader<ClickedTileEvent> _clickedTileEventReader;
        
        private EntityQuery _tileQuery;

        //[BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _tileQuery = state.EntityManager.CreateEntityQuery(typeof(TileItemComponent));
            _clickedTileEventReader = state.GetEventReader<ClickedTileEvent>();
        }
        
        
        /*public partial struct FindNeighborsJob : IJobEntity
        {
            
            
            private void Execute()
            {
                
            }
        }*/

        //[BurstCompile]
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

            var tileItemComponent = SystemAPI.GetComponent<TileItemComponent>(clickedTileEntity);
            Log.Warning($"Clicked tile: {(int)tileItemComponent.TileType}, {tileItemComponent.Address}");
            
            
            var tileEntities = _tileQuery.ToEntityArray(state.WorldUpdateAllocator);
            
            
            
            
            
            
            
            
            
            
            
            
            

            int2 startingAddress = tileItemComponent.Address;
            NativeList<Entity> sameColorTiles = FindSameColorTiles(state.EntityManager, startingAddress,
                clickedTileEntity, tileItemComponent.TileType, 10, 8);

// Process the found tiles
            foreach (Entity tileEntity in sameColorTiles)
            {
                // Do something with the tile entity, e.g., change its color
                var localTransform = SystemAPI.GetComponent<LocalTransform>(tileEntity);
                localTransform.Scale = 0.3f;
                SystemAPI.SetComponent<LocalTransform>(tileEntity, localTransform);
                // ...
            }

            sameColorTiles.Dispose(); // VERY IMPORTANT: Dispose the NativeList when you're done with it.
        }
        
         


        private NativeList<Entity> FindSameColorTiles(EntityManager entityManager, int2 startAddress,
            Entity startingTileEntity, TileType tileType, int rows, int columns)
        {
            // Find the starting tile entity.
            //var tileQuery = entityManager.CreateEntityQuery(typeof(TileItemComponent));


            TileType targetTileType = tileType;
            NativeList<Entity> foundTiles = new NativeList<Entity>(Allocator.Temp);
            NativeQueue<int2> addressQueue = new NativeQueue<int2>(Allocator.Temp);
            NativeHashSet<int2> visitedAddresses = new NativeHashSet<int2>(4, Allocator.Temp);

            NativeArray<TileItemComponent> tileItemComponents =
                _tileQuery.ToComponentDataArray<TileItemComponent>(Allocator.Temp);

            addressQueue.Enqueue(startAddress);
            visitedAddresses.Add(startAddress);
            foundTiles.Add(startingTileEntity);

            int2[] neighborOffsets = { new int2(1, 0), new int2(-1, 0), new int2(0, 1), new int2(0, -1) };

            while (addressQueue.Count > 0)
            {
                int2 currentAddress = addressQueue.Dequeue();
                
                Log.Warning($"Current address: {currentAddress}");

                foreach (var offset in neighborOffsets)
                {
                    int2 neighbor = currentAddress + offset;
                    Log.Warning($"Neighbor found: {currentAddress} + {offset} -> {neighbor}");

                    if (neighbor.x < 0 || neighbor.x >= columns || neighbor.y < 0 || neighbor.y >= rows)
                        continue;

                    if (!visitedAddresses.Contains(neighbor))
                    {
                        //Entity neighborEntity = Entity.Null;

                        for (int i = 0; i < tileItemComponents.Length; i++)
                        {
                            if (tileItemComponents[i].Address.Equals(neighbor) && tileItemComponents[i].TileType == targetTileType)
                            {
                                
                                var neighborEntity = _tileQuery.ToEntityArray(Allocator.Temp)[i];
                                foundTiles.Add(neighborEntity);
                                addressQueue.Enqueue(neighbor);
                                visitedAddresses.Add(neighbor);
                                break;
                            }
                        }
                    }
                }
            }

            addressQueue.Dispose();
            visitedAddresses.Dispose();
            tileItemComponents.Dispose();
            return foundTiles;
        }


        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}

/*
using Unity.Entities;
   using Unity.Mathematics;
   using Unity.Collections;

   public static class TileFinder
   {
       public static NativeList<Entity> FindSameColorTiles(EntityManager entityManager, int2 startAddress)
       {
           // Find the starting tile entity.
           Entity startingTileEntity = -1; // Invalid entity handle initially.
           var tileQuery = entityManager.CreateEntityQuery(typeof(TileItemComponent));
           var tiles = tileQuery.ToComponentDataArray<TileItemComponent>(Allocator.Temp);

           for (int i = 0; i < tiles.Length; i++)
           {
               if (tiles[i].Address.Equals(startAddress))
               {
                   startingTileEntity = tileQuery.ToEntityArray(Allocator.Temp)[i];
                   break; // Found the starting tile
               }
           }

           tiles.Dispose();

           if (startingTileEntity == -1)
           {
               return new NativeList<Entity>(Allocator.Temp); // Return empty list if start address not found.
           }


           TileType targetTileType = entityManager.GetComponentData<TileItemComponent>(startingTileEntity).TileType;
           NativeList<Entity> foundTiles = new NativeList<Entity>(Allocator.Temp);
           NativeQueue<int2> addressQueue = new NativeQueue<int2>(Allocator.Temp);
           NativeHashSet<int2> visitedAddresses = new NativeHashSet<int2>(Allocator.Temp);

           addressQueue.Enqueue(startAddress);
           visitedAddresses.Add(startAddress);
           foundTiles.Add(startingTileEntity);

           while (addressQueue.Count > 0)
           {
               int2 currentAddress = addressQueue.Dequeue();

               int2[] neighborOffsets = { new int2(1, 0), new int2(-1, 0), new int2(0, 1), new int2(0, -1) };

               foreach (int2 offset in neighborOffsets)
               {
                   int2 neighborAddress = currentAddress + offset;

                   if (!visitedAddresses.Contains(neighborAddress))
                   {

                       Entity neighborEntity = -1;
                       var neighborTiles = tileQuery.ToComponentDataArray<TileItemComponent>(Allocator.Temp);

                        for (int i = 0; i < neighborTiles.Length; i++)
                        {
                          if (neighborTiles[i].Address.Equals(neighborAddress))
                          {
                              neighborEntity = tileQuery.ToEntityArray(Allocator.Temp)[i];
                              break;
                          }
                        }

                       neighborTiles.Dispose();


                       if (neighborEntity != -1 && entityManager.GetComponentData<TileItemComponent>(neighborEntity).TileType == targetTileType)
                       {
                           foundTiles.Add(neighborEntity);
                           addressQueue.Enqueue(neighborAddress);
                           visitedAddresses.Add(neighborAddress);
                       }
                   }
               }
           }

           addressQueue.Dispose();
           visitedAddresses.Dispose();
           return foundTiles;
       }
   }
    */