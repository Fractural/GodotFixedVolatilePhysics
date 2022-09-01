using FixMath.NET;
using System;
using System.Collections.Generic;

namespace Volatile.GodotEngine
{
    public static class VoltPropertyHint
    {
        static VoltPropertyHint()
        {
            HintToType = new Dictionary<string, Type>();
            HintToType.Add(Fix64, typeof(Fix64));
            HintToType.Add(VoltVector2, typeof(VoltVector2));
            HintToType.Add(VoltTransform2D, typeof(VoltTransform2D));
            HintToType.Add(VoltRect2, typeof(VoltRect2));
        }

        public static readonly Dictionary<string, Type> HintToType;

        public const string Array = nameof(Array);
        public const string Fix64 = nameof(Fix64);
        public const string VoltVector2 = nameof(VoltVector2);
        public const string VoltTransform2D = nameof(VoltTransform2D);
        public const string VoltRect2 = nameof(VoltRect2);

        public static readonly string[] Values =
        {
            Array,
            Fix64,
            VoltVector2,
            VoltTransform2D,
            VoltRect2
        };
    }
}