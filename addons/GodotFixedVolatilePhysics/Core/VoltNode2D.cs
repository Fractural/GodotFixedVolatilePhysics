using FixMath.NET;
using Fractural;
using Godot;
using Godot.Collections;

namespace Volatile.GodotEngine
{
    [Tool]
    public class VoltNode2D : Node2D
    {
        #region FixedTransform
        private VoltTransform2D fixedTransform;
        public VoltTransform2D FixedTransform
        {
            get
            {
#if TOOLS
                if (Engine.EditorHint)
                    return VoltType.DeserializeOrDefault<VoltTransform2D>(_fixedTransform);
                else
#endif
                    return fixedTransform;
            }
            set
            {
#if TOOLS
                if (Engine.EditorHint)
                    _fixedTransform = VoltType.Serialize(value);
                else
#endif
                    fixedTransform = value;
                FixedTransformChanged();
            }
        }
        public byte[] _fixedTransform = VoltType.Serialize(VoltTransform2D.Default());
        public VoltTransform2D _OnFixedTransformSet
        {
            set
            {
                FixedTransformChanged();
            }
        }
        #endregion

        public VoltVector2 FixedPosition
        {
            get => FixedTransform.Origin;
            set
            {
#if TOOLS
                if (Engine.EditorHint)
                {
                    var copy = FixedTransform;
                    copy.Origin = value;
                    FixedTransform = copy;
                }
                else
#endif
                {
                    fixedTransform.Origin = value;
                    FixedTransformChanged();
                }
            }
        }
        private byte[] _FixedPosition
        {
            get => VoltType.Serialize(FixedPosition);
            set => FixedPosition = VoltType.DeserializeOrDefault<VoltVector2>(value);
        }

        public VoltVector2 FixedScale
        {
            get => FixedTransform.Scale;
            set
            {
#if TOOLS
                if (Engine.EditorHint)
                {
                    var copy = FixedTransform;
                    copy.Scale = value;
                    FixedTransform = copy;
                }
                else
#endif
                {
                    fixedTransform.Scale = value;
                    FixedTransformChanged();
                }
            }
        }
        private byte[] _FixedScale
        {
            get => VoltType.Serialize(FixedScale);
            set => FixedScale = VoltType.DeserializeOrDefault<VoltVector2>(value);
        }

        public Fix64 FixedRotation
        {
            get => FixedTransform.Rotation;
            set
            {
#if TOOLS
                if (Engine.EditorHint)
                {
                    var copy = FixedTransform;
                    copy.Rotation = value;
                    FixedTransform = copy;
                }
                else
#endif
                {
                    fixedTransform.Rotation = value;
                    FixedTransformChanged();
                }
            }
        }
        private byte[] _FixedRotation
        {
            get => VoltType.Serialize(FixedRotation);
            set => FixedRotation = VoltType.DeserializeOrDefault<Fix64>(value);
        }

        public VoltTransform2D GlobalFixedTransform
        {
            get
            {
                if (GetParent() is VoltNode2D voltNode)
                {
                    return voltNode.GlobalFixedTransform * FixedTransform;
                }
                return FixedTransform;
            }
            set
            {
                if (GetParent() is VoltNode2D voltNode)
                    UpdateFixedTransform(voltNode.GlobalFixedTransform.AffineInverse() * value);
                else
                    UpdateFixedTransform(value);
            }
        }

        public VoltVector2 GlobalFixedPosition
        {
            get => GlobalFixedTransform.Origin;
            set
            {
                var copy = FixedTransform;
                if (GetParent() is VoltNode2D voltNode)
                    copy.Origin = voltNode.GlobalFixedTransform.AffineInverse().XForm(value);
                else
                    copy.Origin = value;
                FixedTransform = copy;
                FixedTransformChanged();
            }
        }

        public Fix64 GlobalFixedRotation
        {
            get => GlobalFixedTransform.Rotation;
            set
            {
                var copy = FixedTransform;
                if (GetParent() is VoltNode2D voltNode)
                    copy.Rotation = value - voltNode.GlobalFixedTransform.Rotation;
                else
                    copy.Rotation = value;
                FixedTransform = copy;
                FixedTransformChanged();
            }
        }

        public override void _EnterTree()
        {
            // We want parent transforms to be initialized first, which is why we load it in
            // _EnterTree.
            FixedTransform = VoltType.DeserializeOrDefault<VoltTransform2D>(_fixedTransform);
        }

        public override void _Ready()
        {
#if TOOLS
            if (Engine.EditorHint)
                previousTransform = Transform;
            else
#endif
            {
                UpdateFloatTransform();
            }
        }

        private void UpdateFloatTransform()
        {
            Transform = FixedTransform.ToGDTransform2D();

            PropertyListChangedNotify();
        }

#if TOOLS
        private Transform2D previousTransform;
        private bool updating = false;
        private float updateDelay = 0.1f;
        private float updateTime;
#endif
        public override void _Process(float delta)
        {
#if TOOLS
            if (!Engine.EditorHint) return;
            if (!Transform.IsEqualApprox(previousTransform))
            {
                // We defer updates, to avoid making several updates when the
                // user is dragging the transform around
                updateTime = updateDelay;
                previousTransform = Transform;
                updating = true;
            }
            else if (updating)
            {
                updateTime -= delta;
                if (updateTime <= 0)
                {
                    // Fixed transform changes immediately propagate to the float transform (see TransformChanged())
                    // Therefore we know that the float transform is the cause of the difference here (due to the user
                    // dragging/rotating/scaling the node2D using Godot's editor tools),

                    // We don't want to emit changed here, because we're using data from the Transform
                    // to update the FixedTransform, and not the other way around. Otherwise
                    // you'd trigger a double update, where Transform gets updated, you update FixedTransform,
                    // which then causes Transform to get reupdated.
                    UpdateFixedTransform(Transform.ToVoltTransform2D(), false);
                    PropertyListChangedNotify();
                    updating = false;
                }
            }
#endif
        }

        public void UpdateFixedTransform(VoltTransform2D transform, bool emitChanged = true)
        {
            FixedTransform = transform;
            if (emitChanged) FixedTransformChanged();
        }

        protected virtual void FixedTransformChanged()
        {
            // Update float transform immediately when the fixed transform changed.
            UpdateFloatTransform();
        }

        public override Array _GetPropertyList()
        {
            var builder = new PropertyListBuilder(base._GetPropertyList());
            builder.AddItem(
                name: "VoltNode2D",
                type: Variant.Type.Nil,
                usage: PropertyUsageFlags.Category
            );
            builder.AddItem(
                name: "Fixed Transform",
                type: Variant.Type.Nil,
                usage: PropertyUsageFlags.Group
            );
            builder.AddItem(
                name: nameof(_fixedTransform),
                type: Variant.Type.RawArray,
                hintString: VoltPropertyHint.VoltTransform2D + ",set:" + nameof(_OnFixedTransformSet),
                usage: PropertyUsageFlags.Default
            );
            builder.AddItem(
                name: nameof(_FixedPosition),
                type: Variant.Type.RawArray,
                hintString: VoltPropertyHint.VoltVector2,
                usage: PropertyUsageFlags.Editor
            );
            builder.AddItem(
                name: nameof(_FixedScale),
                type: Variant.Type.RawArray,
                hintString: VoltPropertyHint.VoltVector2,
                usage: PropertyUsageFlags.Editor
            );
            builder.AddItem(
                name: nameof(_FixedRotation),
                type: Variant.Type.RawArray,
                hintString: VoltPropertyHint.Fix64,
                usage: PropertyUsageFlags.Editor
            );
            return builder.Build();
        }
    }
}
