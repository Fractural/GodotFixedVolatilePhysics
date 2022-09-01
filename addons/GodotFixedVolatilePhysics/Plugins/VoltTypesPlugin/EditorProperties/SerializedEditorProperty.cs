using Godot;
using System;

#if TOOLS
namespace Volatile.GodotEngine.Plugin
{
    public interface ISerializedEditorProperty
    {
        event Action<object> ManualValueChanged;
        object ManualValue { get; set; }
        bool UseManualValue { get; set; }
    }

    [Tool]
    public abstract class SerializedEditorProperty<T, TTypeSerializer> : ExtendedEditorProperty, ISerializedEditorProperty where TTypeSerializer : TypeSerializer<T>, new()
    {
        public event Action<object> ManualValueChanged;
        object ISerializedEditorProperty.ManualValue { get => ManualValue; set => ManualValue = (T)value; }

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
                    return TypeSerializer.Deserialize(Data);
            }
            set
            {
                if (UseManualValue)
                {
                    ManualValue = value;
                    // We don't emit the value, in case it's not marshallable by Godot
                    EmitChanged(GetEditedProperty(), null);
                }
                else
                    EmitChanged(GetEditedProperty(), TypeSerializer.Serialize(value));
            }
        }
        protected TTypeSerializer TypeSerializer { get; } = new TTypeSerializer();

        public void UpdateProperty(T value)
        {
            ManualValue = value;
            ManualValueChanged?.Invoke(ManualValue);
            UpdateProperty();
        }
    }
}
#endif