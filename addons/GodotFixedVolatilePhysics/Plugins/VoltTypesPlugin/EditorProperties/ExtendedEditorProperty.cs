using Godot;

#if TOOLS
namespace Volatile.GodotEngine.Plugin
{
    [Tool]
    public abstract class ExtendedEditorProperty : EditorProperty
    {
        public bool SupressFocusable { get; set; } = false;
        public string ManualEditedProperty { get; set; } = "";
        public Godot.Object ManualEditedObject { get; set; }
        public new string GetEditedProperty()
        {
            if (ManualEditedProperty != "")
                return ManualEditedProperty;
            return base.GetEditedProperty();
        }

        public new Godot.Object GetEditedObject()
        {
            if (ManualEditedObject != null)
                return ManualEditedObject;
            return base.GetEditedObject();
        }

        public new void AddFocusable(Control control)
        {
            // We don't want to add focus if we're
            // not using this as an editor property
            // inside the inspector. (ie. being
            // using inside of an array editor
            // property)
            if (SupressFocusable)
                base.AddFocusable(control);
        }

        protected bool updating;

        public override void UpdateProperty()
        {
            updating = true;
            InternalUpdateProperty();
            updating = false;
        }

        protected abstract void InternalUpdateProperty();
    }
}
#endif