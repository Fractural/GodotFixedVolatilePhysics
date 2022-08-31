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

        protected VoltRect2 valueCopy;

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
            valueCopy = Value;
            positionProperty.UpdateProperty(valueCopy.Position);
            sizeProperty.UpdateProperty(valueCopy.Size);
        }

        private void OnPropertyChanged(string property, object value, string field, bool changing)
        {
            if (updating) return;
            GD.Print("updating " + valueCopy);
            switch (property)
            {
                case "position":
                    GD.Print("Real update position");
                    valueCopy.Position = positionProperty.ManualValue;
                    break;
                case "size":
                    GD.Print("real update size");
                    valueCopy.Size = sizeProperty.ManualValue;
                    break;
            }
            GD.Print("done prop change -> prep to emit" + valueCopy);
            EmitChanged();
        }

        private void EmitChanged()
        {
            GD.Print("emitting");
            EmitChanged(GetEditedProperty(), VoltRect2Serializer.Global.Serialize(valueCopy));

            GD.Print("emitting done");
        }
    }

    [Tool]
    public class VoltRect2EditorPropertyParser : ExtendedEditorPropertyParser
    {
        public VoltRect2EditorPropertyParser() { }
        public override ExtendedEditorProperty ParseProperty(string[] args)
        {
            if (args.TryGet(0) == VoltPropertyHint.VoltRect2)
                return new VoltRect2EditorProperty();
            return null;
        }

        public override byte[] GetDefaultBytes(string type)
        {
            if (type == VoltPropertyHint.VoltRect2)
                return VoltRect2Serializer.Global.Serialize(new VoltRect2());
            return null;
        }
    }
}
#endif