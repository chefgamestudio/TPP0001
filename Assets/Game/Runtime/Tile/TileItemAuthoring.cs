using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Serialization;

namespace gs.chef.game.tile
{
    public class TileItemAuthoring : MonoBehaviour
    {
        [SerializeField] private float _matchingDuration = 0.5f;
        
        private class TileItemAuthoringBaker : Baker<TileItemAuthoring>
        {
            public override void Bake(TileItemAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent<ClickableComponent>(entity);
                SetComponentEnabled<ClickableComponent>(entity, false);
                
                AddComponent<TileItemComponent>(entity);
                
                AddComponent<TileMovingComponent>(entity);
                //SetComponentEnabled<TileMovingComponent>(entity, false);
                
                AddComponent(entity, new TileMatchAnimationTag
                {
                    CurrentScale = 1f,
                    TargetScale = 0.3f,
                    Duration = authoring._matchingDuration,
                    Timer = 0f
                });
                SetComponentEnabled<TileMatchAnimationTag>(entity, false);
            }
        }
    }
    
    public struct ClickableComponent : IComponentData, IEnableableComponent
    {
    }
    
    public struct TileItemComponent : IComponentData
    {
        public TileType TileType;
        public float3 Position;
        public quaternion Rotation;
        public float Scale;
        public int2 Address;
        public bool IsMatched;
        public Entity TileEntity;
    }
    
    public struct TileMovingComponent : IComponentData
    {
        public float3 TargetPosition;
        public float Speed;
        public bool IsMoving;
    }
    
    public struct TileMatchAnimationTag : IComponentData, IEnableableComponent
    {
        public float TargetScale;
        public float CurrentScale;
        public float Duration;
        public float Timer;
    }
}