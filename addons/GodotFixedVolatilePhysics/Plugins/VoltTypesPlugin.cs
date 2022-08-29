using FixMath.NET;
using Fractural.Plugin;
using Godot;

#if TOOLS
namespace Volatile.GodotEngine.Plugin
{
    [Tool]
    public class VoltTypesPlugin : SubPlugin
    {
        public override string PluginName => nameof(VoltTypesPlugin);

        public override void Load()
        {
            GD.Print("adding custom inspector plugin");
            Plugin.AddManagedInspectorPlugin(new VoltTypesInspectorPlugin());
        }
    }
}
#endif