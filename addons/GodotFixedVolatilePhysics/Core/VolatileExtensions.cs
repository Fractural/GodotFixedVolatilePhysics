using Godot;
using Volatile;
using FixMath.NET;

namespace GodotFixedVolatilePhysics
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
    }
}
