using Fractural.Plugin;
using Godot;
using GDC = Godot.Collections;

#if TOOLS
namespace Volatile.GodotEngine.Plugin
{
    public abstract class PointsEditorPlugin : SubPlugin
    {
        public abstract class Anchor : Godot.Reference
        {
            public Vector2 position;
            public Rect2 rect;
            public enum MovementMode
            {
                Freeform,
                XAxis,
                YAxis
            }
            public MovementMode movementMode;
            public Vector2 xRange;
            public Vector2 yRange;

            public abstract void RegisterUndo(UndoRedo undo);
            public abstract void CommitUndo(UndoRedo undo);
            public abstract void DragTo(Vector2 localPosition, bool commit);
        }

        private GDC.Array<Anchor> anchors = new GDC.Array<Anchor>();

        public override bool Handles(Object @object)
        {
            return @object is VolatilePolygon polygon;
        }

        const float CIRCLE_RADIUS = 6;
        const float STROKE_RADIUS = 2;
        readonly Color STROKE_COLOR = Palette.Main;
        readonly Color FILL_COLOR = Palette.Blank;

        private Node2D editedTarget;
        protected Node2D EditedTarget
        {
            get
            {
                if (!IsInstanceValid(editedTarget))
                    editedTarget = null;
                return editedTarget;
            }
            set
            {
                editedTarget = value;
            }
        }
        protected virtual bool HasEditableTarget => EditedTarget != null;
        protected Transform2D LocalToViewportTransform
            => EditedTarget.GetViewportTransform() * (EditedTarget.GetCanvasTransform() * EditedTarget.Transform);
        protected Transform2D ViewportToLocalTransform
            => EditedTarget.Transform.AffineInverse() * (EditedTarget.GetCanvasTransform().AffineInverse() * EditedTarget.GetViewportTransform().AffineInverse());

        private Control overlay;
        public override void ForwardCanvasDrawOverViewport(Control overlay)
        {
            if (!HasEditableTarget) return;

            this.overlay = overlay;
            anchors.Clear();
            AddAndDrawAnchors();
        }

        public abstract void AddAndDrawAnchors();

        public void AddAndDrawAnchor(Anchor anchor) => AddAndDrawAnchor(anchor, CIRCLE_RADIUS, STROKE_RADIUS, FILL_COLOR, STROKE_COLOR);
        public void AddAndDrawAnchor(Anchor anchor, float radius, float strokeRadius, Color fillColor, Color strokeColor)
        {
            var anchorSize = Vector2.One * radius * 2f;
            anchor.rect = new Rect2(anchor.position - anchorSize / 2f, anchorSize);

            anchors.Add(anchor);
            overlay.DrawCircle(anchor.position, radius + strokeRadius, strokeColor);
            overlay.DrawCircle(anchor.position, radius, fillColor);
        }

        public Anchor draggedAnchor;

        public override bool ForwardCanvasGuiInput(InputEvent @event)
        {
            if (!HasEditableTarget) return false;

            if (@event is InputEventMouseButton mouseButtonEvent && mouseButtonEvent.ButtonIndex == (int)ButtonList.Left)
            {
                if (draggedAnchor == null && mouseButtonEvent.Pressed)
                {
                    // Start Drag
                    foreach (var anchor in anchors)
                    {
                        if (!anchor.rect.HasPoint(mouseButtonEvent.Position)) continue;
                        var undo = Plugin.GetUndoRedo();
                        anchor.RegisterUndo(undo);
                        draggedAnchor = anchor;
                        return true;
                    }
                }
                else if (draggedAnchor != null && !mouseButtonEvent.Pressed)
                {
                    // End Drag
                    DragTo(mouseButtonEvent.Position, true);
                    var undo = Plugin.GetUndoRedo();
                    draggedAnchor.CommitUndo(undo);
                    draggedAnchor = null;
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
            var translatedPosition = ViewportToLocalTransform * eventPosition;
            draggedAnchor.DragTo(translatedPosition, commit);

            draggedAnchor.position = eventPosition;
            Plugin.UpdateOverlays();
        }
    }
}
#endif