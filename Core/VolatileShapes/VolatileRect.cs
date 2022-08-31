using Godot;
using Volatile;
using Godot.Collections;
using Fractural;
using FixMath.NET;
using System.Linq;

namespace Volatile.GodotEngine
{
    [Tool]
    public class VolatileRect : VolatileShape
    {
        public override VoltShape PrepareShape(VoltWorld world)
        {
            var globalPosition = GlobalFixedPosition;
            return world.CreatePolygonWorldSpace(
              Rect.Points.Select(x => x + globalPosition).ToArray(),
              Density,
              Friction,
              Restitution);
        }

        public override Vector2 ComputeTrueCenterOfMass()
        {
            return Rect.GetCenter().ToGDVector2();
        }

        protected override void InitValues()
        {
            base.InitValues();
            Rect = VoltType.Deserialize<VoltRect2>(_rect);
        }

        #region Rect
        private VoltRect2 rect;
        public VoltRect2 Rect
        {
            get
            {
#if TOOLS
                if (Engine.EditorHint)
                    return VoltType.Deserialize<VoltRect2>(_rect);
                else
#endif
                    return rect;
            }
            set
            {
#if TOOLS
                if (Engine.EditorHint)
                    _rect = VoltType.Serialize(value);
                else
#endif
                    rect = value;
            }
        }
        [Export(PropertyHint.None, VoltPropertyHint.VoltRect2)]
        private byte[] _rect;
        #endregion
    }
}
