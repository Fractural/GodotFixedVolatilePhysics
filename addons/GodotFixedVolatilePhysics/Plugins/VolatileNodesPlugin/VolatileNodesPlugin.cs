﻿using Fractural.Plugin;
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
            Plugin.AddCustomType(nameof(VolatileShape), nameof(Node2D),
                GD.Load<CSharpScript>("res://addons/GodotFixedVolatilePhysics/Core/VolatileShapes/VolatileShape.cs"),
                GD.Load<Texture>("res://addons/GodotFixedVolatilePhysics/Assets/VoltNode2D.svg"));
            Plugin.AddCustomType(nameof(VolatileRect), nameof(Node2D),
                GD.Load<CSharpScript>("res://addons/GodotFixedVolatilePhysics/Core/VolatileShapes/VolatileRect.cs"),
                GD.Load<Texture>("res://addons/GodotFixedVolatilePhysics/Assets/VoltRect.svg"));
            Plugin.AddCustomType(nameof(VolatilePolygon), nameof(Node2D),
                GD.Load<CSharpScript>("res://addons/GodotFixedVolatilePhysics/Core/VolatileShapes/VolatilePolygon.cs"),
                GD.Load<Texture>("res://addons/GodotFixedVolatilePhysics/Assets/VoltPolygon.svg"));
        }

        public override void Unload()
        {
            Plugin.RemoveCustomType(nameof(VoltNode2D));
            Plugin.RemoveCustomType(nameof(VoltShape));
            Plugin.RemoveCustomType(nameof(VolatileRect));
            Plugin.RemoveCustomType(nameof(VolatilePolygon));
        }
    }
}
#endif