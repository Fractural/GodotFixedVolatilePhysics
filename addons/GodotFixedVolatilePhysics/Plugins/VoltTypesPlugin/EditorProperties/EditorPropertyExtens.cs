#if TOOLS
namespace Volatile.GodotEngine.Plugin
{
    public static class EditorPropertyExtens
    {
        /// <summary>
        /// Configures the <see cref="IExtendedEditorProperty"/> to override regular editor functionality. This is used for embedding this editor property inside of another editor proeprty.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="manualEditedProperty"></param>
        /// <param name="manualEditedObject"></param>
        /// <param name="suppressFocusable"></param>
        public static void ConfigureOverrides(this IExtendedEditorProperty property, string manualEditedProperty = "", Godot.Object manualEditedObject = null, bool suppressFocusable = false, string label = "")
        {
            property.ManualEditedProperty = manualEditedProperty;
            property.ManualEditedObject = manualEditedObject;
            property.SupressFocusable = suppressFocusable;
            property.Label = label;
        }

        /// <summary>
        /// Configures the <see cref="ISerializedEditorProperty"/> to override regular editor functionality. This is used for embedding this editor property inside of another editor proeprty.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="manualEditedProperty"></param>
        /// <param name="manualEditedObject"></param>
        /// <param name="suppressFocusable"></param>
        /// <param name="useManualValue"></param>
        public static void ConfigureOverrides(this ISerializedEditorProperty property, string manualEditedProperty = "", Godot.Object manualEditedObject = null, bool suppressFocusable = false, string label = "", bool useManualValue = false)
        {
            ((IExtendedEditorProperty)property).ConfigureOverrides(manualEditedProperty, manualEditedObject, suppressFocusable, label);
            property.UseManualValue = useManualValue;
        }
    }
}
#endif