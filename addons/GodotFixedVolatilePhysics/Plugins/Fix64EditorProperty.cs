using FixMath.NET;
using Fractural.Utils;
using Godot;
using System.Linq;

#if TOOLS
namespace Volatile.GodotEngine.Plugin
{
    [Tool]
    public class Fix64EditorProperty : VoltTypeEditorProperty<Fix64, Fix64Serializer>
    {
        private EditorSpinSlider spin;

        public Fix64EditorProperty()
        {
            spin = new EditorSpinSlider();
            spin.Connect("value_changed", this, nameof(OnSpinBoxChanged));

            AddChild(spin);
            AddFocusable(spin);
        }

        public void Setup(double min, double max, double step, bool noSlider, bool expRange, bool greater, bool lesser)
        {
            spin.Step = step;
            spin.AllowLesser = lesser;
            spin.AllowGreater = greater;
            spin.HideSlider = noSlider;
            spin.MinValue = min;
            spin.MaxValue = max;
            spin.ExpEdit = expRange;
        }

        private void OnSpinBoxChanged(double value)
        {
            if (updating) return;
            Value = (Fix64)value;
        }

        protected override void InternalUpdateProperty()
        {
            spin.Value = (double)TypeSerializer.Deserialize(Data);
        }
    }

    public class Fix64EditorPropertyParser : VoltTypeEditorPropertyParser
    {
        public override bool ParseProperty(EditorInspectorPlugin inspectorPlugin, Object @object, int type, string path, int hint, string hintText, int usage, string[] args)
        {
            if (args.TryGet(0) == VoltPropertyHint.Fix64)
            {
                double min = double.Parse(args.TryGet(1, "0"));
                double max = double.Parse(args.TryGet(2, "0"));
                double step = double.Parse(args.TryGet(3, "0.0001"));

                bool orLesser = args.Any(x => x == "or_lesser");
                bool orGreater = args.Any(x => x == "or_greater");
                bool slider = args.Any(x => x == "slider");
                bool expRange = args.Any(x => x == "exp_range");

                var editorProperty = new Fix64EditorProperty();
                editorProperty.Setup(min, max, step, !slider, expRange, orGreater, orLesser);

                inspectorPlugin.AddPropertyEditor(path, editorProperty);
                return true;
            }
            return false;
        }
    }
}
#endif