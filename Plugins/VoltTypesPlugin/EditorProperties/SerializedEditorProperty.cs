using Godot;
using System;

#if TOOLS
namespace Volatile.GodotEngine.Plugin
{
    [Tool]
    public abstract class SerializedEditorProperty<T, TTypeSerializer> : ExtendedEditorProperty where TTypeSerializer : TypeSerializer<T>, new()
    {
        protected byte[] Data => GetEditedObject().Get(GetEditedProperty()) as byte[];
        protected T Value
        {
            get => TypeSerializer.Deserialize(Data);
            set
            {
                EmitChanged(GetEditedProperty(), TypeSerializer.Serialize(value));
            }
        }
        protected TTypeSerializer TypeSerializer { get; } = new TTypeSerializer();
    }
}
#endif