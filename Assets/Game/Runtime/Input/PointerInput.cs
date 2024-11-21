#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Utilities;

namespace gs.chef.game.input
{
    public struct PointerInput
    {
        public bool Contact;

        public int InputId;

        public Vector2 Position;

        public Vector2? Delta;
    }

#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class PointerInputComposite : InputBindingComposite<PointerInput>
    {
        [InputControl(layout = "Button")] public int contact;

        [InputControl(layout = "Vector2")] public int position;

        [InputControl(layout = "Vector2")] public int delta;

        [InputControl(layout = "Integer")] public int inputId;

        public override PointerInput ReadValue(ref InputBindingCompositeContext context)
        {
            var contact = context.ReadValueAsButton(this.contact);
            var pointerId = context.ReadValue<int>(inputId);
            var position = context.ReadValue<Vector2, Vector2MagnitudeComparer>(this.position);
            var delta = context.ReadValue<Vector2, Vector2MagnitudeComparer>(this.delta);

            return new PointerInput
            {
                Contact = contact,
                InputId = pointerId,
                Position = position,
                Delta = delta,
            };
        }

#if UNITY_EDITOR
        static PointerInputComposite()
        {
            Register();
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            InputSystem.RegisterBindingComposite<PointerInputComposite>();
        }
    }
}