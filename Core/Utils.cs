using FixMath.NET;
using System.Linq;

namespace Volatile.GodotEngine
{
    public static class Utils
    {
        public static Fix64 SignedArea(this VoltVector2[] points)
        {
            Fix64 signedArea = Fix64.Zero;
            var previousPoint = points.Last();
            foreach (var point in points)
                signedArea += (previousPoint.x * point.y - point.x * previousPoint.y);

            return signedArea / Fix64.From(2);
        }
    }
}
