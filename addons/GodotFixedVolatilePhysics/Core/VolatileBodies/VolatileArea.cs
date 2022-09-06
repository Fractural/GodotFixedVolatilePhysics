using Godot;

namespace Volatile.GodotEngine
{
    [Tool]
    public class VolatileArea : VolatileBody
    {
        protected override VoltBody CreateBody(VoltWorld world, VoltShape[] shapes)
            => world.CreateTriggerBody(GlobalFixedPosition, GlobalFixedRotation, shapes);

        public VoltBodyCollisionResult QueryCollisions(bool collideDynamic, VoltCollisionFilter filter)
        {
            return Body.QueryTriggerCollisions(collideDynamic, filter);
        }
    }
}
