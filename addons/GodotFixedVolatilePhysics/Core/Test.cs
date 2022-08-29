using FixMath.NET;
using Godot;
using Volatile.GodotEngine.Plugin;

namespace Volatile.GodotEngine
{
    public class Test : Node2D
    {
        [Export(PropertyHint.None, VoltPropertyHint.Fix64 + ",403")]
        public byte[] fix64Number = new byte[0];
        [Export]
        public float oof;
        [Export]
        public string tests;
    }
}
