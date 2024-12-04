using EntitiesEvents;
using gs.chef.game.Grid;
using gs.chef.game.level;
using gs.chef.game.tile.events;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace gs.chef.game.tile
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(TileDropDownSystem))]
    public partial class SpawnNewTileSystem : SystemBase
    {
        private EventReader<SpawnNewTileEvent> _spawnNewTileEvent;
        private EntityQuery _gridQuery;

        protected override void OnCreate()
        {
            _spawnNewTileEvent = this.GetEventReader<SpawnNewTileEvent>();
            RequireForUpdate<LevelConfigSystemAuthoring.SystemIsEnabledTag>();
        }

        protected override void OnUpdate()
        {
            bool isCheck = false;
            foreach (var eventData in _spawnNewTileEvent.Read())
            {
                isCheck = true;
            }

            if (!isCheck)
            {
                return;
            }

            var levelConfigEntity = SystemAPI.GetSingletonEntity<LevelConfigSystemAuthoring.SystemIsEnabledTag>();
            var createTilesConfigData = SystemAPI.GetComponent<LevelTilesConfigComponent>(levelConfigEntity);
            var levelConfigData = SystemAPI.GetComponent<LevelConfigComponent>(levelConfigEntity);
            //var startTransform = SystemAPI.GetComponent<LocalTransform>(levelConfigEntity);
            //var initialPosition = startTransform.Position;

            _gridQuery = EntityManager.CreateEntityQuery(typeof(GridCellComponent));
            var gridEntities = _gridQuery.ToEntityArray(Allocator.Temp);

            var seed = (uint)levelConfigData.LevelAttempts;
            var random = new Random(seed);

            foreach (var gridEntity in gridEntities)
            {
                var gridCellComp = SystemAPI.GetComponent<GridCellComponent>(gridEntity);
                if (gridCellComp.IsEmpty)
                {
                    gridCellComp.IsEmpty = false;

                    SystemAPI.SetComponent(gridEntity, gridCellComp);
                    var tileType = (TileType)random.NextInt(1, 5);
                    var tileEntity = EntityManager.Instantiate(createTilesConfigData.GetTilePrefab(tileType));

                    SystemAPI.SetComponent(tileEntity, new LocalTransform
                    {
                        Position = gridCellComp.Position + new float3(0, 14f, 0),
                        Rotation = quaternion.identity,
                        Scale = 1f
                    });

                    SystemAPI.SetComponent(tileEntity, new TileItemComponent
                    {
                        Address = gridCellComp.Address,
                        TileType = tileType,
                        Position = gridCellComp.Position + new float3(0, 14f, 0),
                        Rotation = quaternion.identity,
                        Scale = 1f,
                        IsMatched = false,
                        TileEntity = tileEntity
                    });

                    SystemAPI.SetComponent(tileEntity, new TileMovingComponent
                    {
                        TargetPosition = gridCellComp.Position,
                        Speed = createTilesConfigData.MovingSpeed,
                        IsMoving = true
                    });
                }
            }


            gridEntities.Dispose();
        }
    }
}