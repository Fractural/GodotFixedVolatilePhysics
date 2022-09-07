using FixMath.NET;
using Fractural.Utils;
using Godot;
using VirtualJoystickAddon;
using Volatile;
using Volatile.GodotEngine;

namespace Tests
{
    [Tool]
    public class KinematicFixedMovement : VolatileKinematicBody
    {
        private Fix64 speed;
        public Fix64 Speed
        {
            get
            {

#if TOOLS
                if (Engine.EditorHint)
                    return VoltType.DeserializeOrDefault<Fix64>(_speed);
                else
#endif
                    return speed;
            }
            set
            {
#if TOOLS
                if (Engine.EditorHint)
                    _speed = VoltType.Serialize(value);
                else
#endif
                    speed = value;
            }
        }
        [Export(hintString: VoltPropertyHint.Fix64)]
        private byte[] _speed;

        [Export]
        private NodePath joystickPath;
        private VirtualJoystick joystick;

        public override void _Ready()
        {
            base._Ready();
            speed = VoltType.DeserializeOrDefault<Fix64>(_speed);
            joystick = this.GetNodeAsWrapper<VirtualJoystick>(joystickPath);
        }

        public override void _PhysicsProcess(float delta)
        {
            base._PhysicsProcess(delta);
            if (Engine.EditorHint) return;
            if (joystick.Output != Vector2.Zero)
                MoveAndSlide(joystick.Output.ToVoltVector2() * speed);
        }
    }
}