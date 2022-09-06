using Godot;
using System;

namespace Tests
{
    public class GDKinematicMovement : KinematicBody2D
    {
        [Export]
        public float speed;

        public override void _PhysicsProcess(float delta)
        {
            var movementInput = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
            if (movementInput != Vector2.Zero)
                MoveAndSlide(movementInput * speed);
        }
    }
}