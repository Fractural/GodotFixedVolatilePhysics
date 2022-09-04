using FixMath.NET;
using System.Linq;

namespace Volatile.GodotEngine
{
    public static class Utils
    {
        public static Fix64 SignedArea(this VoltVector2[] points)
        {
            Fix64 sum = Fix64.Zero;

            for (int i = 0; i < points.Length; i++)
            {
                VoltVector2 v = points[i];
                VoltVector2 u = points[(i + 1) % points.Length];
                VoltVector2 w = points[(i + 2) % points.Length];

                sum += u.x * (v.y - w.y);
            }

            return sum / (Fix64)2;
        }
    }
}
