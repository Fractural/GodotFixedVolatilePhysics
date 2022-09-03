using Godot;
using System.Collections.Generic;
using System.Linq;

#if TOOLS
namespace Volatile.GodotEngine.Plugin
{
    [Tool]
    public class VolatilePolygonPlugin : PointsEditorPlugin
    {
        public override string PluginName => nameof(VolatilePolygonPlugin);

        public class PointsArrayAnchor : Anchor
        {
            public VolatilePolygon editedPolygon;
            public int index;

            public override void RegisterUndo(UndoRedo undo)
            {
                undo.CreateAction("Move anchor");
                undo.AddUndoProperty(editedPolygon, nameof(editedPolygon._points), editedPolygon._points);
            }

            public override void CommitUndo(UndoRedo undo)
            {
                undo.AddDoProperty(editedPolygon, nameof(editedPolygon._points), editedPolygon._points);
                undo.CommitAction();
            }

            public override void DragTo(Vector2 localPosition, bool commit)
            {
                if (commit)
                {
                    // Commiting is only done when the drag stops, because it's really costly
                    var points = (VoltVector2[])editedPolygon.Points.Clone();
                    points[index] = localPosition.ToVoltVector2();
                    editedPolygon.Points = points;
                    editedPolygon.PropertyListChangedNotify();
                }
                else
                {
                    editedPolygon.EditorGDPoints[index] = localPosition;
                    editedPolygon.Update();
                }
            }
        }

        protected VolatilePolygon EditedVolatilePolygon
        {
            get => (VolatilePolygon)EditedTarget;
            set => EditedTarget = value;
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

        public override void AddAndDrawAnchors()
        {
            var transform = LocalToViewportTransform;
            var points = EditedVolatilePolygon.EditorGDPoints;
            for (int i = 0; i < points.Length; i++)
            {
                AddAndDrawAnchor(new PointsArrayAnchor()
                {
                    index = i,
                    position = transform * points[i],
                    editedPolygon = EditedVolatilePolygon
                });
            }
        }
    }
}
#endif