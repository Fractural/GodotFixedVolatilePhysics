using Godot;
using Godot.Collections;
using Fractural;
using FixMath.NET;
using Volatile.GodotEngine.Plugin;

namespace Volatile.GodotEngine
{
    [Tool]
    public class VolatileCircle : VolatileShape
    {
        public override VoltShape PrepareShape(VoltWorld world)
        {
            return world.CreateCircleWorldSpace(GlobalFixedPosition, Radius);
        }

        public override Vector2 ComputeLocalCenterOfMass()
        {
            return Vector2.Zero;
        }

        public override void _Ready()
        {
            base._Ready();
            if (Engine.EditorHint)
                _OnRadiusSet = VoltType.DeserializeOrDefault<Fix64>(_radius);
            else
                Radius = VoltType.DeserializeOrDefault<Fix64>(_radius);
        }

        #region Radius
        protected Fix64 radius;
        public Fix64 Radius
        {
            get
            {
#if TOOLS
                if (Engine.EditorHint)
                    return VoltType.DeserializeOrDefault<Fix64>(_radius);
                else
#endif
                    return radius;
            }
            set
            {
#if TOOLS
                if (Engine.EditorHint)
                    _radius = VoltType.Serialize(value);
                else
#endif
                    radius = value;
            }
        }
        [Export(hintString: VoltPropertyHint.Fix64 + ",set:" + nameof(_OnRadiusSet))]
        public byte[] _radius = VoltType.Serialize(Fix64.From(1));

        public float EditorRadius { get; set; }
        private Fix64 _OnRadiusSet
        {
            set
            {
                EditorRadius = (float)value;
                Update();
            }
        }
        #endregion

#if TOOLS
        public override void _Draw()
        {
            base._Draw();
            if (!Engine.EditorHint || EditorRadius == 0) return;
            var radius = EditorRadius;

            var color = Palette.Main;
            var fill = color;
            fill.a = 0.075f;

            var circumference = 2 * Mathf.Pi * radius;
            var points = Mathf.Max((int)(circumference * 10), 10);
            DrawLine(Vector2.Zero, new Vector2(radius, 0), color);
            DrawArc(Vector2.Zero, radius, 0, 2 * Mathf.Pi, points, color);
            DrawCircle(Vector2.Zero, radius, fill);
        }
#endif
    }
}
