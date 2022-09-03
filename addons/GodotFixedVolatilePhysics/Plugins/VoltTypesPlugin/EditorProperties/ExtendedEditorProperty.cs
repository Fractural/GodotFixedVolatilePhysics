using Godot;
using GDC = Godot.Collections;

#if TOOLS
namespace Volatile.GodotEngine.Plugin
{
    public interface IEditorProperty
    {
        string GetEditedProperty();
        Godot.Object GetEditedObject();
        void AddFocusable(Control control);
        void UpdateProperty();
        string Label { get; set; }
        Error Connect(string signal, Godot.Object target, string method, GDC.Array binds = null, uint flags = 0);
    }

    public interface IExtendedEditorProperty : IEditorProperty
    {
        bool SupressFocusable { get; set; }
        string ManualEditedProperty { get; set; }
        Godot.Object ManualEditedObject { get; set; }
    }

    [Tool]
    public abstract class ExtendedEditorProperty : EditorProperty, IExtendedEditorProperty
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

        public void ConfigureOverrides(string manualEditedProperty = "", Object manualEditedObject = null, bool supressFocusable = false)
        {
            ManualEditedProperty = manualEditedProperty;
            ManualEditedObject = manualEditedObject;
            SupressFocusable = supressFocusable;
        }
    }
}
#endif