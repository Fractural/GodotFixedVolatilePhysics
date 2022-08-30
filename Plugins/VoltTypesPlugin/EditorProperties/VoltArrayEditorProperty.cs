using Fractural.Utils;
using Godot;
using System.Collections.Generic;
using System.Linq;

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
        public int IndexOffset { get; set; } = 0;

        public VoltArrayPartitionedDataObject() { }

        public override object _Get(string property)
        {
            if (property.BeginsWith("indices"))
            {
                //GD.Print("getting proc");
                var index = int.Parse(property.Split('/')[1]) - IndexOffset;

                //GD.Print("getting index[" + index + "]");
                return PartitionedData[index];
            }
            return base._Get(property);
        }

        public override bool _Set(string property, object value)
        {
            if (property.BeginsWith("indices"))
            {
                GD.Print("setting proc");
                var index = int.Parse(property.Split('/')[1]) - IndexOffset;
                PartitionedData[index] = (byte[])value;

                GD.Print("setting index[" + index + "]");
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

        private int itemsPerPage;
        private int pageIndex = 0;

        private VoltTypesInspectorPlugin plugin;

        public VoltArrayEditorProperty() { }
        public VoltArrayEditorProperty(string[] elementTypeArgs, VoltTypesInspectorPlugin plugin, int itemsPerPage)
        {
            this.plugin = plugin;
            this.itemsPerPage = itemsPerPage;
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
                    newPartition[i] = plugin.GetDefaultBytesForType(ElementType);
            }
            DataObject.PartitionedData = newPartition;

            EmitChanged(GetEditedProperty(), DataObject.GetConsolidatedData());
        }

        private void OnPageChanged(double value)
        {
            pageIndex = (int)value;
            DataObject.IndexOffset = pageIndex * itemsPerPage;

            UpdateProperty();
        }

        private void OnPropertyChanged(string property, object value, string field, bool changing)
        {
            if (updating) return;
            if (property.BeginsWith("indices"))
            {
                DataObject._Set(property, value);
                EmitChanged(GetEditedProperty(), DataObject.GetConsolidatedData());
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
                pageSpin.MaxValue = length / itemsPerPage;
                var partitionedData = new byte[length][];

                var start = pageIndex * itemsPerPage;
                var end = start + itemsPerPage;
                if (end > length)
                    end = (int)length;

                foreach (Node prop in elementControlsContainer.GetChildren())
                    prop.Free();
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

                        // Add editor properties
                        var prop = plugin.GetEditorProperty(elementTypeArgs);
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
                        var deleteButton = new Button();
                        deleteButton.Icon = theme.GetIcon("Remove", "EditorIcons");

                        propHBox.AddChild(reorderButton);
                        propHBox.AddChild(prop);
                        propHBox.AddChild(deleteButton);

                        elementControlsContainer.AddChild(propHBox);
                    }
                }

                DataObject.PartitionedData = partitionedData;

                // Update element editor properties after the partitioned data is ready
                foreach (var prop in props)
                    prop.UpdateProperty();
            }
            else
            {
                if (bottomVBox != null)
                {

                    GD.Print("out 1");
                    bottomVBox.Free();
                    SetBottomEditor(null);
                    bottomVBox = null;
                    elementControlsContainer = null;
                    sizeSpin = null;
                    GD.Print("out 2");
                }

            }
        }
    }

    public class VoltArrayEditorPropertyParser : ExtendedEditorPropertyParser
    {
        public int itemsPerPage;
        public VoltTypesInspectorPlugin plugin;

        public VoltArrayEditorPropertyParser() { }
        public VoltArrayEditorPropertyParser(VoltTypesInspectorPlugin plugin, int itemsPerPage)
        {
            this.plugin = plugin;
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
                return new VoltArrayEditorProperty(args.Skip(1).ToArray(), plugin, itemsPerPage);
            return null;
        }

        public override byte[] GetDefaultBytes(string type)
        {
            return null;
        }
    }
}
#endif