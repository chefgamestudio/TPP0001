using EntitiesEvents;
using gs.chef.game.tile;
using Synthesis.App;
using Synthesis.SystemGroups;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace gs.chef.game.level
{
    [UpdateInGroup(typeof(SynthesisFirstSimulationGroup), OrderFirst = true)]
    public partial struct CreateLevelTilesSystem : ISystem
    {
        private EventReader<OnChangeAppStateEvent> _onChangeAppStateEventReader;

        private NativeArray<float3> positions;
        private NativeArray<int2> addresses;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<LevelConfigSystemAuthoring.SystemIsEnabledTag>();
            _onChangeAppStateEventReader = state.GetEventReader<OnChangeAppStateEvent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var levelConfigEntity = SystemAPI.GetSingletonEntity<LevelConfigSystemAuthoring.SystemIsEnabledTag>();

            bool isActive = SystemAPI.IsComponentEnabled<CreateLevelTilesTag>(levelConfigEntity);

            if (isActive)
            {
                SystemAPI.SetComponentEnabled<CreateLevelTilesTag>(levelConfigEntity, false);
                var levelConfigData = SystemAPI.GetComponent<LevelConfigComponent>(levelConfigEntity);
                var startTransform = SystemAPI.GetComponent<LocalTransform>(levelConfigEntity);
                var initialPosition = startTransform.Position;
                var createTilesConfigData = SystemAPI.GetComponent<LevelTilesConfigComponent>(levelConfigEntity);

                var rows = levelConfigData.TotalRows;
                var columns = levelConfigData.Columns;
                var vistaRows = levelConfigData.VistaRows;
                
                //var scale = 1;

                var gridSize = columns * vistaRows;

                positions = new NativeArray<float3>(gridSize, Allocator.TempJob);
                addresses = new NativeArray<int2>(gridSize, Allocator.TempJob);

                var createLevelTilesJob = new CreateLevelTilesJob()
                {
                    Columns = columns,
                    Padding = new float2(0.05f, 0.05f),
                    InitialPosition = initialPosition,
                    Scale = 1f,
                    Positions = positions,
                    Addresses = addresses
                };

                state.Dependency = createLevelTilesJob.Schedule();
                state.Dependency.Complete();

                var seed = (uint)levelConfigData.LevelAttempts;
                var random = new Random(seed);

                for (int i = 0; i < positions.Length; i++)
                {
                    var tileTypeInt = random.NextInt(1, 5);
                    var tileType = (TileType)tileTypeInt;
                    
                    
                    var tileEntity = state.EntityManager.Instantiate(createTilesConfigData.GetTilePrefab(tileType));
                    
                    
                    
                    state.EntityManager.SetComponentData(tileEntity, new LocalTransform
                    {
                        Position = positions[i] + new float3(0, 14f, 0),
                        Rotation = quaternion.identity,
                        Scale = 1f
                    });

                    state.EntityManager.SetComponentData(tileEntity, new AddressComponent
                    {
                        Address = addresses[i]
                    });

                    state.EntityManager.SetComponentData(tileEntity, new TileItemComponent
                    {
                        TileType = tileType,
                        Position = positions[i] + new float3(0, 14f, 0),
                        Rotation = Quaternion.identity,
                        Scale = 1f
                    });

                    state.EntityManager.SetComponentData(tileEntity, new TileMovingComponent
                    {
                        TargetPosition = positions[i],
                        Speed = createTilesConfigData.MovingSpeed
                    });

                    //state.EntityManager.SetComponentEnabled<MaterialMeshInfo>(tileEntity, addresses[i].y < vistaRows);

                    state.EntityManager.SetComponentEnabled<ClickableComponent>(tileEntity, false);
                    state.EntityManager.SetComponentEnabled<TileMovingComponent>(tileEntity, true);
                }

                positions.Dispose();
                addresses.Dispose();

                
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            if (positions.IsCreated)
            {
                positions.Dispose();
            }

            if (addresses.IsCreated)
            {
                addresses.Dispose();
            }
        }
    }

    [BurstCompile]
    public struct CreateLevelTilesJob : IJob
    {
        public int Columns;
        public float2 Padding;
        public float3 InitialPosition;
        public float Scale;
        public NativeArray<float3> Positions;
        public NativeArray<int2> Addresses;


        [BurstCompile]
        public void Execute()
        {
            float rowWidth = -1 + (Columns * Scale) + (Columns - 1) * Padding.x;
            float startX = InitialPosition.x - (rowWidth * 0.5f);
            float startY = InitialPosition.y;

            for (int i = 0; i < Positions.Length; i++)
            {
                int row = i / Columns;
                int column = i % Columns;

                float x = startX + column * (Scale + Padding.x);
                float y = startY + row * (Scale + Padding.y);

                Positions[i] = new float3(x, y, InitialPosition.z);
                Addresses[i] = new int2(column, row);
            }
        }
    }
}