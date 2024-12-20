using EntitiesEvents;
using gs.chef.game.tile.events;
using gs.ChefDI;
using MessagePipe;
using Synthesis.App;
using Synthesis.SystemGroups;
using Unity.Entities;
using Unity.Logging;

namespace gs.chef.game.level
{
    [UpdateInGroup(typeof(SynthesisInitializationSystemGroup), OrderFirst = true)]
    public partial class LevelManageSystem : SystemBase
    {
        [Inject] private readonly LevelConfig _levelConfig;
        [Inject] private readonly IPublisher<AddMatchedTilesEvent> _addMatchedTilesEventPublisher;
        
        private EventWriter<OnChangeAppStateEvent> _onChangeAppStateEventWriter;
        private EventReader<OnChangeAppStateEvent> _onChangeAppStateEventReader;
        private EventReader<AddMatchedTilesEvent> _addMatchedTilesEventReader;

        protected override void OnCreate()
        {
            _addMatchedTilesEventReader = this.GetEventReader<AddMatchedTilesEvent>();
            _onChangeAppStateEventWriter = this.GetEventWriter<OnChangeAppStateEvent>();
            _onChangeAppStateEventReader = this.GetEventReader<OnChangeAppStateEvent>();
            RequireForUpdate<LevelConfigSystemAuthoring.SystemIsEnabledTag>();
        }

        protected override void OnUpdate()
        {
            var singletonEntity = SystemAPI.GetSingletonEntity<LevelConfigSystemAuthoring.SystemIsEnabledTag>();
            var component = SystemAPI.GetComponent<LevelConfigComponent>(singletonEntity);

            foreach (var eventData in _onChangeAppStateEventReader.Read())
            {
                if (eventData.AppState == AppState.LevelDataLoading)
                {
                    var attempt = _levelConfig.LevelAttempts;
                    component.LevelAttempts = attempt;
                    SystemAPI.SetComponent(singletonEntity, component);
                    _onChangeAppStateEventWriter.Write(
                        new OnChangeAppStateEvent { AppState = AppState.LevelDataLoaded });
                    Log.Info("Level attempts set to " + attempt);
                }
                else if (eventData.AppState == AppState.LevelDataLoaded)
                {
                    _onChangeAppStateEventWriter.Write(new OnChangeAppStateEvent
                        { AppState = AppState.LevelDataInitializing });
                    InitializeLevelData();
                }
                else if (eventData.AppState == AppState.LevelDataInitialized)
                {
                    _onChangeAppStateEventWriter.Write(new OnChangeAppStateEvent { AppState = AppState.LevelCreating });
                    SystemAPI.SetComponentEnabled<CreateLevelTilesTag>(singletonEntity, true);
                    //CreateLevel();
                }
            }
            
            foreach (var eventData in _addMatchedTilesEventReader.Read())
            {
                _addMatchedTilesEventPublisher.Publish(new AddMatchedTilesEvent
                {
                    TileType = eventData.TileType,
                    Count = eventData.Count
                });
            }
        }

        private void CreateLevel()
        {
            _onChangeAppStateEventWriter.Write(new OnChangeAppStateEvent { AppState = AppState.LevelCreated });
            Log.Info("CreateLevel");
        }

        private void InitializeLevelData()
        {
            Log.Info("InitializeLevelData");
            _onChangeAppStateEventWriter.Write(new OnChangeAppStateEvent { AppState = AppState.LevelDataInitialized });
        }
    }
}