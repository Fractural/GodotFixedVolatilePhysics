using FixMath.NET;
using Godot;

#if TOOLS
namespace Volatile.GodotEngine.Plugin
{
    [Tool]
    public class VoltTypesInspectorPlugin : EditorInspectorPlugin
    {
        public override bool CanHandle(Object @object)
        {
            return true;
        }

        public override bool ParseProperty(Object @object, int type, string path, int hint, string hintText, int usage)
        {
            var propertyValue = @object.Get(path);
            var args = hintText.Split(',');
            if (args.Length > 0)
            {
                if (args[0] == VoltPropertyHint.Fix64)
                {
                    Fix64 initialValue = Fix64.Zero;
                    if (args.Length >= 2)
                        initialValue = Fix64.From(args[1]);
                    AddPropertyEditor(path, new Fix64EditorProperty(initialValue));
                    GD.Print("Can handle fix64boxed at: " + path + ": " + propertyValue);
                    return true;
                }
            }
            return base.ParseProperty(@object, type, path, hint, hintText, usage);
        }
    }

    public static class VoltPropertyHint
    {
        public const string Fix64 = nameof(Fix64);
    }
}
#endif