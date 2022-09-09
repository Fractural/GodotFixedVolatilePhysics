using Godot;
using Volatile.GodotEngine;

namespace Tests
{
    [Tool]
    public class VolatileAreaTest : VolatileArea
    {
        [Export]
        private NodePath hasEntityInsideSpritePath;
        private Sprite hasEntityInsideSprite;

        public override void _Ready()
        {
            base._Ready();
            if (Engine.EditorHint) return;
            hasEntityInsideSprite = GetNode<Sprite>(hasEntityInsideSpritePath);
        }

        protected override void OnBodyEntered(IVolatileBody body)
        {
            base.OnBodyEntered(body);
            OnBodyCollidingWithChanged();
        }

        protected override void OnBodyExited(IVolatileBody body)
        {
            base.OnBodyExited(body);
            OnBodyCollidingWithChanged();
        }

        private void OnBodyCollidingWithChanged()
        {
            hasEntityInsideSprite.Visible = CurrCollidingWith.Count > 0;
        }
    }
}