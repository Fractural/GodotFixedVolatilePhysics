using Godot;
using Volatile;
using Godot.Collections;
using Fractural;
using FixMath.NET;

namespace GodotFixedVolatilePhysics
{
    [Tool]
    public class VolatileCircle : VolatileShape
    {
        public override VoltShape PrepareShape(VoltWorld world)
        {
            throw new System.NotImplementedException();
        }

        public override Vector2 ComputeTrueCenterOfMass()
        {
            throw new System.NotImplementedException();
        }

        #region Radius
        protected Fix64 radius;
        public Fix64 Radius
        {
            get
            {
                if (Engine.EditorHint)
                    return GetRadiusFromData();
                else
                    return radius;
            }
            set
            {
                if (Engine.EditorHint)
                    SetRadiusData(value);
                else
                    radius = value;
            }
        }
        public byte[] radiusData = new byte[0];
        public Fix64 GetRadiusFromData()
        {
            var buffer = new StreamPeerBuffer();
            buffer.PutData(radiusData);
            buffer.Seek(0);
            return Fix64.FromRaw(buffer.Get64());
        }
        public void SetRadiusData(float radius) => SetRadiusData(radius);
        public void SetRadiusData(Fix64 radius)
        {
            var buffer = new StreamPeerBuffer();
            buffer.Put64(radius.RawValue);
        }
        public float _Radius
        {
            get => (float)GetRadiusFromData();
            set => SetRadiusData(value);
        }
        #endregion

        public override Array _GetPropertyList()
        {
            var builder = new PropertyListBuilder(base._GetPropertyList());
            builder.AddItem(
                name: nameof(radiusData),
                type: Variant.Type.RawArray,
                hint: PropertyHint.None,
                usage: PropertyUsageFlags.Storage
            );
            builder.AddItem(
                name: nameof(_Radius),
                type: Variant.Type.Real,
                hint: PropertyHint.None,
                usage: PropertyUsageFlags.Editor
            );
            return builder.Build();
        }
    }

    [Tool]
    public class VolatileRect : VolatileShape
    {
        public override VoltShape PrepareShape(VoltWorld world)
        {
            return world.CreatePolygonWorldSpace(
              Rect.Points,
              Density,
              Friction,
              Restitution);
        }

        public override Vector2 ComputeTrueCenterOfMass()
        {
            return _Rect.GetCenter();
        }

        #region Rect
        public VoltRect2 Rect;
        private byte[] rectData;
        public VoltRect2 GetRectFromData()
        {
            var buffer = new StreamPeerBuffer();
            buffer.PutData(rectData);
            buffer.Seek(0);
            var center = new VoltVector2(Fix64.FromRaw(buffer.Get64()), Fix64.FromRaw(buffer.Get64()));
            var size = new VoltVector2(Fix64.FromRaw(buffer.Get64()), Fix64.FromRaw(buffer.Get64()));
            return new VoltRect2(center, size);
        }
        public void SetRectData(Rect2 rect) => SetRectData(new VoltRect2(rect.Position.ToVoltVector2(), rect.Size.ToVoltVector2()));
        public void SetRectData(VoltRect2 rect)
        {
            var buffer = new StreamPeerBuffer();
            buffer.Put64(rect.Position.x.RawValue);
            buffer.Put64(rect.Position.y.RawValue);
            buffer.Put64(rect.Size.x.RawValue);
            buffer.Put64(rect.Size.y.RawValue);
            rectData = buffer.DataArray;
        }
        public Rect2 _Rect
        {
            get => GetRectFromData().ToGDRect2();
            set => SetRectData(value);
        }
        #endregion

        public override Array _GetPropertyList()
        {
            var builder = new PropertyListBuilder(base._GetPropertyList());
            builder.AddItem(
                name: nameof(_Rect),
                type: Variant.Type.Rect2,
                hint: PropertyHint.None,
                usage: PropertyUsageFlags.Editor
            );
            builder.AddItem(
                name: nameof(rectData),
                type: Variant.Type.RawArray,
                hint: PropertyHint.None,
                usage: PropertyUsageFlags.Storage
            );
            return builder.Build();
        }
    }
}
