using FixMath.NET;
using Godot;

#if TOOLS
namespace Volatile.GodotEngine.Plugin
{
    [Tool]
    public class Fix64EditorProperty : EditorProperty
    {
        public SpinBox spinBox;
        public bool updating;

        public byte[] currValue => GetEditedObject().Get(GetEditedProperty()) as byte[];
        private Fix64 initialValue;

        public Fix64EditorProperty() { }

        public Fix64EditorProperty(Fix64 initialValue)
        {
            this.initialValue = initialValue;
            spinBox = new SpinBox();
            spinBox.Step = 0;
            spinBox.AllowLesser = true;
            spinBox.AllowGreater = true;
            spinBox.Connect("value_changed", this, nameof(OnSpinBoxChanged));

            AddChild(spinBox);
            AddFocusable(spinBox);
        }

        public override void _Ready()
        {
            if (currValue == null || currValue.Length == 0)
            {
                var buffer = new StreamPeerBuffer();
                buffer.PutFix64(initialValue);
                EmitChanged(GetEditedProperty(), buffer.DataArray);
            }
        }

        private void OnSpinBoxChanged(double value)
        {
            if (updating) return;

            var buffer = new StreamPeerBuffer();
            buffer.PutFix64((Fix64)value);
            EmitChanged(GetEditedProperty(), buffer.DataArray);
        }

        public override void UpdateProperty()
        {
            updating = true;
            var buffer = new StreamPeerBuffer();
            buffer.PutData(currValue);
            buffer.Seek(0);
            spinBox.Value = (double)buffer.GetFix64();
            updating = false;

        }
    }
}
#endif