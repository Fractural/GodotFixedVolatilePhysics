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

        public override Vector2 ComputeTrueCenterOfMass()
        {
            return Position;
        }

        protected override void InitValues()
        {
            base.InitValues();
            Radius = GetRadiusFromData();
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
            if (radiusData.Length == 0)
                SetRadiusData(10);
            var buffer = new StreamPeerBuffer();
            buffer.PutData(radiusData);
            buffer.Seek(0);
            return buffer.GetFix64();
        }
        public void SetRadiusData(float radius) => SetRadiusData(radius);
        public void SetRadiusData(Fix64 radius)
        {
            var buffer = new StreamPeerBuffer();
            buffer.PutFix64(radius);
            radiusData = buffer.DataArray;
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
}
