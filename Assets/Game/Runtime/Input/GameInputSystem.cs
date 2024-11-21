using EntitiesEvents;
using gs.chef.game.tile;
using gs.chef.game.tile.events;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Logging;
using Unity.Physics;
using Unity.Physics.Authoring;
using UnityEngine;
using UnityEngine.InputSystem;
using RaycastHit = Unity.Physics.RaycastHit;

namespace gs.chef.game.input
{
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    public partial class GameInputSystem : SystemBase
    {
        private EventReader<InputActiveEvent> _inputActiveEventReader;
        private EventWriter<ClickedTileEvent> _clickedTileEventWriter;
        private InputControls _inputs;

        private bool InputActive { get; set; }
        private PointerInput? _pointerInput;

        private bool _isContact;

        protected override void OnCreate()
        {
            _inputs = new InputControls();
            _clickedTileEventWriter = this.GetEventWriter<ClickedTileEvent>();
            _inputActiveEventReader = this.GetEventReader<InputActiveEvent>();
        }

        protected override void OnStartRunning()
        {
            _inputs.Enable();
        }

        protected override void OnStopRunning()
        {
            _inputs.Disable();
        }

        protected override void OnUpdate()
        {
            foreach (var eventData in _inputActiveEventReader.Read())
            {
                InputActive = eventData.IsActive;
            }

            if (!InputActive)
                return;

            _pointerInput = _inputs.pointer.point.ReadValue<PointerInput>();

            if (_pointerInput.Value.Contact && !_isContact)
            {
                _isContact = true;
                Log.Warning($"Pointer Down: {_pointerInput.Value.Position}");
            }

            if (_pointerInput.Value.Contact && _isContact)
            {
                var delta = _pointerInput.Value.Delta;
                if (delta.HasValue && delta.Value.sqrMagnitude > 0)
                {
                    Log.Warning($"Pointer Drag: {delta.Value}");
                }
                //Log.Warning($"Pointer Move: {_pointerInput.Value.Position}");
            }

            if (!_pointerInput.Value.Contact && _isContact)
            {
                _isContact = false;
                Log.Warning($"Pointer Up: {_pointerInput.Value.Position}");
                var world = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;
                var ray = Camera.main.ScreenPointToRay(_pointerInput.Value.Position);
                NativeReference<RaycastHit> hitReference = new NativeReference<RaycastHit>(Allocator.TempJob);

                Dependency = new RaycastJob
                {
                    RayInput = new RaycastInput
                    {
                        Start = ray.origin,
                        End = ray.origin + ray.direction * 1000f,
                        //Filter = CollisionFilter.Default
                        Filter = new CollisionFilter
                        {
                            BelongsTo = (1u << 1),
                            CollidesWith = (1u << 0),
                            GroupIndex = 0
                        }
                    },
                    CollisionWorld = world.CollisionWorld,
                    Hit = hitReference
                }.Schedule(Dependency);

                Dependency.Complete();

                var hit = hitReference.Value;

                Entity hitEntity = hit.Entity;

                if (hitEntity != Entity.Null)
                {
                    var clickableComponent = SystemAPI.HasComponent<ClickableComponent>(hitEntity);
                    var isTileComponent = SystemAPI.HasComponent<TileItemComponent>(hitEntity);
                    var isActiveClickable = SystemAPI.IsComponentEnabled<ClickableComponent>(hitEntity);

                    if (isTileComponent && clickableComponent && isActiveClickable)
                    {
                        Log.Warning("Tile Clicked");
                        _clickedTileEventWriter.Write(new ClickedTileEvent
                        {
                            ClickedTile = hitEntity
                        });
                    }
                }
                else
                {
                    Log.Warning("No entity hit");
                }

                hitReference.Dispose();
            }
        }
    }

    public struct RaycastJob : IJob
    {
        public RaycastInput RayInput;
        [ReadOnly] public CollisionWorld CollisionWorld;
        public NativeReference<RaycastHit> Hit;

        [BurstCompile]
        public void Execute()
        {
            if (CollisionWorld.CastRay(RayInput, out var hit))
            {
                Hit.Value = hit;
            }
        }
    }
}