using FixMath.NET;
using Godot;

namespace Volatile.GodotEngine
{
    public class Fix64Boxed : Boxed<Fix64>
    {
        public Fix64Boxed(byte[] data) : base(data) { }
        public Fix64Boxed() : base() { }

        protected override Fix64 InternalGetValueFromData(StreamPeerBuffer buffer)
        {
            return buffer.GetFix64();
        }

        protected override void InternalSetValueFromData(StreamPeerBuffer buffer, Fix64 value)
        {
            buffer.PutFix64(value);
        }
    }
}
