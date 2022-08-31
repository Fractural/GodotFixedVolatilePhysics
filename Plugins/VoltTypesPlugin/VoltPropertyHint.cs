namespace Volatile.GodotEngine
{
    public static class VoltPropertyHint
    {
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