using Godot;

#if TOOLS
namespace Volatile.GodotEngine.Plugin
{
    [Tool]
    public abstract class ExtendedEditorPropertyParser : Godot.Reference
    {
        public virtual ExtendedEditorProperty ParseProperty(Godot.Object @object, int type, string path, int hint, string hintText, int usage, string[] args)
        {
            var property = ParseProperty(args);
            if (property != null)
            {
                if (@object.Get(path) == null) @object.Set(path, GetDefaultObject(args));
                return property;
            }
            return null;
        }
        public abstract ExtendedEditorProperty ParseProperty(string[] args);

        public abstract object GetDefaultObject(string[] args);
    }
}
#endif