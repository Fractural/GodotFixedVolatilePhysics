using Godot;

#if TOOLS
namespace Volatile.GodotEngine.Plugin
{
    [Tool]
    public abstract class ExtendedEditorPropertyParser : Godot.Reference
    {
        public virtual ExtendedEditorProperty ParseProperty(Godot.Object @object, int type, string path, int hint, string hintText, int usage, string[] args)
        {
            var result = ParseProperty(args);
            if (result != null)
            {
                if (@object.Get(path) == null) @object.Set(path, GetDefaultBytes(args[0]));
                return result;
            }
            return null;
        }
        public abstract ExtendedEditorProperty ParseProperty(string[] args);

        public abstract byte[] GetDefaultBytes(string type);
    }
}
#endif