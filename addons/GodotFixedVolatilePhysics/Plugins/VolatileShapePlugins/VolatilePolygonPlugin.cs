using Fractural.Plugin;
using Godot;

#if TOOLS
namespace Volatile.GodotEngine.Plugin
{
    [Tool]
    public class VolatilePolygonPlugin : SubPlugin
    {
        public override string PluginName => nameof(VolatilePolygonPlugin);

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
        VolatilePolygon editedVolatilePolygon;

        public override void Edit(Godot.Object @object)
        {
            if (@object is VolatilePolygon shape)
                editedVolatilePolygon = shape;
        }

        public override void MakeVisible(bool visible)
        {
            if (editedVolatilePolygon == null) return;
            if (!visible) editedVolatilePolygon = null;
            Plugin.UpdateOverlays();
        }

        public override bool Handles(Object @object)
        {
            return @object is VolatilePolygon;
        }

        const float CIRCLE_RADIUS = 6;
        const float STROKE_RADIUS = 2;
        readonly Color STROKE_COLOR = Colors.DeepPink;
        readonly Color FILL_COLOR = Colors.White;

        public override void ForwardCanvasDrawOverViewport(Control overlay)
        {
            if (editedVolatilePolygon == null || !editedVolatilePolygon.IsInsideTree()) return;

            var transformViewport = editedVolatilePolygon.GetViewportTransform();
            var transformGlobal = editedVolatilePolygon.GetCanvasTransform();
            var points = editedVolatilePolygon._Points;
            anchors = new Anchor[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                var anchorPosition = transformViewport * (transformGlobal * (points[i] + editedVolatilePolygon.GlobalPosition));
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
            if (editedVolatilePolygon == null || !editedVolatilePolygon.Visible) return false;

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
                        undo.AddUndoProperty(editedVolatilePolygon, nameof(editedVolatilePolygon._Points), editedVolatilePolygon._Points);
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
                    undo.AddDoProperty(editedVolatilePolygon, nameof(editedVolatilePolygon._Points), editedVolatilePolygon._Points);
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
            var inverseTransformViewport = editedVolatilePolygon.GetViewportTransform().AffineInverse();
            var inverseTransformGlobal = editedVolatilePolygon.GetCanvasTransform().AffineInverse();
            var points = (Vector2[])editedVolatilePolygon._Points.Clone();
            points[draggedAnchor.index] = inverseTransformViewport * (inverseTransformGlobal * (eventPosition - editedVolatilePolygon.GlobalPosition));
            editedVolatilePolygon._Points = points;
            editedVolatilePolygon.PropertyListChangedNotify();
            draggedAnchor.position = eventPosition;
            Plugin.UpdateOverlays();
        }
    }
}
#endif