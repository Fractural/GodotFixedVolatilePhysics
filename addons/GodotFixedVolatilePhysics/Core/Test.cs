using FixMath.NET;
using Godot;

namespace Volatile.GodotEngine
{
    public class Test : Node2D
    {
        [Export(hintString: VoltPropertyHint.Fix64)]
        public byte[] fix64Number = VoltType.Serialize(Fix64.From("3384.4390"));
        [Export(hintString: VoltPropertyHint.VoltVector2)]
        public byte[] fixedVector = VoltType.Serialize(VoltVector2.From("3.430", "-234.03999"));
        [Export(hintString: VoltPropertyHint.Array + "," + VoltPropertyHint.Fix64)]
        public byte[] fixed64Array = VoltType.Serialize(new[]
        {
            Fix64.From("303.43003"),
            Fix64.From("40.34939"),
            Fix64.From("9348.2340"),
            Fix64.From("23.34029"),
        });
        [Export(hintString: VoltPropertyHint.Array + "," + VoltPropertyHint.VoltVector2)]
        public byte[] fixedVectorArray = VoltType.Serialize(new[]
        {
            VoltVector2.From("303.43003", "230.3409"),
            VoltVector2.From("40.34939", "9348.2340"),
            VoltVector2.From("23.34029", "24.34038"),
        });
        [Export]
        public byte[] regularByteArray = new byte[0];
        [Export(hintString: VoltPropertyHint.Array + "," + VoltPropertyHint.VoltRect2)]
        public byte[] voltRectArray = VoltType.Serialize(new VoltRect2[] { });
        [Export(hintString: VoltPropertyHint.VoltRect2)]
        public byte[] voltRect = VoltType.Serialize(new VoltRect2());
        [Export]
        public string tests;
        [Export]
        public Rect2 rect;
    }
}
