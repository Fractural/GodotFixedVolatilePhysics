using Godot;
using Godot.Collections;
using Fractural;
using FixMath.NET;

namespace Volatile.GodotEngine
{
    [Tool]
    public class VolatileCircle : VolatileShape
    {
        public override VoltShape PrepareShape(VoltWorld world)
        {
            return world.CreateCircleWorldSpace(GlobalFixedPosition, Radius);
        }

        public override Vector2 ComputeGlobalCenterOfMass()
        {
            return GlobalPosition;
        }

        protected override void InitValues()
        {
            base.InitValues();
            Radius = VoltType.Deserialize<Fix64>(_radius);
        }

        #region Radius
        protected Fix64 radius;
        public Fix64 Radius
        {
            get
            {
#if TOOLS
                if (Engine.EditorHint)
                    return VoltType.Deserialize<Fix64>(_radius);
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
        [Export(hintString: VoltPropertyHint.Fix64)]
        private byte[] _radius = VoltType.Serialize(Fix64.From(1));
        #endregion
    }
}
