using Unity.Entities;

namespace gs.chef.game.tile.events
{
    public struct ClickedTileEvent
    {
        public Entity ClickedTile;
    }
    
    public struct AddMatchedTilesEvent
    {
        public TileType TileType;
        public int Count;
    }
    
    public struct DropDownTilesEvent{}
    
    public struct SpawnNewTileEvent {}
}