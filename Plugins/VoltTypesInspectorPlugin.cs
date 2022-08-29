using FixMath.NET;
using Godot;

#if TOOLS
namespace Volatile.GodotEngine.Plugin
{
    [Tool]
    public class VoltTypesInspectorPlugin : EditorInspectorPlugin
    {
        public VoltTypeEditorPropertyParser[] Parsers { get; } =
        {
            new Fix64EditorPropertyParser(),
            new VoltVector2EditorPropertyParser(),
        };

        public override bool CanHandle(Object @object)
        {
            return true;
        }

        public override bool ParseProperty(Object @object, int type, string path, int hint, string hintText, int usage)
        {
            foreach (var parser in Parsers)
            {
                if (parser.ParseProperty(this, @object, type, path, hint, hintText, usage, hintText.Split(',')))
                    return true;
            }
            return base.ParseProperty(@object, type, path, hint, hintText, usage);
        }
    }

    public static class VoltPropertyHint
    {
        public const string Fix64 = nameof(Fix64);
        public const string Fix64Array = nameof(Fix64Array);
        public const string VoltVector2 = nameof(VoltVector2);
        public const string VoltVector2Array = nameof(VoltVector2Array);
        public const string VoltTransform2D = nameof(VoltTransform2D);
        public const string VoltTransform2DArray = nameof(VoltTransform2DArray);
    }
}
#endif