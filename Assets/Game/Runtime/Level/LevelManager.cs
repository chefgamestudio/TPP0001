using System;
using EntitiesEvents;
using gs.chef.game.level.events;
using gs.chef.game.tile.events;
using gs.ChefDI;
using MessagePipe;
using Synthesis.App;
using Synthesis.Core;
using Unity.Logging;

namespace gs.chef.game.level
{
    public class LevelManager : BaseSubscribable
    {
        //[Inject] private readonly LevelConfig _levelConfig;
        
        #region Event Subscribers

        [Inject] private readonly ISubscriber<CurrentAppStateEvent> _currentAppStateEventSubscriber;
        [Inject] private readonly ISubscriber<LoadLevelEvent> _loadLevelEventSubscriber;
        [Inject] private readonly ISubscriber<AddMatchedTilesEvent> _addingNewEventArgsSubscriber;

        #endregion
        
        #region Event Publishers

        [Inject] private readonly Func<EventWriter<OnChangeAppStateEvent>> _onChangeAppStateEventWriter;
        [Inject] private readonly IPublisher<LoadLevelEvent> _loadLevelEventPublisher;

        #endregion


        protected override void Subscriptions()
        {
            _currentAppStateEventSubscriber.Subscribe(e => OnChangeAppState(e)).AddTo(_bagBuilder);
            _loadLevelEventSubscriber.Subscribe(e => OnLoadLevel(e)).AddTo(_bagBuilder);
            _addingNewEventArgsSubscriber.Subscribe(e => OnAddingNewEventArgs(e)).AddTo(_bagBuilder);
        }

        private void OnAddingNewEventArgs(AddMatchedTilesEvent e)
        {
            Log.Warning($"MATCHING TILES: {e.Count} of type {e.TileType}");
        }

        private void OnLoadLevel(LoadLevelEvent e)
        {
            _onChangeAppStateEventWriter().Write(new OnChangeAppStateEvent
            {
                AppState = AppState.LevelDataLoading
            });
        }

        private void OnChangeAppState(CurrentAppStateEvent e)
        {
            if (e.AppState.Equals(AppState.Ready))
            {
                
                _loadLevelEventPublisher.Publish(new LoadLevelEvent());
            }
        }
    }
}