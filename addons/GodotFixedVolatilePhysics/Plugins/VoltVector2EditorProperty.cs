using FixMath.NET;
using Fractural.Utils;
using Godot;

#if TOOLS
namespace Volatile.GodotEngine.Plugin
{
    [Tool]
    public class VoltVector2EditorProperty : VoltTypeEditorProperty<VoltVector2, VoltVector2Serializer>
    {
        private EditorSpinSlider[] spin = new EditorSpinSlider[2];

        public VoltVector2EditorProperty()
        {
            for (int i = 0; i < 2; i++)
            {
                spin[i] = new EditorSpinSlider();
                spin[i].HideSlider = true;
            }
            spin[0].Label = "x";
            spin[0].Connect("value_changed", this, nameof(OnXSpinChanged));
            spin[1].Label = "y";
            spin[1].Connect("value_changed", this, nameof(OnYSpinChanged));
        }

        private void OnXSpinChanged(double value)
        {
            if (updating) return;
            var valueCopy = Value;
            Value = new VoltVector2((Fix64)value, valueCopy.y);
        }

        private void OnYSpinChanged(double value)
        {
            if (updating) return;
            var valueCopy = Value;
            Value = new VoltVector2(valueCopy.x, (Fix64)value);
        }

        protected override void InternalUpdateProperty()
        {
            var valueCopy = Value;
            spin[0].Value = (double)valueCopy.x;
            spin[0].Value = (double)valueCopy.y;
        }
    }

    public class VoltVector2EditorPropertyParser : VoltTypeEditorPropertyParser
    {
        public override bool ParseProperty(EditorInspectorPlugin inspectorPlugin, Object @object, int type, string path, int hint, string hintText, int usage, string[] args)
        {
            if (args.TryGet(0) == VoltPropertyHint.Fix64)
            {
                var editorProperty = new VoltVector2EditorProperty();
                inspectorPlugin.AddPropertyEditor(path, editorProperty);
                return true;
            }
            return false;
        }
    }
}
#endif