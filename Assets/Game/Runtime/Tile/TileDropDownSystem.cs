using System.Collections.Generic;
using EntitiesEvents;
using gs.chef.game.Grid;
using gs.chef.game.level;
using gs.chef.game.tile.events;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace gs.chef.game.tile
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(TileMatchingAnimationSystem))]
    public partial class TileDropDownSystem : SystemBase
    {
        private EventWriter<SpawnNewTileEvent> _spawnNewTilesEvent;
        private EntityQuery _gridQuery;
        private EntityQuery _tileQuery;
        private EventReader<AddMatchedTilesEvent> _addMatchedTilesEventReader;


        [BurstCompile]
        protected override void OnCreate()
        {
            _spawnNewTilesEvent = this.GetEventWriter<SpawnNewTileEvent>();
            _addMatchedTilesEventReader = this.GetEventReader<AddMatchedTilesEvent>();
            RequireForUpdate<LevelConfigSystemAuthoring.SystemIsEnabledTag>();
            _gridQuery = EntityManager.CreateEntityQuery(typeof(GridCellComponent));
            
        }

        //[BurstCompile]
        protected override void OnUpdate()
        {
            var checkEvent = false;
            foreach (var eventData in _addMatchedTilesEventReader.Read())
            {
                if (eventData.Count > 0)
                {
                    checkEvent = true;
                }
            }

            if (!checkEvent)
            {
                return;
            }
            
            _tileQuery = EntityManager.CreateEntityQuery(typeof(TileItemComponent));
            NativeArray<Entity> tileEntities = _tileQuery.ToEntityArray(Allocator.Temp);
            NativeArray<TileItemComponent> tileComponents =
                _tileQuery.ToComponentDataArray<TileItemComponent>(Allocator.Temp);

            /*SortJob<TileItemComponent, AddressComparer> sortJob = tileComponents.SortJob(new AddressComparer { });
            
            sortJob.Schedule(Dependency).Complete();*/
            
            //NativeArray<TileItemComponent> tileComponents = _tileQuery.ToComponentDataArray<TileItemComponent>(Allocator.Temp).Sort();
            
            //sort by address tileEntities
            /*for (int i = 0; i < tileComponents.Length; i++)
            {
                var tileComponent = tileComponents[i];
                var tileEntity = tileComponent.TileEntity;
                
                for (int j = i + 1; j < tileEntities.Length; j++)
                {
                    var tileEntityNext = tileEntities[j];
                    var tileComponentNext = SystemAPI.GetComponent<TileItemComponent>(tileEntityNext);
                    if (tileComponent.Address.y > tileComponentNext.Address.y)
                    {
                        (tileEntities[i], tileEntities[j]) = (tileEntities[j], tileEntities[i]);
                    }
                }
            }*/
            
            tileComponents.Sort(new AddressComparer());

            for (int e = 0; e < tileComponents.Length; e++)
            {
                var tileComponent = tileComponents[e];
                var tileEntity = tileComponent.TileEntity;
                
                var movingComponent = SystemAPI.GetComponent<TileMovingComponent>(tileEntity);
                var address = tileComponent.Address;
                if (tileComponent.IsMatched || address.y == 0 || movingComponent.IsMoving)
                {
                    continue;
                }
                
                //int? lastEmptyGridIndex = null;
                Entity lastEmptyEntity = Entity.Null;
                NativeArray<Entity> gridEntities = _gridQuery.ToEntityArray(Allocator.Temp);
                /*NativeArray<GridCellComponent> gridComponents =
                    _gridQuery.ToComponentDataArray<GridCellComponent>(Allocator.Temp);*/
                
                var startY = address.y - 1;
                for (int i = startY; i >= 0; i--)
                {
                    var emptyAddress = new int2(address.x, i);
                    //var isCellEmpty = false;
                    
                    for (int j = 0; j < gridEntities.Length; j++)
                    {
                        var gridComponent = SystemAPI.GetComponent<GridCellComponent>(gridEntities[j]);
                        if (gridComponent.Address.Equals(emptyAddress) && gridComponent.IsEmpty)
                        {
                            lastEmptyEntity = gridEntities[j];
                            // lastEmptyAddress = emptyAddress;
                            //lastEmptyGridIndex = j;
                        }
                    }
                }
                
                if (lastEmptyEntity != Entity.Null)
                {
                    
                    for (int i = 0; i < gridEntities.Length; i++)
                    {
                        var gridComponent = SystemAPI.GetComponent<GridCellComponent>(gridEntities[i]);
                        if (gridComponent.Address.Equals(address))
                        {
                            var gridCompOld = gridComponent;
                            var gridEntityOld = gridEntities[i];
                            gridCompOld.IsEmpty = true;
                            SystemAPI.SetComponent(gridEntityOld, gridCompOld);
                        }
                    }

                    var lastEmptyGridComp = SystemAPI.GetComponent<GridCellComponent>(lastEmptyEntity);
                    tileComponent.Address = lastEmptyGridComp.Address;
                    
                    movingComponent.TargetPosition = lastEmptyGridComp.Position;
                    movingComponent.IsMoving = true;
                    
                    //var gridComp = gridComponents[lastEmptyGridIndex.Value];
                    lastEmptyGridComp.IsEmpty = false;
                    SystemAPI.SetComponent(tileEntity, tileComponent);
                    SystemAPI.SetComponent(tileEntity, movingComponent);
                    SystemAPI.SetComponent(lastEmptyEntity, lastEmptyGridComp);
                }

                gridEntities.Dispose();
                
            }
            
            tileEntities.Dispose();
            tileComponents.Dispose();
            
            _spawnNewTilesEvent.Write(new SpawnNewTileEvent());
        }
    }

    public struct AddressComparer : IComparer<TileItemComponent>
    {
        public int Compare(TileItemComponent x, TileItemComponent y)
        {
            return x.Address.y.CompareTo(y.Address.y);
        }
    }


    /*public struct SortByAddressComparer : IComparer<Entity>
    {
        public int Compare(Entity x, Entity y)
        {
            var xComponent = SystemAPI.GetComponent<TileItemComponent>(x);
            var yComponent = SystemAPI.GetComponent<TileItemComponent>(y);
            return xComponent.Address.y.CompareTo(yComponent.Address.y);
        }
    }*/
}