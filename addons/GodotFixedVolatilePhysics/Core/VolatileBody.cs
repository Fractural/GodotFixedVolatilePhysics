using FixMath.NET;
using Godot;
using System;
using System.Linq;
using Fractural.Utils;
using System.Collections.Generic;

namespace Volatile.GodotEngine
{
    [Tool]
    public class VolatileBody : VoltNode2D
    {
        public VolatileShape[] Shapes { get; set; }
        [Export]
        public bool IsStatic { get; set; } = false;
        [Export]
        public bool DoInterpolation { get; set; } = true;

        public VoltBody Body { get; private set; }

        // Interpolation
        private VoltVector2 lastPosition;
        private VoltVector2 nextPosition;

        private Fix64 lastAngle;
        private Fix64 nextAngle;


        public override string _GetConfigurationWarning()
        {
            var volatileWorld = this.GetAncestor<VolatileWorld>(false);
            if (volatileWorld == null)
                return $"This node must be a descendant of a VolatileWorld.";

            var shapes = this.GetDescendants<VolatileShape>();
            if (shapes.Count == 0)
                return "This node has no shape, so it can't collide or interact with other objects.\nConsider addinga VolatileShape (VolatilePolygon, VolatileRect, VolatileRect) as a child to define its shape.";
            return "";
        }

        public override void _Ready()
        {
            base._Ready();
#if TOOLS
            if (Engine.EditorHint) 
            {
                SetPhysicsProcess(false);
                return;
            }
#endif
            var volatileWorldNode = this.GetAncestor<VolatileWorld>(false);
            if (volatileWorldNode == null)
                return;

            var shapeNodes = this.GetDescendants<VolatileShape>();
            if (shapeNodes.Count == 0)
                return;

            var world = volatileWorldNode.World;
            var shapes = shapeNodes.Select(x => x.PrepareShape(world)).ToArray();

            if (IsStatic)
                Body = world.CreateStaticBody(GlobalFixedPosition, FixedRotation, shapes);
            else
                Body = world.CreateDynamicBody(GlobalFixedPosition, FixedRotation, shapes);

            lastPosition = nextPosition = GlobalFixedPosition;
            lastAngle = nextAngle = GlobalFixedRotation;
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            if (Engine.EditorHint) return;
            if (DoInterpolation)
            {
                Fix64 t = (Fix64)Engine.GetPhysicsInterpolationFraction();

                GlobalFixedPosition = VoltVector2.Lerp(lastPosition, nextPosition, t);
                Fix64 angle = Fix64.Lerp(lastAngle, nextAngle, t);
                GlobalFixedRotation = angle;
            }
            else
            {
                GlobalFixedPosition = Body.Position;
                GlobalFixedRotation = Body.Angle;
            }
        }

        public override void _PhysicsProcess(float delta)
        {
            lastPosition = nextPosition;
            lastAngle = nextAngle;
            nextPosition = Body.Position;
            nextAngle = Body.Angle;
        }

        public void AddForce(VoltVector2 force)
        {
            Body.AddForce(force);
        }

        public void AddTorque(Fix64 radians)
        {
            Body.AddTorque(radians);
        }

        public void SetState(VoltVector2 position, Fix64 radians)
        {
            Body.Set(position, radians);
        }
    }
}
