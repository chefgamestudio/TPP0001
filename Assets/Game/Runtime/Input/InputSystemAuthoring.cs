using Synthesis.Core;
using Unity.Entities;
using Unity.Mathematics;

namespace gs.chef.game.input
{
    public class InputSystemAuthoring : AbsEnabledSystemAuthoring
    {
        
        public struct SystemEnableTag : IComponentData{}
        
        private class InputSystemAuthoringBaker : Baker<InputSystemAuthoring>
        {
            public override void Bake(InputSystemAuthoring authoring)
            {
                if (authoring._isSystemEnabled)
                {
                    var entity = GetEntity(TransformUsageFlags.None);
                    AddComponent<SystemEnableTag>(entity);
                    
                    AddComponent<InputComponent>(entity);
                    SetComponentEnabled<InputComponent>(entity, false);
                }
            }
        }
    }
    
    public struct InputComponent : IComponentData, IEnableableComponent
    {
        public float2 PointerPosition;
        public Entity ClickedEntity;
    }
}