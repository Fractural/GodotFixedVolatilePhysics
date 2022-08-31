using FixMath.NET;
using Fractural.Utils;
using Godot;
using System.Linq;

#if TOOLS
namespace Volatile.GodotEngine.Plugin
{
    [Tool]
    public class Fix64EditorProperty : SerializedEditorProperty<Fix64, Fix64Serializer>
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

    [Tool]
    public class Fix64EditorPropertyParser : ExtendedEditorPropertyParser
    {
        public Fix64EditorPropertyParser() { }
        public override ExtendedEditorProperty ParseProperty(string[] args)
        {
            if (args.TryGet(0) == VoltPropertyHint.Fix64)
            {
                bool hasRange = true;

                double min = 0;
                if (args.TryGet(1, out string minString))
                    min = double.Parse(minString);
                else
                    hasRange = false;

                double max = 0;
                if (args.TryGet(2, out string maxString))
                    max = double.Parse(maxString);
                else
                    hasRange = false;

                double step = double.Parse(args.TryGet(3, "0.0001"));

                bool orLesser = args.Any(x => x == "or_lesser");
                bool orGreater = args.Any(x => x == "or_greater");
                bool slider = args.Any(x => x == "slider");
                bool expRange = args.Any(x => x == "exp_range");

                if (!hasRange)
                {
                    orLesser = true;
                    orGreater = true;
                }

                var editorProperty = new Fix64EditorProperty();
                editorProperty.Setup(min, max, step, !slider, expRange, orGreater, orLesser);

                return editorProperty;
            }
            return null;
        }

        public override byte[] GetDefaultBytes(string type)
        {
            if (type == VoltPropertyHint.Fix64)
                return Fix64Serializer.Global.Serialize(Fix64.Zero);
            return null;
        }
    }
}
#endif