using Godot;

#if TOOLS
namespace Volatile.GodotEngine.Plugin
{
    /// <summary>
    /// Parser for <see cref="ExtendedEditorProperty"/>. It can return the default object for a type, and can also parse properties for the type.
    /// </summary>
    [Tool]
    public abstract class ExtendedEditorPropertyParser : Godot.Reference
    {
        public virtual ExtendedEditorProperty ParseProperty(Godot.Object @object, int type, string path, int hint, string hintText, int usage, string[] args)
        {
            var property = ParseProperty(args);
            if (property != null)
                return property;
            return null;
        }
        public abstract ExtendedEditorProperty ParseProperty(string[] args);

        public abstract object GetDefaultObject(string[] args);
    }
}
#endif