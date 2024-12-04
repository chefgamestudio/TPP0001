using gs.chef.game.tile;
using Synthesis.Core;
using Unity.Entities;
using UnityEngine;

namespace gs.chef.game.level
{
    public class LevelConfigSystemAuthoring : AbsEnabledSystemAuthoring
    {
        [Header("Level Config")]
        [SerializeField] private int _columns = 8;
        [SerializeField] private int _vistaRows = 8;
        [SerializeField] private int _totalRows = 10;
        
        [Space(height:5f)]
        [Header("Level Tiles Config")]
        [SerializeField] private GameObject _tile1Prefab;
        [SerializeField] private GameObject _tile2Prefab;
        [SerializeField] private GameObject _tile3Prefab;
        [SerializeField] private GameObject _tile4Prefab;
        [SerializeField] private float _scale;
        [SerializeField] private float _movingSpeed;
        
        [Space(height:5f)]
        [Header("Level Grids Config")]
        [SerializeField] private GameObject _gridCellPrefab;
        
        public struct SystemIsEnabledTag : IComponentData {}
        private class LevelConfigSystemAuthoringBaker : Baker<LevelConfigSystemAuthoring>
        {
            public override void Bake(LevelConfigSystemAuthoring authoring)
            {
                if (authoring._isSystemEnabled)
                {
                    Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                    AddComponent<SystemIsEnabledTag>(entity);
                    
                    AddComponent(entity, new LevelConfigComponent
                    {
                        LevelAttempts = 0,
                        Columns = authoring._columns,
                        VistaRows = authoring._vistaRows,
                        TotalRows = authoring._totalRows
                    });
                    
                    AddComponent(entity, new LevelTilesConfigComponent
                    {
                        Tile1Prefab = GetEntity(authoring._tile1Prefab, TransformUsageFlags.Dynamic),
                        Tile2Prefab = GetEntity(authoring._tile2Prefab, TransformUsageFlags.Dynamic),
                        Tile3Prefab = GetEntity(authoring._tile3Prefab, TransformUsageFlags.Dynamic),
                        Tile4Prefab = GetEntity(authoring._tile4Prefab, TransformUsageFlags.Dynamic),
                        Scale = authoring._scale,
                        MovingSpeed = authoring._movingSpeed
                    });
                    
                    AddComponent<CreateLevelTilesTag>(entity);
                    SetComponentEnabled<CreateLevelTilesTag>(entity, false);
                    
                    AddComponent<LevelGridsConfigComponent>(entity, new LevelGridsConfigComponent
                    {
                        GridCellPrefab = GetEntity(authoring._gridCellPrefab, TransformUsageFlags.None)
                    });
                    //SetComponentEnabled<LevelGridsConfigComponent>(entity, false);
                    
                    
                    //SetComponentEnabled<LevelTilesConfigComponent>(entity, false);
                }
            }
        }
    }

    public struct LevelGridsConfigComponent : IComponentData
    {
        public Entity GridCellPrefab;
    }
        
    
    public struct LevelConfigComponent : IComponentData
    {
        public int LevelAttempts;
        public int Columns;
        public int VistaRows;
        public int TotalRows;
    }
    
    public struct CreateLevelTilesTag : IComponentData, IEnableableComponent {}
    
    public struct LevelTilesConfigComponent : IComponentData, IEnableableComponent
    {
        public Entity Tile1Prefab;
        public Entity Tile2Prefab;
        public Entity Tile3Prefab;
        public Entity Tile4Prefab;
        public float Scale;
        public float MovingSpeed;
        
        public Entity GetTilePrefab(TileType type)
        {
            switch (type)
            {
                case TileType.Tile1:
                    return Tile1Prefab;
                case TileType.Tile2:
                    return Tile2Prefab;
                case TileType.Tile3:
                    return Tile3Prefab;
                case TileType.Tile4:
                    return Tile4Prefab;
                default:
                    return Entity.Null;
            }
        }
    }
}