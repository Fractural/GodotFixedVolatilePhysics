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
            var points = new VoltVector2[]
            {
                new VoltVector2(globalPosition.x + Extents.x, globalPosition.y + Extents.y),
                new VoltVector2(globalPosition.x - Extents.x, globalPosition.y + Extents.y),
                new VoltVector2(globalPosition.x - Extents.x, globalPosition.y - Extents.y),
                new VoltVector2(globalPosition.x + Extents.x, globalPosition.y - Extents.y),
            };
            return world.CreatePolygonWorldSpace(
              points,
              Density,
              Friction,
              Restitution);
        }

        public override Vector2 ComputeGlobalCenterOfMass()
        {
            return GlobalPosition;
        }

        protected override void InitValues()
        {
            base.InitValues();
            Extents = VoltType.Deserialize<VoltVector2>(_size);
        }

        #region Rect
        private VoltVector2 size;
        public VoltVector2 Extents
        {
            get
            {
#if TOOLS
                if (Engine.EditorHint)
                    return VoltType.Deserialize<VoltVector2>(_size);
                else
#endif
                    return size;
            }
            set
            {
#if TOOLS
                if (Engine.EditorHint)
                    _size = VoltType.Serialize(value);
                else
#endif
                    size = value;
            }
        }
        [Export(PropertyHint.None, VoltPropertyHint.VoltVector2)]
        private byte[] _size = VoltType.Serialize(VoltVector2.One);
        #endregion
    }
}
