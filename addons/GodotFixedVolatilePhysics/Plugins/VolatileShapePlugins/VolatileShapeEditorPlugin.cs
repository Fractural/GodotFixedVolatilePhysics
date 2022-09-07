using Godot;

#if TOOLS
namespace Volatile.GodotEngine.Plugin
{
    [Tool]
    public abstract class VolatileShapeEditorPlugin : PointsEditorPlugin
    {
        public override void AddAndDrawAnchor(Anchor anchor)
        {
            if (EditedTarget is VolatileShape shape)
                AddAndDrawAnchor(anchor, CIRCLE_RADIUS, STROKE_RADIUS, FILL_COLOR, shape.GetShapeDrawColor());
        }
    }
}
#endif