using Godot;

#if TOOLS
namespace Volatile.GodotEngine.Plugin
{
    [Tool]
    public class VolatileCirclePlugin : VolatileShapeEditorPlugin
    {
        public override string PluginName => nameof(VolatileCirclePlugin);

        public class RadiusPointAnchor : Anchor
        {
            public VolatileCircle editedCircle;

            public override void RegisterUndo(UndoRedo undo)
            {
                undo.CreateAction("Move anchor");
                undo.AddUndoProperty(editedCircle, nameof(editedCircle._radius), editedCircle._radius);
            }

            public override void CommitUndo(UndoRedo undo)
            {
                undo.AddDoProperty(editedCircle, nameof(editedCircle._radius), editedCircle._radius);
                undo.CommitAction();
            }

            public override void DragTo(Vector2 localPosition, bool commit)
            {
                if (commit)
                {
                    // Local position accounts for rotation
                    // Commiting is only done when the drag stops, because it's really costly
                    var fixedLocalPosition = localPosition.ToVoltVector2();
                    // We only care about the distance each point is from the local center (0,0)
                    var radius = fixedLocalPosition.x;
                    editedCircle.Radius = radius;
                    editedCircle.PropertyListChangedNotify();
                }
                else
                {
                    editedCircle.EditorRadius = localPosition.x;
                    editedCircle.Update();
                }
            }
        }

        protected VolatileCircle EditedVolatileCircle
        {
            get => (VolatileCircle)EditedTarget;
            set => EditedTarget = value;
        }

        protected override bool HasEditableTarget => EditedVolatileCircle != null && EditedVolatileCircle.Editing;

        public override bool Handles(Object @object)
        {
            return @object is VolatileCircle;
        }

        public override void Edit(Godot.Object @object)
        {
            if (@object is VolatileCircle shape)
            {
                EditedVolatileCircle = shape;
                EditedVolatileCircle.Connect(nameof(VolatileShape.EditingChanged), this, nameof(OnEditingChanged));
            }
        }

        private void OnEditingChanged(bool editing)
        {
            Plugin.UpdateOverlays();
        }

        public override void MakeVisible(bool visible)
        {
            if (!visible && EditedVolatileCircle != null)
            {
                // Clean up
                EditedVolatileCircle.Disconnect(nameof(VolatileShape.EditingChanged), this, nameof(OnEditingChanged));
                EditedVolatileCircle = null;
            }
            Plugin.UpdateOverlays();
        }

        public override void AddAndDrawAnchors()
        {
            var transform = LocalToViewportTransform;
            var radius = EditedVolatileCircle.EditorRadius;

            // Corners of the rect
            AddAndDrawAnchor(new RadiusPointAnchor()
            {
                position = transform * new Vector2(radius, 0),
                editedCircle = EditedVolatileCircle,
                movementMode = Anchor.MovementMode.XAxis,
                xRange = new Vector2(0.01f, Mathf.Inf)
            });
        }
    }
}
#endif