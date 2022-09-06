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
        public VoltBodyType BodyType { get; set; } = VoltBodyType.Static;
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

            switch (BodyType)
            {
                case VoltBodyType.Static:
                    Body = world.CreateStaticBody(GlobalFixedPosition, FixedRotation, shapes);
                    break;
                case VoltBodyType.Kinematic:
                    Body = world.CreateKinematicBody(GlobalFixedPosition, FixedRotation, shapes);
                    break;
                case VoltBodyType.Dynamic:
                    Body = world.CreateDynamicBody(GlobalFixedPosition, FixedRotation, shapes);
                    break;
                case VoltBodyType.Trigger:
                    Body = world.CreateTriggerBody(GlobalFixedPosition, FixedRotation, shapes);
                    break;
            }

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

        public void Set(VoltVector2 position, Fix64 radians)
        {
            Body.Set(position, radians);
        }

        public void SetVelocity(VoltVector2 linearVelocity, Fix64 angularVelocity)
        {
            Body.LinearVelocity = linearVelocity;
            Body.AngularVelocity = angularVelocity;
        }

        public void SetForce(VoltVector2 force, Fix64 torque, VoltVector2 biasVelocity, Fix64 biasRotation)
        {
            Body.SetForce(force, torque, biasVelocity, biasRotation);
        }

        public VoltKinematicCollisionResult MoveAndCollide(VoltVector2 linearVelocity)
        {
            return Body.MoveAndCollide(linearVelocity);
        }

        public void MoveAndSlide(VoltVector2 linearVelocity, int maxSlides = 4)
        {
            Body.MoveAndSlide(linearVelocity, maxSlides);
        }
    }
}
