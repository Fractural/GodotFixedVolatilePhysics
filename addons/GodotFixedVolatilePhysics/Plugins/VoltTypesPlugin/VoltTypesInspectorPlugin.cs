﻿using FixMath.NET;
using Godot;
using Fractural.Utils;
using GDC = Godot.Collections;

#if TOOLS
namespace Volatile.GodotEngine.Plugin
{
    [Tool]
    public class VoltTypesInspectorPlugin : EditorInspectorPlugin
    {
        public GDC.Array<ExtendedEditorPropertyParser> Parsers { get; private set; } = new GDC.Array<ExtendedEditorPropertyParser>();

        public VoltTypesInspectorPlugin() { }
        public VoltTypesInspectorPlugin(EditorPlugin plugin)
        {
            var settings = plugin.GetEditorInterface().GetEditorSettings();

            Parsers.Add(new Fix64EditorPropertyParser());
            Parsers.Add(new VoltVector2EditorPropertyParser());
            Parsers.Add(new VoltArrayEditorPropertyParser(
                plugin.GetEditorInterface(),
                this,
                settings.Get<int>("interface/inspector/max_array_dictionary_items_per_page"))
            );
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
}
#endif