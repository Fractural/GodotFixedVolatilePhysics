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
            return world.CreatePolygonWorldSpace(
              Rect.Points.Select(x => x + globalPosition).ToArray(),
              Density,
              Friction,
              Restitution);
        }

        public override Vector2 ComputeTrueCenterOfMass()
        {
            return _Rect.GetCenter();
        }

        protected override void InitValues()
        {
            base.InitValues();
            Rect = GetRectFromData();
        }

        #region Rect
        private VoltRect2 rect;
        public VoltRect2 Rect
        {
            get
            {
                if (Engine.EditorHint)
                    return GetRectFromData();
                else
                    return rect;
            }
            set
            {
                if (Engine.EditorHint)
                    SetRectData(value);
                else
                    rect = value;
            }
        }
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
