using Godot;
using System;

namespace Volatile.GodotEngine
{
    public class ArraySerializer
    {
        public static readonly ArraySerializer Global = new ArraySerializer();

        public Array Deserialize(Type elementType, byte[] data)
        {
            var buffer = new StreamPeerBuffer();
            buffer.PutData(data);
            buffer.Seek(0);
            return Deserialize(elementType, buffer);
        }

        public byte[] Serialize(Type elementType, Array value)
        {
            var buffer = new StreamPeerBuffer();
            Serialize(elementType, buffer, value);
            return buffer.DataArray;
        }

        public Array Deserialize(Type elementType, StreamPeerBuffer buffer)
        {
            var length = buffer.GetU32();
            var array = Array.CreateInstance(elementType, length);
            for (int i = 0; i < length; i++)
            {
                // We don't to use the bytes for anything.
                // We can store at most 255 bytes per type.
                buffer.GetU8();
                array.SetValue(VoltType.Deserialize(elementType, buffer), i);
            }
            return array;
        }

        public Array Default(Type elementType) => Array.CreateInstance(elementType, 0);
        public byte[] DefaultAsBytes(Type elementType) => Serialize(elementType, Default(elementType));

        public void Serialize(Type elementType, StreamPeerBuffer buffer, Array array)
        {
            buffer.PutU32((uint)array.Length);
            for (int i = 0; i < array.Length; i++)
            {
                var byteArray = VoltType.Serialize(elementType, array.GetValue(i));
                // By storing the byte length per element, we can dynamically omit
                // unecessary data in each type serializer. But the caveat is that
                // every element will take up an extra byte.
                buffer.PutU8((byte)byteArray.Length);
                buffer.PutData(byteArray);
            }
        }
    }
}
