using Godot;
using System;

#if TOOLS
namespace Volatile.GodotEngine.Plugin
{
    [Tool]
    public abstract class SerializedEditorProperty<T, TTypeSerializer> : ExtendedEditorProperty where TTypeSerializer : TypeSerializer<T>, new()
    {
        public T ManualValue { get; set; }
        public bool UseManualValue { get; set; } = false;

        protected byte[] Data => GetEditedObject().Get(GetEditedProperty()) as byte[];
        protected T Value
        {
            get
            {
                if (UseManualValue)
                    return ManualValue;
                else
                {
                    GD.Print("getting actual " + GetEditedObject() + " is null? " + (GetEditedObject() == null));
                    return TypeSerializer.Deserialize(Data);
                }
            }
            set
            {
                if (UseManualValue)
                {
                    ManualValue = value;
                    GD.Print("emit changed with " + GetEditedObject() + " is null? " + (GetEditedObject() == null));
                    // We don't emit the value, in case it's not marshallable by Godot
                    EmitChanged(GetEditedProperty(), null);
                }
                else
                {
                    GD.Print("setting actual " + GetEditedObject() + " is null? " + (GetEditedObject() == null));
                    EmitChanged(GetEditedProperty(), TypeSerializer.Serialize(value));
                }
            }
        }
        protected TTypeSerializer TypeSerializer { get; } = new TTypeSerializer();

        public void UpdateProperty(T value)
        {
            ManualValue = value;
            UpdateProperty();
        }
    }
}
#endif