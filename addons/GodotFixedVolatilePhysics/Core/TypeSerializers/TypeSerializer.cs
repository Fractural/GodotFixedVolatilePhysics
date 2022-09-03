using Fractural;
using Godot;

namespace Volatile.GodotEngine
{
    public interface ITypeSerializer<T>
    {
        T Deserialize(byte[] data);
        byte[] Serialize(T value);
        T Default();
        byte[] DefaultAsBytes();
    }

    public interface ITypeSerializer
    {
        object Deserialize(byte[] data);
        byte[] Serialize(object value);
        object Default();
        byte[] DefaultAsBytes();
    }

    public interface IBufferSerializer<T>
    {
        T Deserialize(StreamPeerBuffer buffer);
        void Serialize(StreamPeerBuffer buffer, T value);
    }

    public interface IBufferSerializer
    {
        object Deserialize(StreamPeerBuffer buffer);
        void Serialize(StreamPeerBuffer buffer, object value);
    }

    public abstract class TypeSerializer<T> : ITypeSerializer<T>, ITypeSerializer, IBufferSerializer<T>, IBufferSerializer
    {
        object ITypeSerializer.Deserialize(byte[] data) => Deserialize(data);
        byte[] ITypeSerializer.Serialize(object value) => Serialize((T)value);
        object IBufferSerializer.Deserialize(StreamPeerBuffer buffer) => Deserialize(buffer);
        void IBufferSerializer.Serialize(StreamPeerBuffer buffer, object value) => Serialize(buffer, (T)value);
        object ITypeSerializer.Default() => Default();
        public abstract T Default();

        public T Deserialize(byte[] data)
        {
            var buffer = new StreamPeerBuffer();
            buffer.PutData(data);
            buffer.Seek(0);
            return Deserialize(buffer);
        }
        public byte[] Serialize(T value)
        {
            var buffer = new StreamPeerBuffer();
            Serialize(buffer, value);
            return buffer.DataArray;
        }
        public abstract T Deserialize(StreamPeerBuffer buffer);
        public abstract void Serialize(StreamPeerBuffer buffer, T value);
        public byte[] DefaultAsBytes() => Serialize(Default());
    }

    public abstract class TypeSerializer : ITypeSerializer, IBufferSerializer
    {
        public object Deserialize(byte[] data)
        {
            var buffer = new StreamPeerBuffer();
            buffer.PutData(data);
            buffer.Seek(0);
            return Deserialize(buffer);
        }

        public byte[] Serialize(object value)
        {
            var buffer = new StreamPeerBuffer();
            Serialize(buffer, value);
            return buffer.DataArray;
        }

        public abstract object Deserialize(StreamPeerBuffer buffer);
        public abstract void Serialize(StreamPeerBuffer buffer, object value);
        public abstract object Default();
        public byte[] DefaultAsBytes() => Serialize(Default());
    }
}
