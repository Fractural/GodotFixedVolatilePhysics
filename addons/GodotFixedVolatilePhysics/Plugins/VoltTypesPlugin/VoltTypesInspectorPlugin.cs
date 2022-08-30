using FixMath.NET;
using Godot;
using Fractural.Utils;

#if TOOLS
namespace Volatile.GodotEngine.Plugin
{
    [Tool]
    public class VoltTypesInspectorPlugin : EditorInspectorPlugin
    {
        public ExtendedEditorPropertyParser[] Parsers { get; private set; }

        public VoltTypesInspectorPlugin() { }
        public VoltTypesInspectorPlugin(EditorPlugin plugin)
        {
            var settings = plugin.GetEditorInterface().GetEditorSettings();

            Parsers = new ExtendedEditorPropertyParser[]{
                new Fix64EditorPropertyParser(),
                new VoltVector2EditorPropertyParser(),
                new VoltArrayEditorPropertyParser(this, settings.Get<int>("interface/inspector/max_array_dictionary_items_per_page")),
            };
        }

        public ExtendedEditorProperty GetEditorProperty(params string[] hintArgs)
        {
            foreach (var parser in Parsers)
            {
                var prop = parser.ParseProperty(hintArgs);
                if (prop != null)
                    return prop;
            }
            GD.PrintErr("Couldn't get the editor property for the args: " + hintArgs);
            return null;
        }

        public byte[] GetDefaultBytesForType(string type)
        {
            foreach (var parser in Parsers)
            {
                var bytes = parser.GetDefaultBytes(type);
                if (bytes != null)
                    return bytes;
            }
            GD.PrintErr("Couldn't get the default bytes for type: " + type);
            return null;
        }

        public override bool CanHandle(Object @object)
        {
            return true;
        }

        public override bool ParseProperty(Object @object, int type, string path, int hint, string hintText, int usage)
        {
            foreach (var parser in Parsers)
            {
                var prop = parser.ParseProperty(@object, type, path, hint, hintText, usage, hintText.Split(','));
                if (prop != null)
                {
                    AddPropertyEditor(path, prop);
                    return true;
                }
            }
            return base.ParseProperty(@object, type, path, hint, hintText, usage);
        }
    }

    public static class VoltPropertyHint
    {
        public const string Array = nameof(Array);
        public const string Fix64 = nameof(Fix64);
        public const string VoltVector2 = nameof(VoltVector2);
        public const string VoltTransform2D = nameof(VoltTransform2D);

        public static readonly string[] Values =
        {
            Array,
            Fix64,
            VoltVector2,
            VoltTransform2D
        };
    }
}
#endif