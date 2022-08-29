using FixMath.NET;
using Godot;
using Volatile.GodotEngine.Plugin;

namespace Volatile.GodotEngine
{
    public class Test : Node2D
    {
        [Export(PropertyHint.None, VoltPropertyHint.Fix64)]
        public byte[] fix64Number = Fix64Serializer.Global.Serialize(Fix64.From("3384.4390"));
        [Export]
        public float oof;
        [Export]
        public string tests;
    }
}
