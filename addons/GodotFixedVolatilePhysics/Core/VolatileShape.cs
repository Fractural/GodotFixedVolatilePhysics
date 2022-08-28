using Godot;
using Volatile;
using Godot.Collections;
using Fractural;
using FixMath.NET;

namespace Volatile.GodotEngine
{
    [Tool]
    public abstract class VolatileShape : Node2D
    {
        public abstract VoltShape PrepareShape(VoltWorld world);
        public abstract Vector2 ComputeTrueCenterOfMass();

        public override void _Ready()
        {
            if (Engine.EditorHint)
            {
                Update();
                return;
            }
            InitValues();
        }

        protected virtual void InitValues()
        {
            Density = GetDensityFromData();
        }

        #region Density
        protected Fix64 density;
        public Fix64 Density
        {
            get
            {
                if (Engine.EditorHint)
                    return GetDensityFromData();
                else
                    return density;
            }
            set
            {
                if (Engine.EditorHint)
                    SetDensityData(value);
                else
                    density = value;
            }
        }
        public byte[] densityData = new byte[0];
        public Fix64 GetDensityFromData()
        {
            var buffer = new StreamPeerBuffer();
            buffer.PutData(densityData);
            buffer.Seek(0);
            return Fix64.FromRaw(buffer.Get64());
        }
        public void SetDensityData(float density) => SetDensityData(density);
        public void SetDensityData(Fix64 density)
        {
            var buffer = new StreamPeerBuffer();
            buffer.Put64(density.RawValue);
        }
        public float _Density
        {
            get => (float)GetDensityFromData();
            set => SetDensityData(value);
        }
        #endregion

        #region Restitution
        protected Fix64 restitution;
        public Fix64 Restitution
        {
            get
            {
                if (Engine.EditorHint)
                    return GetRestitutionFromData();
                else
                    return restitution;
            }
            set
            {
                if (Engine.EditorHint)
                    SetRestitutionData(value);
                else
                    restitution = value;
            }
        }
        public byte[] restitutionData = new byte[0];
        public Fix64 GetRestitutionFromData()
        {
            var buffer = new StreamPeerBuffer();
            buffer.PutData(restitutionData);
            buffer.Seek(0);
            return Fix64.FromRaw(buffer.Get64());
        }
        public void SetRestitutionData(float restitution) => SetRestitutionData(restitution);
        public void SetRestitutionData(Fix64 restitution)
        {
            var buffer = new StreamPeerBuffer();
            buffer.Put64(restitution.RawValue);
        }
        public float _Restitution
        {
            get => (float)GetRestitutionFromData();
            set => SetRestitutionData(value);
        }
        #endregion

        #region Friction
        protected Fix64 friction;
        public Fix64 Friction
        {
            get
            {
                if (Engine.EditorHint)
                    return GetFrictionFromData();
                else
                    return friction;
            }
            set
            {
                if (Engine.EditorHint)
                    SetFrictionData(value);
                else
                    friction = value;
            }
        }
        public byte[] frictionData = new byte[0];
        public Fix64 GetFrictionFromData()
        {
            var buffer = new StreamPeerBuffer();
            buffer.PutData(frictionData);
            buffer.Seek(0);
            return Fix64.FromRaw(buffer.Get64());
        }
        public void SetFrictionData(float friction) => SetFrictionData(friction);
        public void SetFrictionData(Fix64 friction)
        {
            var buffer = new StreamPeerBuffer();
            buffer.Put64(friction.RawValue);
        }
        public float _Friction
        {
            get => (float)GetFrictionFromData();
            set => SetFrictionData(value);
        }
        #endregion

        public override Godot.Collections.Array _GetPropertyList()
        {
            var builder = new PropertyListBuilder();
            builder.AddItem(
                name: nameof(densityData),
                type: Variant.Type.RawArray,
                hint: PropertyHint.None,
                usage: PropertyUsageFlags.Storage
            );
            builder.AddItem(
                name: nameof(_Density),
                type: Variant.Type.Real,
                hint: PropertyHint.None,
                usage: PropertyUsageFlags.Editor
            );
            builder.AddItem(
                name: nameof(restitutionData),
                type: Variant.Type.RawArray,
                hint: PropertyHint.None,
                usage: PropertyUsageFlags.Storage
            );
            builder.AddItem(
                name: nameof(_Restitution),
                type: Variant.Type.Real,
                hint: PropertyHint.None,
                usage: PropertyUsageFlags.Editor
            );
            builder.AddItem(
                name: nameof(frictionData),
                type: Variant.Type.RawArray,
                hint: PropertyHint.None,
                usage: PropertyUsageFlags.Storage
            );
            builder.AddItem(
                name: nameof(_Friction),
                type: Variant.Type.Real,
                hint: PropertyHint.None,
                usage: PropertyUsageFlags.Editor
            );
            return builder.Build();
        }
    }
}
