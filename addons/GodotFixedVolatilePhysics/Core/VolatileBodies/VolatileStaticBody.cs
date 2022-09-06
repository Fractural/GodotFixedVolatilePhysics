using Godot;

namespace Volatile.GodotEngine
{
    [Tool]
    public class VolatileStaticBody : VolatileBody
    {
        protected override VoltBody CreateBody(VoltWorld world, VoltShape[] shapes)
            => world.CreateStaticBody(GlobalFixedPosition, GlobalFixedRotation, shapes);

    }
}
