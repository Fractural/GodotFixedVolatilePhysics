using Fractural.Utils;
using Godot;
using System;

namespace Volatile.GodotEngine
{
    public static class VoltType
    {
        public static readonly ITypeSerializer[] TypeSerializers = new ITypeSerializer[]
        {
            Fix64Serializer.Global,
            VoltVector2Serializer.Global,
            VoltTransform2DSerializer.Global,
            VoltRect2Serializer.Global,
            VoltMatrixSerializer.Global
        };

        public static readonly ArraySerializer ArraySerializer = ArraySerializer.Global;

        public static void Serialize(System.Type type, StreamPeerBuffer buffer, object value)
        {
            if (type.IsArray)
            {
                ArraySerializer.Serialize(type.GetElementType(), buffer, (Array)value);
                return;
            }
            foreach (var serializer in TypeSerializers)
                if (serializer.IsInstanceOfGenericType(typeof(TypeSerializer<>), type))
                    ((IBufferSerializer)serializer).Serialize(buffer, value);
            GD.PrintErr("VoltType: Couldn't serialize " + value.GetType().FullName);
        }
        public static byte[] Serialize(System.Type type, object value)
        {
            if (type.IsArray)
                return ArraySerializer.Serialize(type.GetElementType(), (Array)value);
            foreach (var serializer in TypeSerializers)
                if (serializer.IsInstanceOfGenericType(typeof(TypeSerializer<>), type))
                    return serializer.Serialize(value);
            GD.PrintErr("VoltType: Couldn't serialize " + value.GetType().FullName);
            return null;
        }

        public static void Serialize<T>(StreamPeerBuffer buffer, T value) => Serialize(typeof(T), buffer, value);
        public static byte[] Serialize<T>(T value) => Serialize(typeof(T), value);

        public static object Deserialize(System.Type type, byte[] data)
        {
            if (type.IsArray)
                return ArraySerializer.Deserialize(type.GetElementType(), data);
            foreach (var serializer in TypeSerializers)
            {
                if (serializer.IsInstanceOfGenericType(typeof(TypeSerializer<>), type))
                    return serializer.Deserialize(data);
            }
            GD.PrintErr("VoltType: Couldn't deserialize " + type.FullName);
            return null;
        }
        public static object Deserialize(System.Type type, StreamPeerBuffer buffer)
        {
            if (type.IsArray)
                return ArraySerializer.Deserialize(type.GetElementType(), buffer);
            foreach (var serializer in TypeSerializers)
            {
                if (serializer.IsInstanceOfGenericType(typeof(TypeSerializer<>), type))
                    return ((IBufferSerializer)serializer).Deserialize(buffer);
            }
            GD.PrintErr("VoltType: Couldn't deserialize " + type.FullName);
            return null;
        }

        public static object Default(System.Type type)
        {
            foreach (var serializer in TypeSerializers)
                if (serializer.IsInstanceOfGenericType(typeof(TypeSerializer<>), type))
                    return serializer.Default();
            GD.PrintErr("VoltType: Couldn't get default for type " + type.FullName);
            return null;
        }
        public static T Default<T>()
        {
            return (T)Default(typeof(T));
        }

        public static object DeserializeOrDefault(System.Type type, byte[] data)
        {
            if (data == null)
                return Default(type);
            return Deserialize(type, data);
        }
        public static T DeserializeOrDefault<T>(byte[] data)
        {
            return (T)DeserializeOrDefault(typeof(T), data);
        }

        public static T Deserialize<T>(byte[] data)
        {
            var result = Deserialize(typeof(T), data);
            if (result.GetType() != typeof(T))
            {
                GD.PrintErr("VoltType: Deserialize got " + result.GetType() + " but expected " + typeof(T));
            }
            return (T)result;
        }

        public static T Deserialize<T>(StreamPeerBuffer buffer) => (T)Deserialize(typeof(T), buffer);
    }
}