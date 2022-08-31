using FixMath.NET;
using Volatile;

namespace Volatile.GodotEngine
{
    public struct VoltRect2
    {
        public VoltRect2(VoltVector2 position, VoltVector2 size)
        {
            Size = size;
            Position = position;
        }

        public VoltVector2 Size { get; set; }
        public VoltVector2 Position { get; set; }

        public VoltVector2 TopLeft => Position;
        public VoltVector2 TopRight => new VoltVector2(Position.x + Size.x, Position.y);
        public VoltVector2 BottomRight => new VoltVector2(Position.x + Size.x, Position.y + Size.y);
        public VoltVector2 BottomLeft => new VoltVector2(Position.x, Position.y + Size.y);

        public VoltVector2[] Points => new VoltVector2[]
        {
            TopLeft,
            TopRight,
            BottomRight,
            BottomLeft
        };

        private static readonly Fix64 FIX_2 = Fix64.From(2);

        public VoltVector2 GetCenter()
        {
            return Position + Size / FIX_2;
        }
    }
}
