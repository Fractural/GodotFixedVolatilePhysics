using Godot;
using Volatile;
using FixMath.NET;

namespace Volatile.GodotEngine
{
    public static class VolatileExtensions
    {
        public static Vector2 ToGDVector2(this VoltVector2 v)
        {
            return new Vector2((float)v.x, (float)v.y);
        }

        public static VoltVector2 ToVoltVector2(this Vector2 v)
        {
            return new VoltVector2((Fix64)v.x, (Fix64)v.y);
        }

        public static Rect2 ToGDRect2(this VoltRect2 rect)
        {
            return new Rect2(rect.Position.ToGDVector2(), rect.Size.ToGDVector2());
        }

        public static VoltRect2 ToVoltRect2(this Rect2 rect)
        {
            return new VoltRect2(rect.Position.ToVoltVector2(), rect.Size.ToVoltVector2());
        }

        public static VoltTransform2D ToVoltTransform2D(this Transform2D transform)
        {
            return new VoltTransform2D(transform.x.ToVoltVector2(), transform.y.ToVoltVector2(), transform.origin.ToVoltVector2());
        }

        public static Transform2D ToGDTransform2D(this VoltTransform2D transform)
        {
            return new Transform2D(transform.X.ToGDVector2(), transform.Y.ToGDVector2(), transform.Origin.ToGDVector2());
        }

        public static void PutFix64(this StreamPeerBuffer buffer, Fix64 fix64)
        {
            buffer.Put64(fix64.RawValue);
        }

        public static Fix64 GetFix64(this StreamPeerBuffer buffer)
        {
            return Fix64.FromRaw(buffer.Get64());
        }

        public static void PutVoltVector2(this StreamPeerBuffer buffer, VoltVector2 vector)
        {
            buffer.PutFix64(vector.x);
            buffer.PutFix64(vector.y);
        }

        public static VoltVector2 GetVoltVector2(this StreamPeerBuffer buffer)
        {
            return new VoltVector2(buffer.GetFix64(), buffer.GetFix64());
        }

        public static void PutVoltTransform2D(this StreamPeerBuffer buffer, VoltTransform2D transform)
        {
            buffer.PutVoltVector2(transform.X);
            buffer.PutVoltVector2(transform.Y);
            buffer.PutVoltVector2(transform.Origin);
        }

        public static VoltTransform2D GetVoltTransform2D(this StreamPeerBuffer buffer)
        {
            return new VoltTransform2D(
                buffer.GetVoltVector2(),
                buffer.GetVoltVector2(),
                buffer.GetVoltVector2()
            );
        }

        public static void PutVoltRect2(this StreamPeerBuffer buffer, VoltRect2 rect)
        {
            buffer.PutVoltVector2(rect.Position);
            buffer.PutVoltVector2(rect.Size);
        }

        public static VoltRect2 GetVoltRect2(this StreamPeerBuffer buffer)
        {
            return new VoltRect2(buffer.GetVoltVector2(), buffer.GetVoltVector2());
        }

        public static void PutVoltMatrix(this StreamPeerBuffer buffer, VoltMatrix matrix)
        {
            buffer.Put32(matrix.Rows);
            buffer.Put32(matrix.Columns);
            for (int row = 0; row < matrix.Rows; row++)
                for (int col = 0; col < matrix.Columns; col++)
                    buffer.PutFix64(matrix[row, col]);
        }

        public static VoltMatrix GetVoltMatrix(this StreamPeerBuffer buffer)
        {
            var matrix = new VoltMatrix(buffer.Get32(), buffer.Get32());
            for (int row = 0; row < matrix.Rows; row++)
                for (int col = 0; col < matrix.Columns; col++)
                    matrix[row, col] = buffer.GetFix64();
            return matrix;
        }
    }
}
