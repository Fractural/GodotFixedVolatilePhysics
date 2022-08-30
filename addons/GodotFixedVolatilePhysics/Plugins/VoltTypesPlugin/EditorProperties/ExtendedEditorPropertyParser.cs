using Godot;

#if TOOLS
namespace Volatile.GodotEngine.Plugin
{
    public abstract class ExtendedEditorPropertyParser : Godot.Reference
    {
        public virtual ExtendedEditorProperty ParseProperty(Godot.Object @object, int type, string path, int hint, string hintText, int usage, string[] args) => ParseProperty(args);
        public abstract ExtendedEditorProperty ParseProperty(string[] args);

        public abstract byte[] GetDefaultBytes(string type);
    }
}
#endif