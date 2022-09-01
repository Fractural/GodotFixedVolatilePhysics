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
                    return VoltType.Deserialize<VoltTransform2D>(_fixedTransform);
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
            }
        }
        public byte[] _fixedTransform = VoltType.Serialize(VoltTransform2D.Default());
        #endregion

        public VoltVector2 FixedPosition
        {
            get => FixedTransform.Origin;
            set
            {
                var copy = FixedTransform;
                copy.Origin = value;
                fixedTransform = copy;

                TransformChanged();

#if TOOLS
                if (Engine.EditorHint)
                {
                    Position = copy.Origin.ToGDVector2();
                    FixedTransform = copy;
                }
#endif
            }
        }
        private Vector2 _fixedPosition
        {
            get => FixedPosition.ToGDVector2();
            set => FixedPosition = value.ToVoltVector2();
        }

        private VoltVector2 fixedScale = VoltVector2.One;
        public VoltVector2 FixedScale
        {
            get => fixedScale;
            set
            {
                fixedScale = value;
                UpdateFixedTransformRotationAndScale();

#if TOOLS
                if (Engine.EditorHint)
                    Scale = FixedScale.ToGDVector2();
#endif
            }
        }
        private Vector2 _fixedScale
        {
            get => FixedScale.ToGDVector2();
            set => FixedScale = value.ToVoltVector2();
        }

        private Fix64 fixedRotation = Fix64.Zero;
        public Fix64 FixedRotation
        {
            get => fixedRotation;
            set
            {
                fixedRotation = value;
                UpdateFixedTransformRotationAndScale();

#if TOOLS
                if (Engine.EditorHint)
                    Rotation = (float)FixedRotation;
#endif
            }
        }
        private float _fixedRotation
        {
            get => (float)FixedRotation;
            set => FixedRotation = (Fix64)value;
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
                TransformChanged();
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
                TransformChanged();
            }
        }

        private bool fixedTransformDirty = false;

        public override void _Ready()
        {
            if (!Engine.EditorHint)
            {
                FixedTransform = VoltType.Deserialize<VoltTransform2D>(_fixedTransform);
                fixedTransformDirty = true;

                UpdateFloatTransform();
            }

            previousTransform = Transform;
            SetNotifyTransform(true);
        }

        public override void _Notification(int what)
        {
            switch (what)
            {
                case NotificationTransformChanged:
                    UpdateFloatTransform();
                    break;
            }
        }

        private void UpdateFloatTransform()
        {
            if (fixedTransformDirty)
            {
                Transform = FixedTransform.ToGDTransform2D();
                PropertyListChangedNotify();

                fixedTransformDirty = false;
            }
        }

#if TOOLS
        private Transform2D previousTransform;
        private const float updateDelay = 0.5f;
        private float updateDelayTime = 0;
        private bool updateDelaying = false;
#endif
        public override void _Process(float delta)
        {
#if TOOLS
            if (Engine.EditorHint && !Transform.IsEqualApprox(previousTransform))
            {
                updateDelayTime = updateDelay;
                previousTransform = Transform;
                updateDelaying = true;
            }
            else
            {
                if (updateDelaying)
                {
                    if (updateDelayTime > 0)
                        updateDelayTime -= delta;
                    else if (updateDelayTime < 0)
                    {
                        UpdateFixedTransform(Transform.ToVoltTransform2D());
                        UpdateFloatTransform();
                        updateDelaying = false;
                    }
                }
            }
#endif
        }

        private void UpdateFixedTransformRotationAndScale()
        {
            var copy = FixedTransform;
            copy.SetRotationAndScale(FixedRotation, fixedScale);
            FixedTransform = copy;
            TransformChanged();
        }

        public void UpdateFixedTransform(VoltTransform2D transform)
        {
            FixedTransform = transform;

            // We don't want to trigger the FixedTransform updates,
            // so we set the backing fields instead
            fixedScale = transform.Scale;
            fixedRotation = transform.Rotation;
            TransformChanged();
        }

        private void TransformChanged()
        {
            fixedTransformDirty = true;
        }

        public override Array _GetPropertyList()
        {
            var builder = new PropertyListBuilder(base._GetPropertyList());
            builder.AddItem(
                name: "VoltNode2D",
                type: Variant.Type.Nil,
                hint: PropertyHint.None,
                usage: PropertyUsageFlags.Category
            );
            builder.AddItem(
                name: "Fixed Transform",
                type: Variant.Type.Nil,
                hint: PropertyHint.None,
                usage: PropertyUsageFlags.Group
            );
            builder.AddItem(
                name: nameof(_fixedTransform),
                type: Variant.Type.RawArray,
                hint: PropertyHint.None,
                hintString: VoltPropertyHint.VoltTransform2D,
                usage: PropertyUsageFlags.Default
            );
            builder.AddItem(
                name: nameof(_fixedPosition),
                type: Variant.Type.Vector2,
                hint: PropertyHint.None,
                usage: PropertyUsageFlags.Editor
            );
            builder.AddItem(
                name: nameof(_fixedScale),
                type: Variant.Type.Vector2,
                hint: PropertyHint.None,
                usage: PropertyUsageFlags.Editor
            );
            builder.AddItem(
                name: nameof(_fixedRotation),
                type: Variant.Type.Real,
                hint: PropertyHint.None,
                usage: PropertyUsageFlags.Editor
            );
            return builder.Build();
        }
    }
}
