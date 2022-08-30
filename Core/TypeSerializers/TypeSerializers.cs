using FixMath.NET;
using Godot;

namespace Volatile.GodotEngine
{
    public class Fix64Serializer : TypeSerializer<Fix64>
    {
        public static readonly Fix64Serializer Global = new Fix64Serializer();

        protected override void InternalSerialize(StreamPeerBuffer buffer, Fix64 value) => buffer.PutFix64(value);
        protected override Fix64 InternalDeserialize(StreamPeerBuffer buffer) => buffer.GetFix64();

        public class ArraySerializer : Array<Fix64Serializer>
        {
            public static readonly ArraySerializer Global = new ArraySerializer();
        }
    }

    public class VoltVector2Serializer : TypeSerializer<VoltVector2>
    {
        public static readonly VoltVector2Serializer Global = new VoltVector2Serializer();

        protected override void InternalSerialize(StreamPeerBuffer buffer, VoltVector2 value) => buffer.PutVoltVector2(value);
        protected override VoltVector2 InternalDeserialize(StreamPeerBuffer buffer) => buffer.GetVoltVector2();

        public class ArraySerializer : Array<VoltVector2Serializer>
        {
            public static readonly ArraySerializer Global = new ArraySerializer();
        }
    }

    public class VoltTransform2DSerializer : TypeSerializer<VoltTransform2D>
    {
        public static readonly VoltTransform2DSerializer Global = new VoltTransform2DSerializer();

        protected override void InternalSerialize(StreamPeerBuffer buffer, VoltTransform2D value) => buffer.PutVoltTransform2D(value);
        protected override VoltTransform2D InternalDeserialize(StreamPeerBuffer buffer) => buffer.GetVoltTransform2D();

        public class ArraySerializer : Array<VoltTransform2DSerializer>
        {
            public static readonly ArraySerializer Global = new ArraySerializer();
        }
    }
}
