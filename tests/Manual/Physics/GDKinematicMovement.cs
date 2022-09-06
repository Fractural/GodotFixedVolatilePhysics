using Godot;
using System;
using VirtualJoystickAddon;
using Fractural.Utils;

namespace Tests
{
    public class GDKinematicMovement : KinematicBody2D
    {
        [Export]
        public float speed;
        [Export]
        private NodePath joystickPath;
        private VirtualJoystick joystick;

        public override void _Ready()
        {
            joystick = this.GetNodeAsWrapper<VirtualJoystick>(joystickPath);
        }

        public override void _PhysicsProcess(float delta)
        {
            if (joystick.Output != Vector2.Zero)
                MoveAndSlide(joystick.Output * speed);
        }
    }
}