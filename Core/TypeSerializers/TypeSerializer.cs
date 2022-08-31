using Fractural;
using Godot;
using Godot.Collections;

namespace Volatile.GodotEngine
{
    public abstract class TypeSerializer<T>
    {
        public T Deserialize(byte[] data)
        {
            var buffer = new StreamPeerBuffer();
            buffer.PutData(data);
            buffer.Seek(0);
            return InternalDeserialize(buffer);
        }
        public byte[] Serialize(T value)
        {
            var buffer = new StreamPeerBuffer();
            InternalSerialize(buffer, value);
            return buffer.DataArray;
        }
        protected abstract T InternalDeserialize(StreamPeerBuffer buffer);
        protected abstract void InternalSerialize(StreamPeerBuffer buffer, T value);

        public class Array<TElementTypeSerializer> : TypeSerializer<T[]> where TElementTypeSerializer : TypeSerializer<T>, new()
        {
            private TElementTypeSerializer elementTypeSerializer = new TElementTypeSerializer();
            protected override T[] InternalDeserialize(StreamPeerBuffer buffer)
            {
                var length = buffer.GetU32();
                var array = new T[length];
                for (int i = 0; i < length; i++)
                {
                    // We don't to use the bytes for anything.
                    // We can store at most 255 bytes per type.
                    buffer.GetU8();
                    array[i] = elementTypeSerializer.InternalDeserialize(buffer);
                }
                return array;
            }

            protected override void InternalSerialize(StreamPeerBuffer buffer, T[] value)
            {
                buffer.PutU32((uint)value.Length);
                for (int i = 0; i < value.Length; i++)
                {
                    var byteArray = elementTypeSerializer.Serialize(value[i]);
                    // By storing the byte length per element, we can dynamically omit
                    // unecessary data in each type serializer. But the caveat is that
                    // every element will take up an extra byte.
                    buffer.PutU8((byte)byteArray.Length);
                    buffer.PutData(byteArray);
                }
            }
        }
    }
}
