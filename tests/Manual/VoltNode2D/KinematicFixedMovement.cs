using FixMath.NET;
using Godot;
using Volatile;
using Volatile.GodotEngine;

namespace Tests
{
    [Tool]
    public class KinematicFixedMovement : VolatileBody
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

        public override void _Ready()
        {
            base._Ready();
            speed = VoltType.DeserializeOrDefault<Fix64>(_speed);
        }

        public override void _PhysicsProcess(float delta)
        {
            base._PhysicsProcess(delta);
            if (Engine.EditorHint) return;
            var movementInput = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down").ToVoltVector2();
            MoveAndCollide(movementInput * speed);
        }
    }
}