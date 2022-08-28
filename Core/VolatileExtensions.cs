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
    }
}
