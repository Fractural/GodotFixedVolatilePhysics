using Fractural.Plugin;
using Godot;
using Godot.Attributes;

#if TOOLS
namespace GodotFixedVolatilePhysics.Plugin
{
    [Tool]
    public class Plugin : ExtendedPlugin
    {
        public override string PluginName => "Godot Fixed Volatile Physics";

        protected override void Load()
        {
            AddSubPlugin(new VolatilePolygonPlugin());
        }

        protected override void Unload()
        {

        }
    }
}
#endif