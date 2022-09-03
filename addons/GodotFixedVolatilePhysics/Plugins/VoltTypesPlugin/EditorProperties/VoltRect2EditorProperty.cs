using Fractural.Utils;
using Godot;

#if TOOLS
namespace Volatile.GodotEngine.Plugin
{
    [Tool]
    public class VoltRect2EditorProperty : CompoundSerializedEditorProperty<VoltRect2, VoltRect2Serializer>
    {
        private VoltVector2EditorProperty positionProperty;
        private VoltVector2EditorProperty sizeProperty;

        public VoltRect2EditorProperty() { }
        public VoltRect2EditorProperty(float scale) : base(scale) { }

        protected override void InitSubProperties()
        {
            positionProperty = new VoltVector2EditorProperty();
            AddProperty(positionProperty, "Position");

            sizeProperty = new VoltVector2EditorProperty();
            AddProperty(sizeProperty, "Size");
        }

        protected override void InternalUpdateProperty()
        {
            positionProperty.UpdateProperty(workingValue.Position);
            sizeProperty.UpdateProperty(workingValue.Size);
        }

        protected override void OnSubPropertyChanged(string property)
        {
            switch (property)
            {
                case "Position":
                    workingValue.Position = positionProperty.Value;
                    break;
                case "Size":
                    workingValue.Size = sizeProperty.Value;
                    break;
            }
        }
    }

    [Tool]
    public class VoltRect2EditorPropertyParser : SerializedEditorPropertyParser
    {
        private float scale;

        public VoltRect2EditorPropertyParser() { }
        public VoltRect2EditorPropertyParser(float scale)
        {
            this.scale = scale;
        }

        public override ISerializedEditorProperty ParseSerializedProperty(string[] args)
        {
            if (args.TryGet(0) == VoltPropertyHint.VoltRect2)
                return new VoltRect2EditorProperty(scale);
            return null;
        }

        public override object GetDefaultObject(string[] args)
        {
            if (args.TryGet(0) == VoltPropertyHint.VoltRect2)
                return VoltRect2Serializer.Global.Default();
            return null;
        }
    }
}
#endif