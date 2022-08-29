using Fractural;
using Godot;
using Godot.Collections;

namespace Volatile.GodotEngine
{
    public abstract class Boxed<T> : Godot.Reference
    {
        public byte[] Data;
        private T value;
        public T Value
        {
            get
            {
                if (Engine.EditorHint)
                    return GetValueFromData();
                else
                    return value;
            }
            set
            {
                if (Engine.EditorHint)
                    SetValueFromData(value);
                else
                    this.value = value;
            }
        }

        public Boxed(byte[] data) : this()
        {
            Data = data;
        }

        public Boxed()
        {
            if (!Engine.EditorHint)
                Value = GetValueFromData();
        }

        public T GetValueFromData()
        {
            var buffer = new StreamPeerBuffer();
            buffer.PutData(Data);
            buffer.Seek(0);
            return InternalGetValueFromData(buffer);
        }
        public void SetValueFromData(T value)
        {
            var buffer = new StreamPeerBuffer();
            InternalSetValueFromData(buffer, value);
            Data = buffer.DataArray;
        }
        protected abstract T InternalGetValueFromData(StreamPeerBuffer buffer);
        protected abstract void InternalSetValueFromData(StreamPeerBuffer buffer, T value);

        public override Array _GetPropertyList()
        {
            var builder = new PropertyListBuilder();
            builder.AddItem(
                name: nameof(Data),
                type: Variant.Type.RawArray,
                hint: PropertyHint.None,
                hintString: "",
                usage: PropertyUsageFlags.Storage
            );
            return builder.Build();
        }
    }
}
