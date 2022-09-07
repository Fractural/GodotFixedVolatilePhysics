using Godot;
using Volatile;
using Godot.Collections;
using Fractural;
using FixMath.NET;
using System;
#if TOOLS
using Volatile.GodotEngine.Plugin;
#endif

namespace Volatile.GodotEngine
{
    [Tool]
    public abstract class VolatileShape : VoltNode2D
    {
        public abstract VoltShape PrepareShape(VoltWorld world);
        public abstract Vector2 ComputeLocalCenterOfMass();

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

        [Export]
        public bool DebugDraw { get; set; } = false;

        public Color GetShapeDrawColor()
        {
            Color color = Palette.DynamicBody;
            var parent = GetParent();
            if (parent != null)
            {
                if (parent is VolatileStaticBody)
                    color = Palette.StaticBody;
                else if (parent is VolatileKinematicBody)
                    color = Palette.KinematicBody;
                else if (parent is VolatileArea)
                    color = Palette.AreaBody;
            }
            return color;
        }

        public override void _Draw()
        {
            base._Draw();
            if (!DebugDraw && !Engine.EditorHint) return;

            var color = Palette.Accent;
            var localCenterOfMass = ComputeLocalCenterOfMass();
            var innerRadius = 1;
            var outerRadius = 1.1f;
            DrawLine(localCenterOfMass - new Vector2(innerRadius, 0), localCenterOfMass + new Vector2(innerRadius, 0), color);
            DrawLine(localCenterOfMass - new Vector2(0, innerRadius), localCenterOfMass + new Vector2(0, innerRadius), color);
            DrawArc(localCenterOfMass, innerRadius, 0, 2 * Mathf.Pi, 60, color);
            DrawArc(localCenterOfMass, outerRadius, 0, 2 * Mathf.Pi, 60, color);
        }
    }
}
