using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using EntitiesEvents;
using gs.ChefDI;
using MessagePipe;
using Synthesis.App;
using Synthesis.Core;

namespace gs.chef.game.app
{
    public class AppManager : BaseSubscribable
    {
        [Inject] private readonly Func<EventWriter<OnChangeAppStateEvent>> _onChangeAppStateEventWriter;
        [Inject] private readonly ISubscriber<CurrentAppStateEvent> _currentAppStateEventSubscriber;

        protected override void Subscriptions()
        {
            _currentAppStateEventSubscriber.Subscribe(e => OnChangeAppState(e)).AddTo(_bagBuilder);
            InitializeGame(Token).Forget();
        }

        private void OnChangeAppState(CurrentAppStateEvent e)
        {
            if (e.AppState == AppState.Initialized)
            {
                _onChangeAppStateEventWriter().Write(new OnChangeAppStateEvent { AppState = AppState.Ready });
            }
        }
        
        private async UniTask InitializeGame(CancellationToken token)
        {
            _onChangeAppStateEventWriter().Write(new OnChangeAppStateEvent { AppState = AppState.Initializing });
            await UniTask.Delay(2000, cancellationToken: token);
            _onChangeAppStateEventWriter().Write(new OnChangeAppStateEvent { AppState = AppState.Initialized });
        } 
    }
}