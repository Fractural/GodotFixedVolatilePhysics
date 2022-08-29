using Godot;

#if TOOLS
namespace Volatile.GodotEngine.Plugin
{
    [Tool]
    public abstract class VoltTypeEditorProperty<T, TTypeSerializer> : EditorProperty where TTypeSerializer : TypeSerializer<T>, new()
    {
        protected bool updating;
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

        public override void UpdateProperty()
        {
            updating = true;
            InternalUpdateProperty();
            updating = false;
        }

        protected abstract void InternalUpdateProperty();
    }

    public abstract class VoltTypeEditorPropertyParser
    {
        public abstract bool ParseProperty(EditorInspectorPlugin inspectorPlugin, Object @object, int type, string path, int hint, string hintText, int usage, string[] args);
    }
}
#endif