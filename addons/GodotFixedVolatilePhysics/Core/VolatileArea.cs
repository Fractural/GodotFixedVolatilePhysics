using Godot;
using System.Linq;
using Fractural.Utils;

namespace Volatile.GodotEngine
{
    [Tool]
    public class VolatileArea : VoltNode2D
    {
        public override string _GetConfigurationWarning()
        {
            var volatileWorld = this.GetAncestor<VolatileWorld>(false);
            if (volatileWorld == null)
                return $"This node must be a descendant of a VolatileWorld.";

            var shapes = this.GetDescendants<VolatileShape>();
            if (shapes.Count == 0)
                return "This node has no shape, so it can't collide or interact with other objects.\nConsider addinga VolatileShape (VolatilePolygon, VolatileRect, VolatileRect) as a child to define its shape.";
            return "";
        }

        //        public override void _Ready()
        //        {
        //            base._Ready();
        //#if TOOLS
        //            if (Engine.EditorHint)
        //            {
        //                SetPhysicsProcess(false);
        //                return;
        //            }
        //#endif
        //            var volatileWorldNode = this.GetAncestor<VolatileWorld>(false);
        //            if (volatileWorldNode == null)
        //                return;

        //            var shapeNodes = this.GetDescendants<VolatileShape>();
        //            if (shapeNodes.Count == 0)
        //                return;

        //            var world = volatileWorldNode.World;
        //            var shapes = shapeNodes.Select(x => x.PrepareShape(world)).ToArray();

        //            if (IsStatic)
        //                Body = world.CreateStaticBody(GlobalFixedPosition, FixedRotation, shapes);
        //            else
        //                Body = world.CreateDynamicBody(GlobalFixedPosition, FixedRotation, shapes);

        //            world.
        //        }
    }
}
