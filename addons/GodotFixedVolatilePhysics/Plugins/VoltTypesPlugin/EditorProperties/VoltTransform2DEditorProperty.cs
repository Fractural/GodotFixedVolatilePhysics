using Fractural.Utils;
using Godot;

#if TOOLS
namespace Volatile.GodotEngine.Plugin
{
    [Tool]
    public class VoltTransform2DEditorProperty : CompoundSerializedEditorProperty<VoltTransform2D, VoltTransform2DSerializer>
    {
        private VoltVector2EditorProperty xProperty;
        private VoltVector2EditorProperty yProperty;
        private VoltVector2EditorProperty originProperty;

        public VoltTransform2DEditorProperty() { }
        public VoltTransform2DEditorProperty(float scale) : base(scale) { }

        protected override void InitSubProperties()
        {
            xProperty = new VoltVector2EditorProperty();
            AddProperty(xProperty, "X");

            yProperty = new VoltVector2EditorProperty();
            AddProperty(yProperty, "Y");

            originProperty = new VoltVector2EditorProperty();
            AddProperty(originProperty, "Origin");
        }

        protected override void InternalUpdateProperty()
        {
            xProperty.UpdateProperty(workingValue.X);
            yProperty.UpdateProperty(workingValue.Y);
            originProperty.UpdateProperty(workingValue.Origin);
        }

        protected override void OnSubPropertyChanged(string property)
        {
            switch (property)
            {
                case "X":
                    workingValue.X = xProperty.Value;
                    break;
                case "Y":
                    workingValue.Y = yProperty.Value;
                    break;
                case "Origin":
                    workingValue.Origin = originProperty.Value;
                    break;
            }
        }
    }

    [Tool]
    public class VoltTransform2DEditorPropertyParser : SerializedEditorPropertyParser
    {
        private float scale;

        public VoltTransform2DEditorPropertyParser() { }
        public VoltTransform2DEditorPropertyParser(float scale)
        {
            this.scale = scale;
        }

        public override ISerializedEditorProperty ParseSerializedProperty(string[] args)
        {
            if (args.TryGet(0) == VoltPropertyHint.VoltTransform2D)
                return new VoltTransform2DEditorProperty(scale);
            return null;
        }

        public override object GetDefaultObject(string[] args)
        {
            if (args.TryGet(0) == VoltPropertyHint.VoltTransform2D)
                return VoltTransform2D.Default();
            return null;
        }
    }
}
#endif