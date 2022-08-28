using FixMath.NET;
using Godot;
using Volatile;

namespace Volatile.GodotEngine
{
    [Tool]
    // TODO: Finis hthis
    public class VoltNode2D : Node2D
    {
        public VoltTransform2D FixedTransform = new VoltTransform2D();
        public VoltVector2 FixedPosition
        {
            get => FixedTransform.Origin;
            set
            {
                FixedTransform.Origin = value;
                TransformChanged();

#if TOOLS
                if (Engine.EditorHint)
                {
                    updatingTransform = true;
                    Position = FixedTransform.Origin.ToGDVector2();
                    updatingTransform = false;
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
                {
                    updatingTransform = true;
                    Scale = FixedScale.ToGDVector2();
                    updatingTransform = false;
                }
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
                {
                    updatingTransform = true;
                    Rotation = (float)FixedRotation;
                    updatingTransform = false;
                }
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
                if (GetParent() is VoltNode2D voltNode)
                    FixedTransform.Origin = voltNode.GlobalFixedTransform.AffineInverse().XForm(value);
                else
                    FixedTransform.Origin = value;
                TransformChanged();
            }
        }

        public Fix64 GlobalFixedRotation
        {
            get => GlobalFixedTransform.Rotation;
            set
            {
                if (GetParent() is VoltNode2D voltNode)
                    FixedTransform.Rotation = value - voltNode.GlobalFixedTransform.Rotation;
                else
                    FixedTransform.Rotation = value;
                TransformChanged();
            }
        }
        
        private bool fixedTransformDirty = false;

#if TOOLS
        private bool updatingTransform = false;
#endif

        public override void _Ready()
        {
            SetNotifyTransform(true);
#if TOOLS
            updatingTransform = false;
#endif
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

#if TOOLS
        public override bool _Set(string property, object value)
        {
            if (!updatingTransform)
            {
                GD.Print("updating trans for prop: " + property);
                switch (property)
                {
                    case "position":
                        FixedTransform.Origin = Position.ToVoltVector2();
                        FixedPosition = FixedTransform.Origin;
                        break;
                    case "scale":
                        FixedScale = Scale.ToVoltVector2();
                        break;
                    case "rotation":
                        FixedRotation = (Fix64)Rotation;
                        break;
                        // Redundant, because position, scale, and rotation would trigger "transform" to update
                        //case "transform":
                        //    FixedTransform = Transform.ToVoltTransform2D();
                        //    break;
                }
            }
            return base._Set(property, value);
        }
#endif

        private void UpdateFloatTransform()
        {
            if (fixedTransformDirty)
            {
#if TOOLS
                updatingTransform = true;
#endif

                Transform2D floatTransform = new Transform2D();
                floatTransform.Rotation = (float)FixedRotation;
                floatTransform.Scale = FixedScale.ToGDVector2();
                Transform = floatTransform;

#if TOOLS
                updatingTransform = false;
#endif
                fixedTransformDirty = false;
            }
            // Not really sure why we would need to call GetGlobalTransform here
            //GetGlobalTransform();
        }

        private void UpdateFixedTransformRotationAndScale()
        {
            VoltTransform2D newTransform = new VoltTransform2D();
            newTransform.SetRotationAndScale(FixedRotation, fixedScale);
            FixedTransform.X = newTransform.X;
            FixedTransform.Y = newTransform.Y;
            TransformChanged();
            PropertyListChangedNotify();
        }

        public void UpdateFixedTransform(VoltTransform2D transform)
        {
            FixedTransform = transform;
            FixedScale = transform.Scale;
            FixedRotation = transform.Rotation;
            TransformChanged();
        }

        private void TransformChanged()
        {
            fixedTransformDirty = true;
        }
    }
}
