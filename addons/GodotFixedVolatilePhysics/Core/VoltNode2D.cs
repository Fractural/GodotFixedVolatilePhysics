using Godot;
using Volatile;

namespace GodotFixedVolatilePhysics
{
    [Tool]
    // TODO: Finis hthis
    public class VoltNode2D : Node2D
    {
        public VoltTransform2D VTransform;

        public override void _Process(float delta)
        {
            if (Engine.EditorHint)
            {
                if (Vh)
            }
            else
            {
                SetProcess(false);
            }
        }
    }
}
