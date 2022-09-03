using FixMath.NET;
using Fractural;
using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;
using Volatile;
using Thread = System.Threading.Thread;

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
                var copy = FixedTransform;
                copy.Origin = value;
                FixedTransform = copy;
            }
        }
        private byte[] _FixedPosition
        {
            get => VoltType.Serialize(FixedPosition);
            set => FixedPosition = VoltType.DeserializeOrDefault<VoltVector2>(value);
        }

        private VoltVector2 fixedScale = VoltVector2.One;
        public VoltVector2 FixedScale
        {
            get => fixedScale;
            set
            {
                fixedScale = value;
                UpdateFixedTransformRotationAndScale();
            }
        }
        private byte[] _FixedScale
        {
            get => VoltType.Serialize(FixedScale);
            set => FixedScale = VoltType.DeserializeOrDefault<VoltVector2>(value);
        }

        private Fix64 fixedRotation = Fix64.Zero;
        public Fix64 FixedRotation
        {
            get => fixedRotation;
            set
            {
                fixedRotation = value;
                UpdateFixedTransformRotationAndScale();
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
                    return voltNode.GlobalFixedTransform * FixedTransform;
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

        public override void _Ready()
        {
            FixedTransform = VoltType.DeserializeOrDefault<VoltTransform2D>(_fixedTransform);
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
            if (Engine.EditorHint && !Transform.IsEqualApprox(previousTransform))
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

        private void UpdateFixedTransformRotationAndScale()
        {
            var copy = FixedTransform;
            copy.SetRotationAndScale(FixedRotation, fixedScale);
            FixedTransform = copy;
        }

        public void UpdateFixedTransform(VoltTransform2D transform, bool emitChanged = true)
        {
            FixedTransform = transform;

            // We don't want to trigger the FixedTransform updates,
            // so we set the backing fields instead
            fixedScale = transform.Scale;
            fixedRotation = transform.Rotation;
            if (emitChanged) FixedTransformChanged();
        }

        private void FixedTransformChanged()
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
