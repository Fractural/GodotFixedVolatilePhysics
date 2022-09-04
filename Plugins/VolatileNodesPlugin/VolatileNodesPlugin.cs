using Fractural.Plugin;
using Godot;

#if TOOLS
namespace Volatile.GodotEngine.Plugin
{
    [Tool]
    public class VolatileNodesPlugin : SubPlugin
    {
        public override void Load()
        {
            Plugin.AddCustomType(nameof(VoltNode2D), nameof(Node2D),
                GD.Load<CSharpScript>("res://addons/GodotFixedVolatilePhysics/Core/VoltNode2D.cs"),
                GD.Load<Texture>("res://addons/GodotFixedVolatilePhysics/Assets/VoltNode2D.svg"));
            Plugin.AddCustomType(nameof(VolatileRect), nameof(Node2D),
                GD.Load<CSharpScript>("res://addons/GodotFixedVolatilePhysics/Core/VolatileShapes/VolatileRect.cs"),
                GD.Load<Texture>("res://addons/GodotFixedVolatilePhysics/Assets/VoltRect.svg"));
            Plugin.AddCustomType(nameof(VolatilePolygon), nameof(Node2D),
                GD.Load<CSharpScript>("res://addons/GodotFixedVolatilePhysics/Core/VolatileShapes/VolatilePolygon.cs"),
                GD.Load<Texture>("res://addons/GodotFixedVolatilePhysics/Assets/VoltPolygon.svg"));
            Plugin.AddCustomType(nameof(VolatileCircle), nameof(Node2D),
                GD.Load<CSharpScript>("res://addons/GodotFixedVolatilePhysics/Core/VolatileShapes/VolatileCircle.cs"),
                GD.Load<Texture>("res://addons/GodotFixedVolatilePhysics/Assets/VoltNode2D.svg"));
        }

        public override void Unload()
        {
            Plugin.RemoveCustomType(nameof(VoltNode2D));
            Plugin.RemoveCustomType(nameof(VolatileRect));
            Plugin.RemoveCustomType(nameof(VolatilePolygon));
            Plugin.RemoveCustomType(nameof(VolatileCircle));
        }
    }
}
#endif