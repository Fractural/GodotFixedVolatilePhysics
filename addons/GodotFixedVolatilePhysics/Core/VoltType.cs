﻿using Fractural.Utils;
using Godot;

namespace Volatile.GodotEngine
{
    public static class VoltType
    {
        public static readonly object[] TypeSerializers = new object[]
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
                ArraySerializer.Serialize(type, buffer, value);
            foreach (var serializer in TypeSerializers)
                if (serializer.IsInstanceOfGenericType(typeof(TypeSerializer<>), type))
                    ((IBufferSerializer)serializer).Serialize(buffer, value);
            GD.PrintErr("VoltType: Couldn't serialize " + value.GetType().FullName);
        }
        public static byte[] Serialize(System.Type type, object value)
        {
            foreach (var serializer in TypeSerializers)
                if (serializer.IsInstanceOfGenericType(typeof(TypeSerializer<>), type))
                    return ((ITypeSerializer)serializer).Serialize(value);
            GD.PrintErr("VoltType: Couldn't serialize " + value.GetType().FullName);
            return null;
        }

        public static void Serialize<T>(StreamPeerBuffer buffer, T value) => Serialize(typeof(T), buffer, value);
        public static byte[] Serialize<T>(T value) => Serialize(typeof(T), value);

        public static object Deserialize(System.Type type, byte[] data)
        {
            if (type.IsArray)
                return ArraySerializer.Deserialize(type, data);
            foreach (var serializer in TypeSerializers)
            {
                if (serializer.IsInstanceOfGenericType(typeof(TypeSerializer<>), type))
                    return ((ITypeSerializer)serializer).Deserialize(data);
            }
            GD.PrintErr("VoltType: Couldn't deserialize " + type.FullName);
            return null;
        }
        public static object Deserialize(System.Type type, StreamPeerBuffer buffer)
        {
            if (type.IsArray)
                return ArraySerializer.Deserialize(type, buffer);
            foreach (var serializer in TypeSerializers)
            {
                if (serializer.IsInstanceOfGenericType(typeof(TypeSerializer<>), type))
                    return ((IBufferSerializer)serializer).Deserialize(buffer);
            }
            GD.PrintErr("VoltType: Couldn't deserialize " + type.FullName);
            return null;
        }

        public static T Deserialize<T>(byte[] data) => (T)Deserialize(typeof(T), data);
        public static T Deserialize<T>(StreamPeerBuffer buffer) => (T)Deserialize(typeof(T), buffer);
    }
}