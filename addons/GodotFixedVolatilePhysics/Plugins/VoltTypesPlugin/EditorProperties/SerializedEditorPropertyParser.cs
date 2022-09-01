using Godot;
using System.Linq;
using System.Reflection;

#if TOOLS
namespace Volatile.GodotEngine.Plugin
{
    [Tool]
    public abstract class SerializedEditorPropertyParser : ExtendedEditorPropertyParser
    {
        public override ExtendedEditorProperty ParseProperty(Object @object, int type, string path, int hint, string hintText, int usage, string[] args)
            => (ExtendedEditorProperty)ParseSerializedProperty(@object, type, path, hint, hintText, usage, args);

        public override ExtendedEditorProperty ParseProperty(string[] args)
            => (ExtendedEditorProperty)ParseSerializedProperty(args);

        public virtual ISerializedEditorProperty ParseSerializedProperty(Object @object, int type, string path, int hint, string hintText, int usage, string[] args)
        {
            var serializedProperty = ParseSerializedProperty(args);
            if (serializedProperty != null)
            {
                var forwardArgsString = args.FirstOrDefault(x => x.BeginsWith(">"));
                if (forwardArgsString != default(string))
                {
                    string forwardTargetPropertyName = forwardArgsString.Remove(0, 1);

                    serializedProperty.ManualValueChanged += (value)
                        =>
                    {
                        var objectType = @object.GetType();
                        var forwardTargetProperty = objectType.GetProperty(forwardTargetPropertyName, BindingFlags.NonPublic | BindingFlags.Instance);
                        if (forwardTargetProperty == null)
                            forwardTargetProperty = objectType.GetProperty(forwardTargetPropertyName, BindingFlags.Public | BindingFlags.Instance);
                        if (forwardTargetProperty == null)
                        {
                            GD.PrintErr($"VoltType: Expected property {forwardTargetPropertyName} to exist on {@object.GetType()} for forwarding.");
                            return;
                        }
                        forwardTargetProperty.SetValue(@object, value);
                    };
                }

                if (@object.Get(path) == null)
                {
                    var defaultObj = GetDefaultObject(args);
                    @object.Set(path, defaultObj);
                }
                return serializedProperty;
            }
            return null;
        }

        public abstract ISerializedEditorProperty ParseSerializedProperty(string[] args);
    }
}
#endif