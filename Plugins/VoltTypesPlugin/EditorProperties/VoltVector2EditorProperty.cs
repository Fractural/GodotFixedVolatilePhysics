using FixMath.NET;
using Fractural.Utils;
using Godot;
using System.Collections.Generic;
using System.Linq;

#if TOOLS
namespace Volatile.GodotEngine.Plugin
{
    [Tool]
    public class VoltVector2EditorProperty : SerializedEditorProperty<VoltVector2, VoltVector2Serializer>
    {
        private EditorSpinSlider[] spin = new EditorSpinSlider[2];
        protected VoltVector2 workingValue;

        public VoltVector2EditorProperty()
        {
            for (int i = 0; i < 2; i++)
            {
                spin[i] = new EditorSpinSlider();
                spin[i].AllowGreater = true;
                spin[i].AllowLesser = true;
                spin[i].HideSlider = true;
                spin[i].Step = 0;
                AddFocusable(spin[i]);
            }

            AddChild(spin[0]);
            var bottomEditorContainer = new HBoxContainer();
            var padding = new Control();

            padding.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            spin[1].SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;

            bottomEditorContainer.AddChild(padding);
            bottomEditorContainer.AddChild(spin[1]);

            AddChild(bottomEditorContainer);
            SetBottomEditor(bottomEditorContainer);

            spin[0].Label = "x";
            spin[0].Connect("value_changed", this, nameof(OnXSpinChanged));
            spin[1].Label = "y";
            spin[1].Connect("value_changed", this, nameof(OnYSpinChanged));
        }

        private void OnXSpinChanged(double value)
        {
            if (updating) return;
            Value = new VoltVector2((Fix64)value, workingValue.y);
        }

        private void OnYSpinChanged(double value)
        {
            if (updating) return;
            Value = new VoltVector2(workingValue.x, (Fix64)value);
        }

        protected override void InternalUpdateProperty()
        {
            workingValue = Value;
            spin[0].Value = (double)workingValue.x;
            spin[1].Value = (double)workingValue.y;
        }
    }

    [Tool]
    public class VoltVector2EditorPropertyParser : SerializedEditorPropertyParser
    {
        public VoltVector2EditorPropertyParser() { }
        public override ISerializedEditorProperty ParseSerializedProperty(string[] args)
        {
            if (args.TryGet(0) == VoltPropertyHint.VoltVector2)
                return new VoltVector2EditorProperty();
            return null;
        }

        public override object GetDefaultObject(string[] args)
        {
            if (args.TryGet(0) == VoltPropertyHint.VoltVector2)
                return VoltVector2.Zero;
            return null;
        }
    }
}
#endif