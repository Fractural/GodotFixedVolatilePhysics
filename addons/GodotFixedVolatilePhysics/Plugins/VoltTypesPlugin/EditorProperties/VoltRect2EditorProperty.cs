using Fractural.Utils;
using Godot;

#if TOOLS
namespace Volatile.GodotEngine.Plugin
{
    [Tool]
    public class VoltRect2EditorProperty : SerializedEditorProperty<VoltRect2, VoltRect2Serializer>
    {
        private VoltVector2EditorProperty positionProperty;
        private VoltVector2EditorProperty sizeProperty;

        protected VoltRect2 workingValue;

        public VoltRect2EditorProperty()
        {
            positionProperty = new VoltVector2EditorProperty();
            positionProperty.SupressFocusable = true;
            positionProperty.UseManualValue = true;
            positionProperty.Label = "Position";
            positionProperty.ManualEditedProperty = "position";
            positionProperty.Connect("property_changed", this, nameof(OnPropertyChanged));

            sizeProperty = new VoltVector2EditorProperty();
            sizeProperty.SupressFocusable = true;
            sizeProperty.UseManualValue = true;
            sizeProperty.Label = "Size";
            sizeProperty.ManualEditedProperty = "size";
            sizeProperty.Connect("property_changed", this, nameof(OnPropertyChanged));

            var vbox = new VBoxContainer();
            vbox.AddChild(positionProperty);
            vbox.AddChild(sizeProperty);

            AddChild(vbox);
            AddFocusable(vbox);
            SetBottomEditor(vbox);
        }

        protected override void InternalUpdateProperty()
        {
            workingValue = Value;
            positionProperty.UpdateProperty(workingValue.Position);
            sizeProperty.UpdateProperty(workingValue.Size);
        }

        private void OnPropertyChanged(string property, object value, string field, bool changing)
        {
            if (updating) return;
            switch (property)
            {
                case "position":
                    workingValue.Position = positionProperty.ManualValue;
                    break;
                case "size":
                    workingValue.Size = sizeProperty.ManualValue;
                    break;
            }
            Value = workingValue;
        }
    }

    [Tool]
    public class VoltRect2EditorPropertyParser : SerializedEditorPropertyParser
    {
        public VoltRect2EditorPropertyParser() { }
        public override ISerializedEditorProperty ParseSerializedProperty(string[] args)
        {
            if (args.TryGet(0) == VoltPropertyHint.VoltRect2)
                return new VoltRect2EditorProperty();
            return null;
        }

        public override object GetDefaultObject(string[] args)
        {
            if (args.TryGet(0) == VoltPropertyHint.VoltRect2)
                return new VoltRect2();
            return null;
        }
    }
}
#endif