using Fractural.Utils;
using Godot;
using System.Collections.Generic;
using System.Linq;
using GDC = Godot.Collections;

#if TOOLS
namespace Volatile.GodotEngine.Plugin
{
    [Tool]
    /// <summary>
    /// Dummy class that's passed into EditorProperties in order to let them access elements from an array.
    /// </summary>
    public class VoltArrayPartitionedDataObject : Godot.Reference
    {
        [Signal]
        public delegate void ArrayChanged(object[] newArray);

        // All VoltTypes will be stored in bytes[]
        public byte[][] PartitionedData { get; set; } = new byte[0][];


        public VoltArrayPartitionedDataObject() { }

        public override object _Get(string property)
        {
            if (property.BeginsWith("indices"))
            {
                var index = int.Parse(property.Split('/')[1]);

                return PartitionedData[index];
            }
            return base._Get(property);
        }

        public override bool _Set(string property, object value)
        {
            if (property.BeginsWith("indices"))
            {
                var index = int.Parse(property.Split('/')[1]);
                PartitionedData[index] = (byte[])value;

                return true;
            }
            return base._Set(property, value);
        }

        public byte[] GetConsolidatedData()
        {
            var buffer = new StreamPeerBuffer();
            buffer.PutU32((uint)PartitionedData.Length);
            for (int i = 0; i < PartitionedData.Length; i++)
            {
                buffer.PutU8((byte)PartitionedData[i].Length);
                buffer.PutData(PartitionedData[i]);
            }
            return buffer.DataArray;
        }

        public void RemoveAndInsert(int from, int to)
        {
            if (from == to) return;

            var previous = PartitionedData[from];
            if (from > to)
            {
                // to ---- from
                for (int i = to; i <= from; i++)
                {
                    var temp = PartitionedData[i];
                    PartitionedData[i] = previous;
                    previous = temp;
                }
            }
            else
            {
                // from ---- to
                for (int i = to; i >= from; i--)
                {
                    var temp = PartitionedData[i];
                    PartitionedData[i] = previous;
                    previous = temp;
                }
            }
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= PartitionedData.Length) return;
            var copy = new byte[PartitionedData.Length - 1][];
            int j = 0;
            for (int i = 0; i < PartitionedData.Length; i++)
                if (index != i)
                    copy[j++] = PartitionedData[i];
            PartitionedData = copy;
        }

        public void InsertAt(int index, byte[] partition)
        {
            var copy = new byte[PartitionedData.Length + 1][];
            int j = 0;
            for (int i = 0; i < index; i++)
                copy[j++] = PartitionedData[i];
            copy[j++] = partition;
            for (int i = index; i < PartitionedData.Length; i++)
                copy[j++] = PartitionedData[i];
            PartitionedData = copy;
        }
    }

    /// <summary>
    /// Custom EditorProperty for an array of Volt types. This only supports
    /// Arrays of the same type.
    /// </summary>
    [Tool]
    public class VoltArrayEditorProperty : ExtendedEditorProperty
    {

        private string[] elementTypeArgs;
        private string ElementType => elementTypeArgs[0];

        protected byte[] Data => GetEditedObject().Get(GetEditedProperty()) as byte[];
        protected VoltArrayPartitionedDataObject DataObject { get; } = new VoltArrayPartitionedDataObject();

        private EditorSpinSlider sizeSpin;
        private EditorSpinSlider pageSpin;
        private VBoxContainer elementControlsContainer;
        private Button editButton;
        private VBoxContainer bottomVBox;

        private int pageLength;
        private int pageIndex = 0;

        private VoltTypesInspectorPlugin inspectorPlugin;
        private EditorInterface editorInterface;

        public VoltArrayEditorProperty() { }
        public VoltArrayEditorProperty(string[] elementTypeArgs, EditorInterface editorInterface, VoltTypesInspectorPlugin inspectorPlugin, int itemsPerPage)
        {
            this.editorInterface = editorInterface;
            this.inspectorPlugin = inspectorPlugin;
            this.pageLength = itemsPerPage;
            this.elementTypeArgs = elementTypeArgs;

            editButton = new Button();
            editButton.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            editButton.ClipText = true;
            editButton.Connect("pressed", this, nameof(OnEditPressed));
            editButton.ToggleMode = true;

            AddChild(editButton);
            AddFocusable(editButton);
        }

        private void OnEditPressed()
        {
            UpdateProperty();
        }

        private void OnSizeSpinChanged(double value)
        {
            if (updating || ((int)value) == DataObject.PartitionedData.Length) return;

            var newPartition = new byte[(int)value][];
            for (int i = 0; i < newPartition.Length; i++)
            {
                // Move old data to new array. If the new array is bigger,
                // fill the empty space with default values for our element's
                // type.
                if (i < DataObject.PartitionedData.Length)
                    newPartition[i] = DataObject.PartitionedData[i];
                else
                    newPartition[i] = inspectorPlugin.GetDefaultBytesForType(ElementType);
            }
            DataObject.PartitionedData = newPartition;

            EmitChanged();
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
            if (property.BeginsWith("indices"))
            {
                DataObject._Set(property, value);
                EmitChanged();
            }
        }

        protected override void InternalUpdateProperty()
        {
            var dataCopy = Data;
            var buffer = new StreamPeerBuffer();
            buffer.PutData(dataCopy);
            buffer.Seek(0);
            var length = buffer.GetU32();
            editButton.Text = $"{ElementType} Array (size {length})";

            if (editButton.Pressed)
            {
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
                var partitionedData = new byte[length][];

                var start = pageIndex * pageLength;
                var end = start + pageLength;
                if (end > length)
                    end = (int)length;

                foreach (Node prop in elementControlsContainer.GetChildren())
                {
                    if (prop == reorderSelectedElementHBox) continue;
                    prop.Free();
                }

                var props = new List<ExtendedEditorProperty>();
                for (int i = 0; i < length; i++)
                {
                    // Populate partitioned data
                    var elementByteLength = buffer.GetU8();
                    partitionedData[i] = new byte[elementByteLength];
                    for (int j = 0; j < elementByteLength; j++)
                        partitionedData[i][j] = buffer.GetU8();

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
                        var prop = inspectorPlugin.GetEditorProperty(elementTypeArgs);
                        prop.ManualEditedObject = DataObject;
                        prop.ManualEditedProperty = $"indices/{i}";
                        prop.Connect("property_changed", this, nameof(OnPropertyChanged));
                        prop.Label = i.ToString();
                        props.Add(prop);

                        prop.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;

                        var theme = this.GetThemeFromParents();

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

                        propHBox.AddChild(reorderButton);
                        propHBox.AddChild(prop);
                        propHBox.AddChild(addButton);
                        propHBox.AddChild(deleteButton);

                        elementControlsContainer.AddChild(propHBox);
                    }
                }

                if (pageSpin.Value != pageIndex)
                    pageSpin.Value = pageIndex;
                // Position our moved element to the correct place.
                if (Reordering && reorderToIndex % pageLength > 0)
                    elementControlsContainer.MoveChild(elementControlsContainer.GetChild(0), reorderToIndex % pageLength);

                DataObject.PartitionedData = partitionedData;

                // Update element editor properties after the partitioned data is ready
                foreach (var prop in props)
                    prop.UpdateProperty();
            }
            else
            {
                if (bottomVBox != null)
                {
                    bottomVBox.Free();
                    SetBottomEditor(null);
                    bottomVBox = null;
                    elementControlsContainer = null;
                    sizeSpin = null;
                }
            }
        }

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
                var size = DataObject.PartitionedData.Length;

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
                DataObject.RemoveAndInsert(reorderFromIndex, reorderToIndex);
                EmitChanged();
            }

            reorderFromIndex = -1;
            reorderToIndex = -1;
            reorderMouseYDelta = 0;

            Input.MouseMode = Input.MouseModeEnum.Visible;
            reorderSelectedButton.WarpMouse(reorderSelectedButton.RectSize / 2f);

            reorderSelectedElementHBox = null;
            reorderSelectedButton = null;
        }

        private void OnAddButtonPressed(int index)
        {
            DataObject.InsertAt(index, inspectorPlugin.GetDefaultBytesForType(ElementType));
            EmitChanged();
        }

        private void OnDeleteButtonPressed(int index)
        {
            DataObject.RemoveAt(index);
            EmitChanged();
        }

        private void EmitChanged()
        {
            EmitChanged(GetEditedProperty(), DataObject.GetConsolidatedData());
        }
    }

    [Tool]
    public class VoltArrayEditorPropertyParser : ExtendedEditorPropertyParser
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
        public override ExtendedEditorProperty ParseProperty(string[] args)
        {
            if (args.TryGet(0) == VoltPropertyHint.Array && args.Length >= 2)
                return new VoltArrayEditorProperty(args.Skip(1).ToArray(), editorInterface, inspectorPlugin, itemsPerPage);
            return null;
        }

        public override byte[] GetDefaultBytes(string type)
        {
            return null;
        }
    }
}
#endif