using Fractural.Utils;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using GDC = Godot.Collections;

#if TOOLS
namespace Volatile.GodotEngine.Plugin
{
    /// <summary>
    /// Custom EditorProperty for an array of Volt types. This only supports
    /// Arrays of the same type.
    /// </summary>
    [Tool]
    public class VoltArrayEditorProperty : SerializedEditorProperty
    {
        #region Base Variables + Constructor
        // Forwarding for types (Kinda dirty to have them forwarded atm)
        private string[] elementHintArgs;
        private Type elementType;
        private VoltTypesInspectorPlugin inspectorPlugin;

        private VBoxContainer bottomVBox;

        public VoltArrayEditorProperty() { }
        public VoltArrayEditorProperty(string[] elementHintArgs, EditorInterface editorInterface, VoltTypesInspectorPlugin inspectorPlugin, int pageLength)
        {
            this.editorInterface = editorInterface;
            this.pageLength = pageLength;

            this.elementType = VoltPropertyHint.HintToType[elementHintArgs[0]];
            this.inspectorPlugin = inspectorPlugin;
            this.elementHintArgs = elementHintArgs;

            editButton = new Button();
            editButton.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            editButton.ClipText = true;
            editButton.Connect("pressed", this, nameof(OnEditPressed));
            editButton.ToggleMode = true;

            AddChild(editButton);
            AddFocusable(editButton);
        }
        #endregion

        #region Elements
        private Array WorkingElements
        {
            get => (Array)workingValue;
            set => workingValue = value;
        }
        private void RemoveAndInsert(int from, int to)
        {
            if (from == to) return;

            var previous = WorkingElements.GetValue(from);
            if (from > to)
            {
                // to ---- from
                for (int i = to; i <= from; i++)
                {
                    var temp = WorkingElements.GetValue(i);
                    WorkingElements.SetValue(previous, i);
                    previous = temp;
                }
            }
            else
            {
                // from ---- to
                for (int i = to; i >= from; i--)
                {
                    var temp = WorkingElements.GetValue(i);
                    WorkingElements.SetValue(previous, i);
                    previous = temp;
                }
            }
        }

        private void RemoveAt(int index)
        {
            if (index < 0 || index >= WorkingElements.Length) return;
            var copy = Array.CreateInstance(elementType, WorkingElements.Length - 1);
            int j = 0;
            for (int i = 0; i < WorkingElements.Length; i++)
                if (index != i)
                    copy.SetValue(WorkingElements.GetValue(i), j++);
            WorkingElements = copy;
        }

        private void InsertAt(int index, object element)
        {
            var copy = Array.CreateInstance(elementType, WorkingElements.Length + 1);
            int j = 0;
            for (int i = 0; i < index; i++)
                copy.SetValue(WorkingElements.GetValue(i), j++);
            copy.SetValue(element, j++);
            for (int i = index; i < WorkingElements.Length; i++)
                copy.SetValue(WorkingElements.GetValue(i), j++);
            WorkingElements = copy;
        }

        private void Resize(int newSize)
        {
            var newPartition = Array.CreateInstance(elementType, newSize);
            for (int i = 0; i < newPartition.Length; i++)
            {
                // Move old data to new array. If the new array is bigger,
                // fill the empty space with default values for our element's
                // type.
                if (i < WorkingElements.Length)
                    newPartition.SetValue(WorkingElements.GetValue(i), i);
                else
                    newPartition.SetValue(inspectorPlugin.GetDefaultObject(elementHintArgs), i);
            }
            WorkingElements = newPartition;
        }
        #endregion

        #region Folding, Pagination, Updating
        private EditorSpinSlider sizeSpin;
        private EditorSpinSlider pageSpin;
        private VBoxContainer elementControlsContainer;
        private Button editButton;

        private int pageLength;
        private int pageIndex = 0;

        private EditorInterface editorInterface;

        private void OnEditPressed()
        {
            UpdateProperty();
        }

        private void OnSizeSpinChanged(int value)
        {
            if (updating || value == WorkingElements.Length) return;

            Resize(value);

            SerializeWorkingValueToEditor();
        }

        private void OnPageChanged(int value)
        {
            if (updating || pageIndex == value) return;

            pageIndex = value;

            UpdateProperty();
        }

        private void OnPropertyChanged(string property, object value, string field, bool changing)
        {
            if (updating) return;
            var index = int.Parse(property);
            var controlsIndex = index - pageIndex * pageLength;

            var elementControl = elementControlsContainer.GetChild(controlsIndex).GetChild(1);
            var serializedProp = (ISerializedEditorProperty)elementControl;
            WorkingElements.SetValue(serializedProp.Value, index);
            SerializeWorkingValueToEditor();
        }

        protected override void InternalUpdateProperty()
        {
            var dataCopy = Data;
            var buffer = new StreamPeerBuffer();
            buffer.PutData(dataCopy);
            buffer.Seek(0);
            var length = buffer.GetU32();
            editButton.Text = $"{elementType.Name} Array (size {length})";

            if (editButton.Pressed)
            {
                // Construct bottomVBox if it doesn't exist (ie. if the property was folded).
                if (bottomVBox == null)
                {
                    // Initialize
                    var sizeHBox = new HBoxContainer();

                    sizeSpin = new EditorSpinSlider();
                    sizeSpin.MinValue = 0;
                    sizeSpin.AllowGreater = true;
                    sizeSpin.AllowLesser = false;
                    sizeSpin.Connect("value_changed", this, nameof(OnSizeSpinChanged));
                    sizeSpin.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;

                    var sizeLabel = new Label();
                    sizeLabel.Text = "Size:";
                    sizeLabel.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;

                    sizeHBox.AddChild(sizeLabel);
                    sizeHBox.AddChild(sizeSpin);

                    var pageHBox = new HBoxContainer();

                    pageSpin = new EditorSpinSlider();
                    pageSpin.MinValue = 0;
                    pageSpin.Step = 1;
                    pageSpin.Connect("value_changed", this, nameof(OnPageChanged));
                    pageSpin.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;

                    var pageLabel = new Label();
                    pageLabel.Text = "Page:";
                    pageLabel.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;

                    pageHBox.AddChild(pageLabel);
                    pageHBox.AddChild(pageSpin);

                    elementControlsContainer = new VBoxContainer();

                    bottomVBox = new VBoxContainer();
                    bottomVBox.AddChild(sizeHBox);
                    bottomVBox.AddChild(pageHBox);
                    bottomVBox.AddChild(elementControlsContainer);
                    AddChild(bottomVBox);
                    SetBottomEditor(bottomVBox);
                }

                sizeSpin.Value = length;
                pageSpin.MaxValue = length / pageLength;
                WorkingElements = ArraySerializer.Global.Deserialize(elementType, Data);

                var start = pageIndex * pageLength;
                var end = start + pageLength;
                if (end > length)
                    end = (int)length;

                foreach (Node prop in elementControlsContainer.GetChildren())
                {
                    if (prop == reorderSelectedElementHBox) continue;
                    prop.QueueFree();
                    elementControlsContainer.RemoveChild(prop);
                }

                var props = new List<ExtendedEditorProperty>();
                for (int i = 0; i < length; i++)
                {
                    if (i >= start && i < end)
                    {
                        // Only add children for the page
                        if (Reordering)
                        {
                            bool reorderIsFromCurrentPage = reorderFromIndex / pageLength == pageIndex;
                            if (reorderIsFromCurrentPage && i == reorderFromIndex)
                            {
                                // Don't duplicate the property that the user is moving.
                                continue;
                            }
                            else if (!reorderIsFromCurrentPage && i == reorderToIndex)
                            {
                                // Don't create the property the moving property will take the place of
                                continue;
                            }
                        }

                        // Add editor properties
                        var prop = inspectorPlugin.GetEditorProperty(elementHintArgs);
                        if (!(prop is ISerializedEditorProperty serializedProp))
                        {
                            GD.PrintErr("VoltArrayEditorProperty: Expected element EditorProperty to be ISerializedEditorProperty.");
                            return;
                        }
                        serializedProp.UpdateProperty(WorkingElements.GetValue(i));
                        serializedProp.ConfigureOverrides(
                            useManualValue: true,
                            suppressFocusable: true,
                            manualEditedProperty: i.ToString(),
                            label: i.ToString()
                        );
                        serializedProp.UpdateProperty(WorkingElements.GetValue(i));
                        prop.Connect("property_changed", this, nameof(OnPropertyChanged));
                        props.Add(prop);

                        prop.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;

                        var theme = this.GetThemeFromAncestor();

                        var propHBox = new HBoxContainer();

                        var reorderButton = new Button();
                        reorderButton.Icon = theme.GetIcon("TripleBar", "EditorIcons");
                        reorderButton.MouseDefaultCursorShape = CursorShape.Move;
                        reorderButton.Connect("gui_input", this, nameof(OnReorderButtonGUIInput));
                        reorderButton.Connect("button_down", this, nameof(OnReorderButtonDown), new GDC.Array(i));
                        reorderButton.Connect("button_up", this, nameof(OnReorderButtonUp));

                        var addButton = new Button();
                        addButton.Icon = theme.GetIcon("Add", "EditorIcons");
                        addButton.Connect("pressed", this, nameof(OnAddButtonPressed), new GDC.Array(i));

                        var deleteButton = new Button();
                        deleteButton.Icon = theme.GetIcon("Remove", "EditorIcons");
                        deleteButton.Connect("pressed", this, nameof(OnDeleteButtonPressed), new GDC.Array(i));

                        var hFlow = new VFlowContainer();
                        hFlow.AddChild(addButton);
                        hFlow.AddChild(deleteButton);

                        propHBox.AddChild(reorderButton);
                        propHBox.AddChild(prop);
                        propHBox.AddChild(hFlow);

                        elementControlsContainer.AddChild(propHBox);
                    }
                }

                if (pageSpin.Value != pageIndex)
                    pageSpin.Value = pageIndex;
                // Position our moved element to the correct place.
                if (Reordering && reorderToIndex % pageLength > 0)
                    elementControlsContainer.MoveChild(elementControlsContainer.GetChild(0), reorderToIndex % pageLength);

                // Update element editor properties after the partitioned data is ready
                foreach (var prop in props)
                    prop.UpdateProperty();
            }
            else
            {
                if (bottomVBox != null)
                {
                    bottomVBox.QueueFree();
                    SetBottomEditor(null);
                    bottomVBox = null;
                    elementControlsContainer = null;
                    sizeSpin = null;
                }
            }
        }

        protected override object Deserialize(byte[] data) => ArraySerializer.Global.Deserialize(elementType, data);
        protected override byte[] Serialize(object workingValue) => ArraySerializer.Global.Serialize(elementType, (Array)workingValue);
        #endregion

        #region Reordering
        private bool Reordering => reorderFromIndex >= 0;
        private int reorderFromIndex = -1;
        private int reorderToIndex = -1;
        private float reorderMouseYDelta = 0;
        private Control reorderSelectedElementHBox;
        private Button reorderSelectedButton;

        private void OnReorderButtonGUIInput(InputEvent inputEvent)
        {
            if (!Reordering) return;
            if (inputEvent is InputEventMouseMotion mouseMotionEvent)
            {
                var size = WorkingElements.Length;

                // Cumulative mouse delta
                reorderMouseYDelta += mouseMotionEvent.Relative.y;

                // If you are out of array bounds, reset the cumulated mouse delta
                if ((reorderToIndex == 0 && reorderMouseYDelta < 0) || (reorderToIndex == size - 1 && reorderMouseYDelta > 0))
                {
                    reorderMouseYDelta = 0;
                    return;
                }

                var requiredYDistance = 20.0f * editorInterface.GetEditorScale();
                if (Mathf.Abs(reorderMouseYDelta) > requiredYDistance)
                {
                    int direction = reorderMouseYDelta > 0 ? 1 : -1;
                    reorderMouseYDelta -= requiredYDistance * direction;

                    reorderToIndex += direction;
                    if ((direction < 0 && reorderToIndex % pageLength == pageLength - 1)
                        || (direction > 0 && reorderToIndex % pageLength == 0))
                    {
                        // Automatically move to the next/previous page
                        OnPageChanged(pageIndex + direction);
                    }
                    elementControlsContainer.MoveChild(reorderSelectedElementHBox, reorderToIndex % pageLength);
                    editorInterface.GetInspector().EnsureControlVisible(reorderSelectedElementHBox);
                }
            }
        }

        private void OnReorderButtonDown(int index)
        {
            reorderFromIndex = index;
            reorderToIndex = index;
            reorderSelectedElementHBox = elementControlsContainer.GetChild<Control>(index % pageLength);
            reorderSelectedButton = reorderSelectedElementHBox.GetChild<Button>(0);
            // Might have to set to invisible if this doesn't work
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }

        private void OnReorderButtonUp()
        {
            if (reorderFromIndex != reorderToIndex)
            {
                RemoveAndInsert(reorderFromIndex, reorderToIndex);
                SerializeWorkingValueToEditor();
            }

            reorderFromIndex = -1;
            reorderToIndex = -1;
            reorderMouseYDelta = 0;

            Input.MouseMode = Input.MouseModeEnum.Visible;
            reorderSelectedButton.WarpMouse(reorderSelectedButton.RectSize / 2f);

            reorderSelectedElementHBox = null;
            reorderSelectedButton = null;
        }
        #endregion

        #region Item Buttons
        private void OnAddButtonPressed(int index)
        {
            InsertAt(index, inspectorPlugin.GetDefaultObject(elementHintArgs));
            SerializeWorkingValueToEditor();
        }

        private void OnDeleteButtonPressed(int index)
        {
            RemoveAt(index);
            SerializeWorkingValueToEditor();
        }
        #endregion
    }

    [Tool]
    public class VoltArrayEditorPropertyParser : SerializedEditorPropertyParser
    {
        private int itemsPerPage;
        private VoltTypesInspectorPlugin inspectorPlugin;
        private EditorInterface editorInterface;

        public VoltArrayEditorPropertyParser() { }
        public VoltArrayEditorPropertyParser(EditorInterface editorInterface, VoltTypesInspectorPlugin inspectorPlugin, int itemsPerPage)
        {
            this.editorInterface = editorInterface;
            this.inspectorPlugin = inspectorPlugin;
            this.itemsPerPage = itemsPerPage;
        }

        // Array hintText is formatted as
        // Array,[ElementType]
        //
        // ie.
        //  Array,VoltVector2
        //  Array,Fix64
        //
        // Note that you can add additional arguments afterwards to modify the element
        //
        // ie.
        //  Array,Fix64,0,100   ---> Array of Fix64 with each having a range from 0 to 100
        //
        public override ISerializedEditorProperty ParseSerializedProperty(string[] args)
        {
            if (args.TryGet(0) == VoltPropertyHint.Array && args.Length >= 2)
                return new VoltArrayEditorProperty(args.Skip(1).ToArray(), editorInterface, inspectorPlugin, itemsPerPage);
            return null;
        }

        public override object GetDefaultObject(string[] args)
        {
            if (args.TryGet(0) == VoltPropertyHint.Array)
            {
                if (VoltPropertyHint.HintToType.TryGetValue(args.TryGet(1), out System.Type type))
                    return ArraySerializer.Global.Default(type);
            }
            return null;
        }
    }
}
#endif