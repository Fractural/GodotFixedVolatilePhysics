using Godot;
using Volatile;
using System.Collections.Generic;
using Godot.Collections;
using Fractural.Utils;
using Fractural;
using FixMath.NET;
using System.Linq;

namespace Volatile.GodotEngine
{
    [Tool]
    public class VolatilePolygon : VolatileShape
    {
        public override VoltShape PrepareShape(VoltWorld world)
        {
            var globalPosition = GlobalFixedPosition;
            return world.CreatePolygonWorldSpace(
              Points.Select(x => x + globalPosition).ToArray(),
              Density,
              Friction,
              Restitution);
        }

        public override Vector2 ComputeTrueCenterOfMass()
        {
            float length = _Points.Length;
            Vector2 sum = Vector2.Zero;
            foreach (var point in _Points)
                sum += point;
            return new Vector2(sum.x / length, sum.y / length);
        }

        #region Points
        private VoltVector2[] points;
        public VoltVector2[] Points
        {
            get
            {
                if (Engine.EditorHint)
                    return GetPointsFromData();
                else
                    return points;
            }
            set
            {
                if (Engine.EditorHint)
                    SetPointsData(value);
                else
                    points = value;
            }
        }
        public byte[] pointsData = new byte[0];
        public VoltVector2[] GetPointsFromData()
        {
            var buffer = new StreamPeerBuffer();
            buffer.PutData(this.pointsData);
            buffer.Seek(0);
            var length = buffer.GetU16();
            VoltVector2[] initialFixedPoints = new VoltVector2[length];
            for (int i = 0; i < length; i++)
            {
                initialFixedPoints[i] = new VoltVector2(Fix64.FromRaw(buffer.Get64()), Fix64.FromRaw(buffer.Get64()));
            }
            return initialFixedPoints;
        }
        public void SetPointsData(IEnumerable<Vector2> array) => SetPointsData(array.Select(x => x.ToVoltVector2()));
        public void SetPointsData(IEnumerable<VoltVector2> array)
        {
            var buffer = new StreamPeerBuffer();
            buffer.PutU16((ushort)array.Count());
            foreach (VoltVector2 vector2 in array)
            {
                buffer.Put64(vector2.x.RawValue);
                buffer.Put64(vector2.y.RawValue);
            }
            pointsData = buffer.DataArray;
            Update();
        }
        public Vector2[] _Points
        {
            get => GetPointsFromData().Select(x => x.ToGDVector2()).ToArray();
            set => SetPointsData(value);
        }
        #endregion

        protected override void InitValues()
        {
            base.InitValues();
            Points = GetPointsFromData();
        }

        public override void _Draw()
        {
            if (!Engine.EditorHint) return;
            var points = _Points;
            if (points.Length > 0)
            {
                var color = Colors.DeepPink;
                var fill = color;
                fill.a = 0.075f;

                var previousPoint = points.Last();
                foreach (var point in points)
                {
                    DrawLine(previousPoint, point, color, 1, true);
                    previousPoint = point;
                }
                var polygonColors = new Color[points.Length];
                polygonColors.Populate(fill);
                DrawPolygon(points.ToArray(), polygonColors);
            }
        }

        public override Array _GetPropertyList()
        {
            var builder = new PropertyListBuilder(base._GetPropertyList());
            builder.AddItem(
                name: nameof(pointsData),
                type: Variant.Type.RawArray,
                hint: PropertyHint.None,
                usage: PropertyUsageFlags.Storage
            );
            builder.AddItem(
                name: nameof(_Points),
                type: Variant.Type.Vector2Array,
                hint: PropertyHint.None,
                usage: PropertyUsageFlags.Editor
            );
            return builder.Build();
        }
    }
}
