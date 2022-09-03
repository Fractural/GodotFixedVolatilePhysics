using Godot;

#if TOOLS
namespace Volatile.GodotEngine.Plugin
{
    [Tool]
    public abstract class CompoundSerializedEditorProperty<T, TTypeSerializer> : SerializedEditorProperty<T, TTypeSerializer> where TTypeSerializer : TypeSerializer<T>, new()
    {
        public const int INDENTATION = 10;

        protected VBoxContainer propertiesVBox;

        public CompoundSerializedEditorProperty() { }
        public CompoundSerializedEditorProperty(float scale)
        {
            propertiesVBox = new VBoxContainer();

            var marginContainer = new MarginContainer();
            marginContainer.AddChild(propertiesVBox);
            marginContainer.AddConstantOverride("margin_left", (int)(INDENTATION * scale));

            InitSubProperties();

            AddChild(marginContainer);
            AddFocusable(marginContainer);
            SetBottomEditor(marginContainer);
        }

        protected abstract void InitSubProperties();
        protected void AddProperty<LT, LTTypeSerializer>(SerializedEditorProperty<LT, LTTypeSerializer> property, string label, string manualEditedProperty = "", bool supressFocusable = true, bool useManualValue = true) where LTTypeSerializer : TypeSerializer<LT>, new()
        {
            if (label != "" && manualEditedProperty == "")
                manualEditedProperty = label;
            property.SupressFocusable = true;
            property.UseManualValue = true;
            property.Label = label;
            property.ManualEditedProperty = manualEditedProperty;
            property.Connect("property_changed", this, nameof(OnPropertyChanged));
            propertiesVBox.AddChild(property);
        }

        private void OnPropertyChanged(string property, object value, string field, bool changing)
        {
            if (updating) return;
            OnSubPropertyChanged(property);
            SerializeWorkingValueToEditor();
        }

        protected abstract void OnSubPropertyChanged(string property);
    }
}
#endif