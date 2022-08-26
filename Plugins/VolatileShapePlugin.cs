using Fractural.Plugin;
using Godot;

#if TOOLS
namespace GodotFixedVolatilePhysics.Plugin
{
    [Tool]
    public class VolatileShapePlugin : SubPlugin
    {
        public override string PluginName => nameof(VolatileShapePlugin);

        public class Anchor : Godot.Reference
        {
            public int index;
            public Vector2 position;
            public Rect2 rect;

            public Anchor() { }
            public Anchor(int index, Vector2 position, Rect2 rect)
            {
                this.index = index;
                this.position = position;
                this.rect = rect;
            }
        }

        Anchor[] anchors;
        VolatileShape editedVolatileShape;

        public override void Edit(Godot.Object @object)
        {
            if (@object is VolatileShape shape)
                editedVolatileShape = shape;
        }

        public override void MakeVisible(bool visible)
        {
            if (editedVolatileShape == null) return;
            if (!visible) editedVolatileShape = null;
            Plugin.UpdateOverlays();
        }

        public override bool Handles(Object @object)
        {
            return @object is VolatileShape;
        }

        const float CIRCLE_RADIUS = 6;
        const float STROKE_RADIUS = 2;
        readonly Color STROKE_COLOR = Colors.DeepPink;
        readonly Color FILL_COLOR = Colors.White;

        public override void ForwardCanvasDrawOverViewport(Control overlay)
        {
            if (editedVolatileShape == null || !editedVolatileShape.IsInsideTree()) return;

            var transformViewport = editedVolatileShape.GetViewportTransform();
            var transformGlobal = editedVolatileShape.GetCanvasTransform();
            var points = editedVolatileShape._Points;
            anchors = new Anchor[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                var anchorPosition = transformViewport * (transformGlobal * (points[i] + editedVolatileShape.GlobalPosition));
                var anchorSize = Vector2.One * CIRCLE_RADIUS * 2f;
                anchors[i] = new Anchor(i, anchorPosition, new Rect2(anchorPosition - anchorSize / 2f, anchorSize));
                DrawAnchor(overlay, anchors[i]);
            }
        }

        public void DrawAnchor(Control overlay, Anchor anchor)
        {
            overlay.DrawCircle(anchor.position, CIRCLE_RADIUS + STROKE_RADIUS, STROKE_COLOR);
            overlay.DrawCircle(anchor.position, CIRCLE_RADIUS, FILL_COLOR);
        }

        public Anchor draggedAnchor;

        public override bool ForwardCanvasGuiInput(InputEvent @event)
        {
            if (editedVolatileShape == null || !editedVolatileShape.Visible) return false;

            if (@event is InputEventMouseButton mouseButtonEvent && mouseButtonEvent.ButtonIndex == (int)ButtonList.Left)
            {
                if (draggedAnchor == null && mouseButtonEvent.Pressed)
                {
                    // Start Drag
                    foreach (var anchor in anchors)
                    {
                        if (!anchor.rect.HasPoint(mouseButtonEvent.Position)) continue;
                        var undo = Plugin.GetUndoRedo();
                        undo.CreateAction("Move anchor");
                        undo.AddUndoProperty(editedVolatileShape, nameof(editedVolatileShape._Points), editedVolatileShape._Points);
                        draggedAnchor = anchor;
                        return true;
                    }
                }
                else if (draggedAnchor != null && !mouseButtonEvent.Pressed)
                {
                    // End Drag
                    DragTo(mouseButtonEvent.Position);
                    draggedAnchor = null;
                    var undo = Plugin.GetUndoRedo();
                    undo.AddDoProperty(editedVolatileShape, nameof(editedVolatileShape._Points), editedVolatileShape._Points);
                    undo.CommitAction();
                    return true;
                }
            }

            if (draggedAnchor == null) return false;
            if (@event is InputEventMouseMotion mouseMotionEvent)
            {
                // Dragging
                DragTo(mouseMotionEvent.Position);
                Plugin.UpdateOverlays();
                return true;
            }
            if (@event.IsActionPressed("ui_cancel"))
            {
                // Cancel
                draggedAnchor = null;
                var undo = Plugin.GetUndoRedo();
                undo.CommitAction();
                undo.Undo();
                Plugin.UpdateOverlays();
                return true;
            }
            return false;
        }

        public void DragTo(Vector2 eventPosition)
        {
            var inverseTransformViewport = editedVolatileShape.GetViewportTransform().AffineInverse();
            var inverseTransformGlobal = editedVolatileShape.GetCanvasTransform().AffineInverse();
            var points = (Vector2[])editedVolatileShape._Points.Clone();
            points[draggedAnchor.index] = inverseTransformViewport * (inverseTransformGlobal * (eventPosition - editedVolatileShape.GlobalPosition));
            editedVolatileShape._Points = points;
            draggedAnchor.position = eventPosition;
            Plugin.UpdateOverlays();
        }
    }
}
#endif