using Godot;
using Volatile;
using Godot.Collections;
using Fractural;
using FixMath.NET;
using System;

namespace Volatile.GodotEngine
{
    [Tool]
    public abstract class VolatileShape : VoltNode2D
    {
        public abstract VoltShape PrepareShape(VoltWorld world);
        public abstract Vector2 ComputeGlobalCenterOfMass();

        public override void _Ready()
        {
            base._Ready();
            Density = VoltType.DeserializeOrDefault<Fix64>(_density);
            Restitution = VoltType.DeserializeOrDefault<Fix64>(_restitution);
            Friction = VoltType.DeserializeOrDefault<Fix64>(_friction);
        }

        #region Density
        protected Fix64 density;
        public Fix64 Density
        {
            get
            {
#if TOOLS
                if (Engine.EditorHint)
                    return VoltType.DeserializeOrDefault<Fix64>(_density);
                else
#endif
                    return density;
            }
            set
            {
#if TOOLS
                if (Engine.EditorHint)
                    _density = VoltType.Serialize(value);
                else
#endif
                    density = value;
            }
        }
        [Export(hintString: VoltPropertyHint.Fix64)]
        private byte[] _density = VoltType.Serialize(VoltConfig.DEFAULT_DENSITY);
        #endregion

        #region Restitution
        protected Fix64 restitution;
        public Fix64 Restitution
        {
            get
            {
#if TOOLS
                if (Engine.EditorHint)
                    return VoltType.DeserializeOrDefault<Fix64>(_restitution);
                else
#endif
                    return restitution;
            }
            set
            {
#if TOOLS
                if (Engine.EditorHint)
                    _restitution = VoltType.Serialize(value);
                else
#endif
                    restitution = value;
            }
        }
        [Export(hintString: VoltPropertyHint.Fix64)]
        public byte[] _restitution = VoltType.Serialize(VoltConfig.DEFAULT_RESTITUTION);
        #endregion

        #region Friction
        protected Fix64 friction;
        public Fix64 Friction
        {
            get
            {
#if TOOLS
                if (Engine.EditorHint)
                    return VoltType.DeserializeOrDefault<Fix64>(_friction);
                else
#endif
                    return friction;
            }
            set
            {
#if TOOLS
                if (Engine.EditorHint)
                    _friction = VoltType.Serialize(value);
                else
#endif
                    friction = value;
            }
        }
        [Export(hintString: VoltPropertyHint.Fix64)]
        public byte[] _friction = VoltType.Serialize(VoltConfig.DEFAULT_FRICTION);
        #endregion

#if TOOLS
        [Signal]
        public delegate void EditingChanged(bool editing);

        private bool editing;
        [Export]
        public bool Editing
        {
            get => editing;
            set
            {
                editing = value;
                EmitSignal(nameof(EditingChanged), value);
            }
        }
#endif
    }
}
