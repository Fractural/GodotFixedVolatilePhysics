using Godot;
using System;
using System.Linq;
using System.Reflection;

#if TOOLS
namespace Volatile.GodotEngine.Plugin
{
    /// <summary>
    /// Parser for <see cref="SerializedEditorProperty"/>. This parser can also use getters and setters to pull data for, and set data from, the serialized editor property.
    /// </summary>
    [Tool]
    public abstract class SerializedEditorPropertyParser : ExtendedEditorPropertyParser
    {
        public override ExtendedEditorProperty ParseProperty(Godot.Object @object, int type, string path, int hint, string hintText, int usage, string[] args)
            => (ExtendedEditorProperty)ParseSerializedProperty(@object, type, path, hint, hintText, usage, args);

        public override ExtendedEditorProperty ParseProperty(string[] args)
            => (ExtendedEditorProperty)ParseSerializedProperty(args);

        public virtual ISerializedEditorProperty ParseSerializedProperty(Godot.Object @object, int type, string path, int hint, string hintText, int usage, string[] args)
        {
            var argsWithoutGetSet = args.Where(x => !x.Contains("set:") && !x.Contains("get:")).ToArray();
            // We don't want get set args interfering with regular arg parsing.
            // Regular arg parsing is order sensitive, therefore adding get:set:
            // arguments could break this order.
            var serializedProperty = ParseSerializedProperty(argsWithoutGetSet);

            if (serializedProperty != null)
            {
                var setterArgsString = args.FirstOrDefault(x => x.Contains("set:"));
                if (setterArgsString != default(string))
                {
                    string propertyName = setterArgsString.Substring(setterArgsString.LastIndexOf(':') + 1);

                    var objectType = @object.GetType();
                    var targetProperty = objectType.GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance);
                    if (targetProperty == null)
                        targetProperty = objectType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                    if (targetProperty == null)
                    {
                        GD.PrintErr($"VoltType: Expected property {propertyName} to exist on {@object.GetType()} as a setter.");
                    }
                    else
                    {
                        serializedProperty.ValueChanged += (value)
                            =>
                        {
                            targetProperty.SetValue(@object, value);
                        };
                    }
                }

                var getterArgsString = args.FirstOrDefault(x => x.Contains("get:"));
                if (getterArgsString != default(string))
                {
                    string propertyName = getterArgsString.Substring(getterArgsString.LastIndexOf(':') + 1);

                    var objectType = @object.GetType();

                    var targetProperty = objectType.GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance);

                    if (targetProperty == null)
                        targetProperty = objectType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

                    if (targetProperty == null)
                    {
                        GD.PrintErr($"VoltType: Expected property {propertyName} to exist on {@object.GetType()} as a getter.");
                    }
                    else
                    {
                        serializedProperty.GetValue = ()
                            =>
                        {
                            return targetProperty.GetValue(@object);
                        };
                    }
                }

                return serializedProperty;
            }
            return null;
        }

        public abstract ISerializedEditorProperty ParseSerializedProperty(string[] args);
    }
}
#endif