using Godot;
using System;
using System.Collections.Generic;
using GDC = Godot.Collections;

#if TOOLS
namespace Volatile.GodotEngine.Plugin
{
    public interface ISerializedEditorProperty : IExtendedEditorProperty
    {
        /// <summary>
        /// Emitted when the value of the editor property changes. This is emitted no matter if a manual value is being used, or if the value is being serialized to from a byte[].
        /// </summary>
        event Action<object> ValueChanged;
        Func<object> GetValue { get; set; }
        object Value { get; }
        bool UseManualValue { get; set; }
        void UpdateProperty(object manualValue);
    }

    /// <summary>
    /// Nongeneric version of <see cref="SerializedEditorProperty{T, TTypeSerializer}"/>. Useful for a more granular handling of serialization to and from a <see cref="byte"/>[].
    /// </summary>
    [Tool]
    public abstract class SerializedEditorProperty : ExtendedEditorProperty, ISerializedEditorProperty
    {
        public Func<object> GetValue { get; set; }
        public object manualValue;
        /// <summary>
        /// The actual value this property is dealing with. This value could be provided through: <see cref="GetValue"/>, <see cref="manualValue"/>, or the <see cref="Serialize(object)"/> and <see cref="Deserialize(byte[])"/> methods.
        /// </summary>
        public object Value
        {
            get
            {
                if (GetValue != null)
                    return GetValue();
                if (UseManualValue)
                    return manualValue;
                return Deserialize(Data);
            }
        }
        public bool UseManualValue { get; set; }
        public event Action<object> ValueChanged;

        protected byte[] Data => GetEditedObject().Get(GetEditedProperty()) as byte[];
        protected object workingValue;

        public void UpdateProperty(object manualValue)
        {
            this.manualValue = manualValue;
            ValueChanged?.Invoke(this.manualValue);
            UpdateProperty();
        }

        public override void UpdateProperty()
        {
            workingValue = Value;
            base.UpdateProperty();
        }

        protected void SerializeWorkingValueToEditor()
        {
            if (UseManualValue)
            {
                manualValue = workingValue;
                // We don't emit the value, in case it's not marshallable by Godot
                EmitChanged(GetEditedProperty(), null);
            }
            else
                EmitChanged(GetEditedProperty(), Serialize(workingValue));
            ValueChanged?.Invoke(workingValue);
        }

        // We don't use TypeSerializer here and use separate methods instead
        // in order to accomodate for serialization that needs more than just
        // the bytes of the data. (ie. arrays, where you need to know the array
        // type)
        protected abstract object Deserialize(byte[] data);
        protected abstract byte[] Serialize(object workingValue);

        public void ConfigureOverrides(string manualEditedProperty = "", Godot.Object manualEditedObject = null, bool suppressFocusable = false, bool useManualValue = false)
        {
            ConfigureOverrides(manualEditedProperty, manualEditedObject, suppressFocusable);
            UseManualValue = useManualValue;
        }
    }

    /// <summary>
    /// Editor property that serializes itself into a <see cref="byte"/>[].
    /// </summary>
    /// <typeparam name="T">Type that this editor property handles. It's also the type that <typeparamref name="TTypeSerializer"/> handles.</typeparam>
    /// <typeparam name="TTypeSerializer"><see cref="TypeSerializer{T}"/> to use for the serialization of this property.</typeparam>
    [Tool]
    public abstract class SerializedEditorProperty<T, TTypeSerializer> : ExtendedEditorProperty, ISerializedEditorProperty where TTypeSerializer : TypeSerializer<T>, new()
    {
        public event Action<object> ValueChanged;
        object ISerializedEditorProperty.Value { get => Value; }
        public Func<object> GetValue { get; set; }

        private T manualValue;
        /// <summary>
        /// The actual value this property is dealing with. This value could be provided through: <see cref="GetValue"/>, <see cref="manualValue"/>, or <see cref="TypeSerializer"/>'s serialization.
        /// </summary>
        public T Value
        {
            get
            {
                if (GetValue != null)
                {
                    var result = GetValue();
                    if (result is T casted)
                        return casted;
                    GD.PrintErr($"SerializedEditorProperty: Expected type '{typeof(T)}' from {nameof(GetValue)} but got '{result}' of type '{result.GetType()}' instead.");
                    return default(T);
                }
                if (UseManualValue)
                    return manualValue;
                return TypeSerializer.Deserialize(Data);
            }
        }
        public bool UseManualValue { get; set; } = false;

        protected byte[] Data => GetEditedObject().Get(GetEditedProperty()) as byte[];
        protected T workingValue;
        protected TTypeSerializer TypeSerializer { get; } = new TTypeSerializer();

        public void UpdateProperty(object manualValue) => UpdateProperty((T)manualValue);
        public void UpdateProperty(T manualValue)
        {
            if (!UseManualValue) return;
            this.manualValue = manualValue;
            ValueChanged?.Invoke(manualValue);
            UpdateProperty();
        }

        public override void UpdateProperty()
        {
            workingValue = Value;
            base.UpdateProperty();
        }

        protected void SerializeWorkingValueToEditor()
        {
            if (UseManualValue)
            {
                manualValue = workingValue;
                // We don't emit the value, in case it's not marshallable by Godot
                EmitChanged(GetEditedProperty(), null);
            }
            else
                EmitChanged(GetEditedProperty(), TypeSerializer.Serialize(workingValue));
            ValueChanged?.Invoke(workingValue);
        }
    }
}
#endif