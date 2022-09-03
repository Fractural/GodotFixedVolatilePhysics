using Fractural.Plugin;
using Godot;
using System.Linq;

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

        private Anchor[] anchors;
        private VolatilePolygon editedVolatilePolygon;
        private VolatilePolygon EditedVolatilePolygon
        {
            get
            {
                if (!IsInstanceValid(editedVolatilePolygon))
                    editedVolatilePolygon = null;
                return editedVolatilePolygon;
            }
            set => editedVolatilePolygon = value;
        }

        public override void Edit(Godot.Object @object)
        {
            if (@object is VolatilePolygon shape)
            {
                EditedVolatilePolygon = shape;
                EditedVolatilePolygon.Connect(nameof(VolatileShape.EditingChanged), this, nameof(OnEditingChanged));
            }
        }

        private void OnEditingChanged(bool editing)
        {
            Plugin.UpdateOverlays();
        }

        public override void MakeVisible(bool visible)
        {
            if (!visible && EditedVolatilePolygon != null)
            {
                // Clean up
                EditedVolatilePolygon.Disconnect(nameof(VolatileShape.EditingChanged), this, nameof(OnEditingChanged));
                EditedVolatilePolygon = null;
            }
            Plugin.UpdateOverlays();
        }

        public override bool Handles(Object @object)
        {
            return @object is VolatilePolygon polygon;
        }

        const float CIRCLE_RADIUS = 6;
        const float STROKE_RADIUS = 2;
        readonly Color STROKE_COLOR = Palette.Main;
        readonly Color FILL_COLOR = Palette.Blank;

        private bool HasEditableVolatilePolygon => EditedVolatilePolygon != null && EditedVolatilePolygon.Editing && EditedVolatilePolygon.Visible;

        public override void ForwardCanvasDrawOverViewport(Control overlay)
        {
            if (!HasEditableVolatilePolygon) return;

            var transformViewport = EditedVolatilePolygon.GetViewportTransform();
            var transformGlobal = EditedVolatilePolygon.GetCanvasTransform();
            var points = EditedVolatilePolygon.EditorGDPoints;
            anchors = new Anchor[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                var anchorPosition = transformViewport * (transformGlobal * (EditedVolatilePolygon.Transform * points[i]));
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
            if (!HasEditableVolatilePolygon) return false;

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
                        undo.AddUndoProperty(EditedVolatilePolygon, nameof(EditedVolatilePolygon._points), EditedVolatilePolygon._points);
                        draggedAnchor = anchor;
                        return true;
                    }
                }
                else if (draggedAnchor != null && !mouseButtonEvent.Pressed)
                {
                    // End Drag
                    DragTo(mouseButtonEvent.Position, true);
                    draggedAnchor = null;
                    var undo = Plugin.GetUndoRedo();
                    undo.AddDoProperty(EditedVolatilePolygon, nameof(EditedVolatilePolygon._points), EditedVolatilePolygon._points);
                    undo.CommitAction();
                    return true;
                }
            }

            if (draggedAnchor == null) return false;
            if (@event is InputEventMouseMotion mouseMotionEvent)
            {
                // Dragging
                DragTo(mouseMotionEvent.Position);
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

        public void DragTo(Vector2 eventPosition, bool commit = false)
        {
            var inverseTransformViewport = EditedVolatilePolygon.GetViewportTransform().AffineInverse();
            var inverseTransformGlobal = EditedVolatilePolygon.GetCanvasTransform().AffineInverse();
            var inverseTransformPolygon = EditedVolatilePolygon.Transform.AffineInverse();

            var draggedAnchorNewPoint = (inverseTransformPolygon * (inverseTransformGlobal * (inverseTransformViewport * eventPosition)));

            if (commit)
            {
                // Commiting is only done when the drag stops, because it's really costly
                var points = (VoltVector2[])EditedVolatilePolygon.Points.Clone();
                points[draggedAnchor.index] = draggedAnchorNewPoint.ToVoltVector2();
                EditedVolatilePolygon.Points = points;
                EditedVolatilePolygon.PropertyListChangedNotify();
            }
            else
            {
                EditedVolatilePolygon.EditorGDPoints[draggedAnchor.index] = draggedAnchorNewPoint;
                EditedVolatilePolygon.Update();
            }

            draggedAnchor.position = eventPosition;
            Plugin.UpdateOverlays();
        }
    }
}
#endif