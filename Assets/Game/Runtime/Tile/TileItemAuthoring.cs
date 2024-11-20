using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using UnityEngine;

namespace gs.chef.game.tile
{
    public class TileItemAuthoring : MonoBehaviour
    {
        private class TileItemAuthoringBaker : Baker<TileItemAuthoring>
        {
            public override void Bake(TileItemAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent<AddressComponent>(entity);
                AddComponent<ClickableComponent>(entity);
                SetComponentEnabled<ClickableComponent>(entity, false);
                AddComponent<TileItemComponent>(entity);
                AddComponent<TileMovingComponent>(entity);
                SetComponentEnabled<TileMovingComponent>(entity, false);
            }
        }
    }
    
    public struct AddressComponent : IComponentData
    {
        public int2 Address;
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
    }
    
    public struct TileMovingComponent : IComponentData, IEnableableComponent
    {
        public float3 TargetPosition;
        public float Speed;
    }
}