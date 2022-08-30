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
            Plugin.AddManagedInspectorPlugin(new VoltTypesInspectorPlugin(Plugin));
        }
    }
}
#endif