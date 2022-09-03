using FixMath.NET;
using Godot;
using Fractural.Utils;
using GDC = Godot.Collections;

#if TOOLS
namespace Volatile.GodotEngine.Plugin
{
    [Tool]
    public class VoltTypesInspectorPlugin : EditorInspectorPlugin
    {
        public GDC.Array<SerializedEditorPropertyParser> Parsers { get; private set; }

        private EditorPlugin plugin;

        public VoltTypesInspectorPlugin() { }
        public VoltTypesInspectorPlugin(EditorPlugin plugin)
        {
            this.plugin = plugin;
            var settings = plugin.GetEditorInterface().GetEditorSettings();
            var scale = plugin.GetEditorInterface().GetEditorScale();

            Parsers = new GDC.Array<SerializedEditorPropertyParser>(
                new Fix64EditorPropertyParser(),
                new VoltVector2EditorPropertyParser(),
                new VoltArrayEditorPropertyParser(
                    plugin.GetEditorInterface(),
                    this,
                    settings.Get<int>("interface/inspector/max_array_dictionary_items_per_page")
                ),
                new VoltRect2EditorPropertyParser(scale),
                new VoltTransform2DEditorPropertyParser(scale)
            );
        }

        /// <summary>
        /// Returns the editor property based on the <paramref name="hintArgs"/>
        /// </summary>
        /// <param name="hintArgs"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the default object based on the <paramref name="hintArgs"/>
        /// </summary>
        /// <param name="hintArgs"></param>
        /// <returns></returns>
        public object GetDefaultObject(string[] hintArgs)
        {
            foreach (var parser in Parsers)
            {
                var @object = parser.GetDefaultObject(hintArgs);
                if (@object != null)
                    return @object;
            }
            GD.PrintErr("Couldn't get the default object for args: " + hintArgs);
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
                var args = hintText.Split(',');
                var prop = parser.ParseProperty(@object, type, path, hint, hintText, usage, args);
                if (prop != null)
                {
                    // We only want to set a default if the getter doesn't exist
                    // If there is a getter, then we aren't serializing this value, so
                    // there's no point in setting a default.
                    if (@object.Get(path) == null)
                    {
                        var defaultObj = GetDefaultObject(args);
                        var serializedResult = VoltType.Serialize(defaultObj.GetType(), defaultObj);
                        @object.Set(path, serializedResult);
                        // Save scene when the default is set
                        plugin.GetEditorInterface().SaveScene();
                    }
                    AddPropertyEditor(path, prop);
                    return true;
                }
            }
            return base.ParseProperty(@object, type, path, hint, hintText, usage);
        }
    }
}
#endif