using Godot;
using System.Collections.Generic;

namespace Volatile.GodotEngine
{
    [Tool]
    public class VolatileArea : VolatileBody
    {
        [Export]
        public bool AutoQuery { get; set; }

        public HashSet<Node> CollidedWith { get; set; }

        protected override VoltBody CreateBody(VoltWorld world, VoltShape[] shapes)
            => world.CreateTriggerBody(GlobalFixedPosition, GlobalFixedRotation, shapes, Layer, Mask);

        public VoltBodyCollisionResult QueryCollisions(bool collideDynamic, VoltCollisionFilter filter)
        {
            return Body.QueryTriggerCollisions(collideDynamic, filter);
        }

        public override void _PhysicsProcess(float delta)
        {
            base._PhysicsProcess(delta);
            if (AutoQuery)
            {
                //var results = QueryCollisions(true, );
            }
        }
    }
}
