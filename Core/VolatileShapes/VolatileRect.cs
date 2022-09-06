using Godot;
using Volatile;
using Godot.Collections;
using Fractural;
using FixMath.NET;
using System.Linq;
#if TOOLS
using Volatile.GodotEngine.Plugin;
#endif
using Fractural.Utils;

namespace Volatile.GodotEngine
{
    [Tool]
    public class VolatileRect : VolatileShape
    {
        public override VoltShape PrepareShape(VoltWorld world)
        {
            var globalPosition = GlobalFixedPosition;
            var points = new VoltVector2[]
            {
                new VoltVector2(globalPosition.x + Extents.x, globalPosition.y + Extents.y),
                new VoltVector2(globalPosition.x - Extents.x, globalPosition.y + Extents.y),
                new VoltVector2(globalPosition.x - Extents.x, globalPosition.y - Extents.y),
                new VoltVector2(globalPosition.x + Extents.x, globalPosition.y - Extents.y),
            };
            return world.CreatePolygonWorldSpace(
              points.Reverse().ToArray(),
              Density,
              Friction,
              Restitution);
        }

        public override Vector2 ComputeLocalCenterOfMass()
        {
            return Vector2.Zero;
        }

        public override void _Ready()
        {
            base._Ready();
            _OnExtentsSet = VoltType.DeserializeOrDefault<VoltVector2>(_extents);
            if (!Engine.EditorHint)
                Extents = VoltType.DeserializeOrDefault<VoltVector2>(_extents);
        }

        #region Rect
        private VoltVector2 size;
        public VoltVector2 Extents
        {
            get
            {
#if TOOLS
                if (Engine.EditorHint)
                    return VoltType.DeserializeOrDefault<VoltVector2>(_extents);
                else
#endif
                    return size;
            }
            set
            {
#if TOOLS
                if (Engine.EditorHint)
                    _extents = VoltType.Serialize(value);
                else
#endif
                    size = value;
            }
        }
        [Export(PropertyHint.None, VoltPropertyHint.VoltVector2 + ",set:" + nameof(_OnExtentsSet))]
        public byte[] _extents = VoltType.Serialize(VoltVector2.One);

        public Vector2 EditorExtents { get; set; }
        private VoltVector2 _OnExtentsSet
        {
            set
            {
                EditorExtents = value.ToGDVector2();
                Update();
            }
        }
        #endregion

        public override void _Draw()
        {
            base._Draw();
            if (!DebugDraw && (!Engine.EditorHint || EditorExtents == Vector2.Zero)) return;
            var extents = EditorExtents;
            var color = Palette.Main;
            var fill = color;
            fill.a = 0.075f;

            var points = new Vector2[] {
                extents,
                new Vector2(-extents.x, extents.y),
                new Vector2(-extents.x, -extents.y),
                new Vector2(extents.x, -extents.y)
            };
            this.DrawSegmentedPolyline(points, color);
            DrawColoredPolygon(points.ToArray(), fill);
        }
    }
}
