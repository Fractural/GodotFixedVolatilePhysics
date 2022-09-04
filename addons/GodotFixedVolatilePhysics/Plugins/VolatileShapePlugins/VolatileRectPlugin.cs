using FixMath.NET;
using Godot;

#if TOOLS
namespace Volatile.GodotEngine.Plugin
{
    [Tool]
    public class VolatileRectPlugin : PointsEditorPlugin
    {
        public override string PluginName => nameof(VolatileRectPlugin);

        public class ExtentsPointAnchor : Anchor
        {
            public VolatileRect editedRect;

            public override void RegisterUndo(UndoRedo undo)
            {
                undo.CreateAction("Move anchor");
                undo.AddUndoProperty(editedRect, nameof(editedRect._extents), editedRect._extents);
            }

            public override void CommitUndo(UndoRedo undo)
            {
                undo.AddDoProperty(editedRect, nameof(editedRect._extents), editedRect._extents);
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
                    var extents = new VoltVector2(
                        Fix64.Abs(fixedLocalPosition.x),
                        Fix64.Abs(fixedLocalPosition.y)
                    );
                    editedRect.Extents = extents;
                    editedRect.PropertyListChangedNotify();
                }
                else
                {
                    editedRect.EditorExtents = new Vector2(
                        Mathf.Abs(localPosition.x),
                        Mathf.Abs(localPosition.y)
                    );
                    editedRect.Update();
                }
            }
        }

        protected VolatileRect EditedVolatileRect
        {
            get => (VolatileRect)EditedTarget;
            set => EditedTarget = value;
        }

        protected override bool HasEditableTarget => EditedVolatileRect != null && EditedVolatileRect.Editing;

        public override bool Handles(Object @object)
        {
            return @object is VolatileRect;
        }

        public override void Edit(Godot.Object @object)
        {
            if (@object is VolatileRect shape)
            {
                EditedVolatileRect = shape;
                EditedVolatileRect.Connect(nameof(VolatileShape.EditingChanged), this, nameof(OnEditingChanged));
            }
        }

        private void OnEditingChanged(bool editing)
        {
            Plugin.UpdateOverlays();
        }

        public override void MakeVisible(bool visible)
        {
            if (!visible && EditedVolatileRect != null)
            {
                // Clean up
                EditedVolatileRect.Disconnect(nameof(VolatileShape.EditingChanged), this, nameof(OnEditingChanged));
                EditedVolatileRect = null;
            }
            Plugin.UpdateOverlays();
        }

        public override void AddAndDrawAnchors()
        {
            var transform = LocalToViewportTransform;
            var extents = EditedVolatileRect.EditorExtents;

            // Corners of the rect
            AddAndDrawAnchor(new ExtentsPointAnchor()
            {
                position = transform * extents,
                editedRect = EditedVolatileRect
            });
            AddAndDrawAnchor(new ExtentsPointAnchor()
            {
                position = transform * new Vector2(-extents.x, extents.y),
                editedRect = EditedVolatileRect
            });
            AddAndDrawAnchor(new ExtentsPointAnchor()
            {
                position = transform * new Vector2(-extents.x, -extents.y),
                editedRect = EditedVolatileRect
            });
            AddAndDrawAnchor(new ExtentsPointAnchor()
            {
                position = transform * new Vector2(extents.x, -extents.y),
                editedRect = EditedVolatileRect
            });
        }
    }
}
#endif