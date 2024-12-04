using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace gs.chef.game.Grid
{
    public class GridCellAuthoring : MonoBehaviour
    {
        private class GridCellAuthoringBaker : Baker<GridCellAuthoring>
        {
            public override void Bake(GridCellAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent<GridCellComponent>(entity);
            }
        }
    }
    
    public struct GridCellComponent : IComponentData
    {
        public int2 Address;
        public bool IsEmpty;
        public float3 Position;
    }
}