using FixMath.NET;
using Fractural;
using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;
using Volatile;

namespace Volatile.GodotEngine
{
    [Tool]
    // TODO: Finis hthis
    public class VoltNode2D : Node2D
    {
        #region FixedTransform
        private VoltTransform2D fixedTransform;
        public VoltTransform2D FixedTransform
        {
            get
            {
                if (Engine.EditorHint)
                    return GetFixedTransformFromData();
                else
                    return fixedTransform;
            }
            set
            {
                if (Engine.EditorHint)
                    SetFixedTransformData(value);
                else
                    fixedTransform = value;
            }
        }
        public byte[] fixedTransformData = new byte[0];
        public VoltTransform2D GetFixedTransformFromData()
        {
            // Initialize the data if it's empty
            if (fixedTransformData.Length == 0)
            {
                SetFixedTransformData(VoltTransform2D.Default());
                PropertyListChangedNotify();
            }
            var buffer = new StreamPeerBuffer();
            buffer.PutData(this.fixedTransformData);
            buffer.Seek(0);
            return buffer.GetVoltTransform2D();
        }
        public void SetFixedTransformData(VoltTransform2D transform)
        {
            var buffer = new StreamPeerBuffer();
            buffer.PutVoltTransform2D(transform);
            fixedTransformData = buffer.DataArray;
        }
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
                    Position = FixedTransform.Origin.ToGDVector2();
                    SetFixedTransformData(FixedTransform);
                }
#endif
            }
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
            FixedTransform = GetFixedTransformFromData();
            fixedTransformDirty = true;

            UpdateFloatTransform();

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
                case NotificationEnterTree:
                    GetGlobalTransform();
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

        private Transform2D previousTransform;

        public override void _Process(float delta)
        {
            if (Engine.EditorHint && !Transform.IsEqualApprox(previousTransform))
            {
                UpdateFixedTransform(Transform.ToVoltTransform2D());
                UpdateFloatTransform();
                previousTransform = Transform;
            }
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
                name: nameof(fixedTransformData),
                type: Variant.Type.RawArray,
                hint: PropertyHint.None,
                usage: PropertyUsageFlags.Storage
            );
            return builder.Build();
        }
    }
}
