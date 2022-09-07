using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Volatile.GodotEngine
{
    [Tool]
    public class VolatileArea : VolatileBody
    {
        public delegate void BodyEnteredDelegate(VolatileBody body);
        public delegate void BodyExitedDelegate(VolatileBody body);

        public BodyEnteredDelegate BodyEntered;
        public BodyExitedDelegate BodyExited;

        [Export]
        public bool AutoQuery { get; set; } = false;
        [Export]
        public bool AutoQueryDynamicBodies { get; set; } = true;

        /// <summary>
        /// All the bodies that this area was previous colliding with.
        /// </summary>
        protected HashSet<VolatileBody> PrevCollidingWith { get; set; }
        /// <summary>
        /// All the bodies that this area is currently colliding with.
        /// </summary>
        protected HashSet<VolatileBody> CurrCollidingWith { get; set; }

        protected override VoltBody CreateBody(VoltWorld world, VoltShape[] shapes)
            => world.CreateTriggerBody(GlobalFixedPosition, GlobalFixedRotation, shapes, Layer, Mask);

        public VoltBodyCollisionResult QueryCollisions(bool collideDynamic = false)
            => QueryCollisions(collideDynamic, VoltCollisionFilters.DefaultWorldCollisionFilter);
        public VoltBodyCollisionResult QueryCollisions(bool collideDynamic, VoltCollisionFilter filter)
        {
            return Body.QueryTriggerCollisions(collideDynamic, filter);
        }

        public override void _Ready()
        {
            base._Ready();
            if (Engine.EditorHint) return;
            if (AutoQuery)
            {
                PrevCollidingWith = new HashSet<VolatileBody>();
                CurrCollidingWith = new HashSet<VolatileBody>();
            }
        }

        protected override void OnBodyCollided(VoltBody body)
        {
            base.OnBodyCollided(body);
            if (AutoQuery && body.UserData is VolatileBody volatileBody)
                CurrCollidingWith.Add(volatileBody);
        }

        public override void _PhysicsProcess(float delta)
        {
            base._PhysicsProcess(delta);
            if (Engine.EditorHint) return;
            if (AutoQuery)
            {
                foreach (var oldCollidingBody in PrevCollidingWith)
                    if (!CurrCollidingWith.Contains(oldCollidingBody))
                        OnBodyExited(oldCollidingBody);

                foreach (var newCollidingBody in CurrCollidingWith)
                    if (!PrevCollidingWith.Contains(newCollidingBody))
                        OnBodyEntered(newCollidingBody);

                // Swap PreviousColliding with CurrColliding, and then clear CurrColliding
                // This effectively transfers CurrColliding into PreviousColliding, and 
                // clears CurrColliding for future use.
                var temp = PrevCollidingWith;
                PrevCollidingWith = CurrCollidingWith;
                CurrCollidingWith = temp;
                CurrCollidingWith.Clear();
            }
        }

        protected virtual void OnBodyEntered(VolatileBody body)
        {
            BodyEntered?.Invoke(body);
        }

        protected virtual void OnBodyExited(VolatileBody body)
        {
            BodyExited?.Invoke(body);
        }
    }
}
