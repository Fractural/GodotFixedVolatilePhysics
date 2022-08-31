using FixMath.NET;
using Godot;

namespace Volatile.GodotEngine
{
    public class Fix64Serializer : TypeSerializer<Fix64>
    {
        public static readonly Fix64Serializer Global = new Fix64Serializer();

        public override void Serialize(StreamPeerBuffer buffer, Fix64 value) => buffer.PutFix64(value);
        public override Fix64 Deserialize(StreamPeerBuffer buffer) => buffer.GetFix64();
    }

    public class VoltVector2Serializer : TypeSerializer<VoltVector2>
    {
        public static readonly VoltVector2Serializer Global = new VoltVector2Serializer();

        public override void Serialize(StreamPeerBuffer buffer, VoltVector2 value) => buffer.PutVoltVector2(value);
        public override VoltVector2 Deserialize(StreamPeerBuffer buffer) => buffer.GetVoltVector2();
    }

    public class VoltTransform2DSerializer : TypeSerializer<VoltTransform2D>
    {
        public static readonly VoltTransform2DSerializer Global = new VoltTransform2DSerializer();

        public override void Serialize(StreamPeerBuffer buffer, VoltTransform2D value) => buffer.PutVoltTransform2D(value);
        public override VoltTransform2D Deserialize(StreamPeerBuffer buffer) => buffer.GetVoltTransform2D();
    }

    public class VoltRect2Serializer : TypeSerializer<VoltRect2>
    {
        public static readonly VoltRect2Serializer Global = new VoltRect2Serializer();

        public override void Serialize(StreamPeerBuffer buffer, VoltRect2 value) => buffer.PutVoltRect2(value);
        public override VoltRect2 Deserialize(StreamPeerBuffer buffer) => buffer.GetVoltRect2();
    }

    public class VoltMatrixSerializer : TypeSerializer<VoltMatrix>
    {
        public static readonly VoltMatrixSerializer Global = new VoltMatrixSerializer();

        public override void Serialize(StreamPeerBuffer buffer, VoltMatrix value) => buffer.PutVoltMatrix(value);
        public override VoltMatrix Deserialize(StreamPeerBuffer buffer) => buffer.GetVoltMatrix();
    }
}
