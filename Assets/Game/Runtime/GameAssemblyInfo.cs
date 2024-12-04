
using EntitiesEvents;
using gs.chef.game.input;
using gs.chef.game.level.events;
using gs.chef.game.tile.events;

[assembly: RegisterEvent(typeof(UpdateLevelAttemptsEvent))]
[assembly: RegisterEvent(typeof(InputActiveEvent))]
[assembly: RegisterEvent(typeof(ClickedTileEvent))]
[assembly: RegisterEvent(typeof(AddMatchedTilesEvent))]
[assembly: RegisterEvent(typeof(SpawnNewTileEvent))]
[assembly: RegisterEvent(typeof(DropDownTilesEvent))]

namespace gs.chef.game{}