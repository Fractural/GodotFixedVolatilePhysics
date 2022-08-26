using Godot;
using Volatile;
using System.Collections.Generic;
using Godot.Collections;
using Fractural.Utils;
using Fractural;
using FixMath.NET;
using System.Linq;
using GDDictionary = Godot.Collections.Dictionary;

namespace GodotFixedVolatilePhysics
{
    [Tool]
    public class VolatileShape : Node2D
    {
        public List<VoltVector2> FixedPoints { get; private set; }
        public byte[] initialFixedPoints = new byte[0];
        public VoltVector2[] GetInitialFixedPoints()
        {
            var buffer = new StreamPeerBuffer();
            buffer.PutData(this.initialFixedPoints);
            buffer.Seek(0);
            var length = buffer.GetU16();
            VoltVector2[] initialFixedPoints = new VoltVector2[length];
            for (int i = 0; i < length; i++)
            {
                initialFixedPoints[i] = new VoltVector2(Fix64.FromRaw(buffer.Get64()), Fix64.FromRaw(buffer.Get64()));
            }
            return initialFixedPoints;
        }
        public void SetInitialFixedPoints(IEnumerable<VoltVector2> array)
        {
            var buffer = new StreamPeerBuffer();
            buffer.PutU16((ushort)array.Count());
            foreach (VoltVector2 vector2 in array)
            {
                buffer.Put64(vector2.x.RawValue);
                buffer.Put64(vector2.y.RawValue);
            }
            initialFixedPoints = buffer.DataArray;
            Update();
        }
        public void SetInitialFixedPoints(IEnumerable<Vector2> array)
        {
            SetInitialFixedPoints(array.Select(x => x.ToVoltVector2()));
        }

        public override void _Ready()
        {
            if (Engine.EditorHint)
            {
                Update();
                return;
            }


            FixedPoints = new List<VoltVector2>(GetInitialFixedPoints());
        }

        public override void _Draw()
        {
            if (!Engine.EditorHint) return;
            var points = GetInitialFixedPoints().Select(x => x.ToGDVector2());
            var count = points.Count();
            if (count > 0)
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
                var polygonColors = new Color[count];
                polygonColors.Populate(fill);
                DrawPolygon(points.ToArray(), polygonColors);
            }
        }

        #region PropertyList
        public Vector2[] _Points
        {
            get => GetInitialFixedPoints().Select(x => x.ToGDVector2()).ToArray();
            set => SetInitialFixedPoints(value);
        }

        public override Array _GetPropertyList()
        {
            var builder = new PropertyListBuilder();
            builder.AddItem(
                name: nameof(initialFixedPoints),
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
        #endregion
    }
}
