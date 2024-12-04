using Unity.Entities;
using UnityEngine;

namespace gs.chef.game.Grid
{
    public class GridCellAuthoring : MonoBehaviour
    {
        private class GridCellAuthoringBaker : Baker<GridCellAuthoring>
        {
            public override void Bake(GridCellAuthoring authoring)
            {
            }
        }
    }
}