using FixMath.NET;
using Godot;
using Volatile;
using Volatile.GodotEngine;

namespace Tests
{
    [Tool]
    public class FixedMovement : VoltNode2D
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

        public static readonly Fix64 FIX_0_0001 = Fix64.From("0.0001");

        public override void _PhysicsProcess(float delta)
        {
            var movementInput = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down").ToVoltVector2();
            if (movementInput != VoltVector2.Zero)
                FixedPosition += movementInput * speed;
            var rotation = Input.GetAxis("ui_left", "ui_right");
            if (rotation != 0)
                FixedRotation += (Fix64)rotation * speed * Fix64.Deg2Rad;
            var scale = Input.GetAxis("ui_up", "ui_down");
            if (scale != 0)
                FixedScale += VoltVector2.One * (Fix64)scale * speed * FIX_0_0001;
        }
    }
}