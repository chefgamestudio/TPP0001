using gs.chef.game.app;
using gs.chef.game.input;
using gs.chef.game.level;
using gs.chef.game.level.events;
using gs.ChefDI;
using gs.ChefDI.Unity;
using MessagePipe;
using Synthesis.App;
using Synthesis.Extensions;

namespace gs.chef.game.core
{
    public class GameLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            #region MessagePipe

            var messagePipeOptions = builder.RegisterMessagePipe();
            builder.RegisterBuildCallback(s=> GlobalMessagePipe.SetProvider(s.AsServiceProvider()));

            #endregion

            #region MessagePipeEvents

            builder.RegisterMessageBroker<CurrentAppStateEvent>(messagePipeOptions);
            builder.RegisterMessageBroker<LoadLevelEvent>(messagePipeOptions);

            #endregion

            #region EntitiesEvents

            builder.RegisterEntityEvent<OnChangeAppStateEvent>(Lifetime.Singleton);

            #endregion

            #region App

            builder.Register<AppManager>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();

            #endregion

            #region Levels

            builder.Register<LevelConfig>(Lifetime.Singleton);
            builder.Register<LevelManager>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();

            #endregion
            

            #region Register ECS Systems

            builder.UseDefaultWorld(system =>
            {
                system.Add<AppStateSystem>();
                system.Add<LevelManageSystem>();
                system.Add<GameInputSystem>();
            });

            #endregion
        }
    }
}