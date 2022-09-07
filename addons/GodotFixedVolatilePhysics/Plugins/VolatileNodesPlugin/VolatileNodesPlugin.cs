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
            Plugin.AddCustomType(nameof(VolatileRigidBody), nameof(Node2D),
                GD.Load<CSharpScript>("res://addons/GodotFixedVolatilePhysics/Core/VolatileBodies/VolatileRigidBody.cs"),
                GD.Load<Texture>("res://addons/GodotFixedVolatilePhysics/Assets/VoltRigidBody.svg"));
            Plugin.AddCustomType(nameof(VolatileStaticBody), nameof(Node2D),
                GD.Load<CSharpScript>("res://addons/GodotFixedVolatilePhysics/Core/VolatileBodies/VolatileStaticBody.cs"),
                GD.Load<Texture>("res://addons/GodotFixedVolatilePhysics/Assets/VoltStaticBody.svg"));
            Plugin.AddCustomType(nameof(VolatileKinematicBody), nameof(Node2D),
                GD.Load<CSharpScript>("res://addons/GodotFixedVolatilePhysics/Core/VolatileBodies/VolatileKinematicBody.cs"),
                GD.Load<Texture>("res://addons/GodotFixedVolatilePhysics/Assets/VoltKinematicBody.svg"));
            Plugin.AddCustomType(nameof(VolatileArea), nameof(Node2D),
                GD.Load<CSharpScript>("res://addons/GodotFixedVolatilePhysics/Core/VolatileBodies/VolatileArea.cs"),
                GD.Load<Texture>("res://addons/GodotFixedVolatilePhysics/Assets/VoltArea.svg"));
            Plugin.AddCustomType(nameof(VolatileRigidBody), nameof(Node2D),
                GD.Load<CSharpScript>("res://addons/GodotFixedVolatilePhysics/Core/VolatileBodies/VolatileRigidBody.cs"),
                GD.Load<Texture>("res://addons/GodotFixedVolatilePhysics/Assets/VoltRigidBody.svg"));
            Plugin.AddCustomType(nameof(VolatileWorld), nameof(Node2D),
                GD.Load<CSharpScript>("res://addons/GodotFixedVolatilePhysics/Core/VolatileWorld.cs"),
                GD.Load<Texture>("res://addons/GodotFixedVolatilePhysics/Assets/VoltWorld.svg"));
            Plugin.AddCustomType(nameof(SimpleForce), nameof(Node2D),
                GD.Load<CSharpScript>("res://addons/GodotFixedVolatilePhysics/Core/SimpleForce.cs"),
                GD.Load<Texture>("res://addons/GodotFixedVolatilePhysics/Assets/SimpleForce.svg"));
        }

        public override void Unload()
        {
            Plugin.RemoveCustomType(nameof(VoltNode2D));
            Plugin.RemoveCustomType(nameof(VolatileRect));
            Plugin.RemoveCustomType(nameof(VolatilePolygon));
            Plugin.RemoveCustomType(nameof(VolatileCircle));
            Plugin.RemoveCustomType(nameof(VolatileRigidBody));
            Plugin.RemoveCustomType(nameof(VolatileStaticBody));
            Plugin.RemoveCustomType(nameof(VolatileKinematicBody));
            Plugin.RemoveCustomType(nameof(VolatileArea));
            Plugin.RemoveCustomType(nameof(VolatileWorld));
            Plugin.RemoveCustomType(nameof(SimpleForce));
        }
    }
}
#endif