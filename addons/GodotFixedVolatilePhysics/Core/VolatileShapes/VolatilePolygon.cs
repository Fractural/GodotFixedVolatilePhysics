using Godot;
using Volatile;
using System.Collections.Generic;
using Godot.Collections;
using Fractural.Utils;
using Fractural;
using FixMath.NET;
using System.Linq;
using System.Collections.ObjectModel;
using Volatile.GodotEngine.Plugin;

namespace Volatile.GodotEngine
{
    [Tool]
    public class VolatilePolygon : VolatileShape
    {
        public override VoltShape PrepareShape(VoltWorld world)
        {
            var globalPosition = GlobalFixedPosition;
            return world.CreatePolygonWorldSpace(
              Points.Select(x => x + globalPosition).ToArray(),
              Density,
              Friction,
              Restitution);
        }

        public override Vector2 ComputeGlobalCenterOfMass()
        {
            var points = Points;
            float length = points.Length;
            Vector2 sum = Vector2.Zero;
            foreach (var point in points)
                sum += point.ToGDVector2();
            return GlobalPosition + new Vector2(sum.x / length, sum.y / length);
        }

        #region Points
        private VoltVector2[] points;
        public VoltVector2[] Points
        {
            get
            {
#if TOOLS
                if (Engine.EditorHint)
                    return VoltType.Deserialize<VoltVector2[]>(_points);
                else
#endif
                    return points;
            }
            set
            {
#if TOOLS
                if (Engine.EditorHint)
                {
                    _points = VoltType.Serialize(value);
                    _PointsForward = value;
                }
                else
#endif
                    points = value;
            }
        }
        // Note that >tempPoints fowards the unserialized value to a property named tempPoints;
        [Export(hintString: VoltPropertyHint.Array + "," + VoltPropertyHint.VoltVector2 + ",>" + nameof(_PointsForward))]
        public byte[] _points;

        public Vector2[] EditorGDPoints { get; set; }
        private VoltVector2[] _PointsForward
        {
            set
            {
                EditorGDPoints = value.Select(x => x.ToGDVector2()).ToArray();
                Update();
            }
        }

        #endregion

        public override void _Ready()
        {
            base._Ready();
            if (Engine.EditorHint)
                _PointsForward = VoltType.Deserialize<VoltVector2[]>(_points);
            else
                Points = VoltType.Deserialize<VoltVector2[]>(_points);
        }

#if TOOLS
        public override void _Draw()
        {
            if (!Engine.EditorHint || EditorGDPoints == null) return;
            var points = EditorGDPoints;
            if (points.Length > 0)
            {
                var color = Palette.Main;
                var fill = color;
                fill.a = 0.075f;

                var previousPoint = points.Last();
                foreach (var point in points)
                {
                    DrawLine(previousPoint, point, color, 1, true);
                    previousPoint = point;
                }
                var polygonColors = new Color[points.Length];
                polygonColors.Populate(fill);
                DrawPolygon(points.ToArray(), polygonColors);
            }
        }
#endif
    }
}
