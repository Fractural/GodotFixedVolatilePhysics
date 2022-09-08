using Fractural.Plugin;
using Fractural.Plugin.AssetsRegistry;
using Godot;

#if TOOLS
namespace Volatile.GodotEngine.Plugin
{
	[Tool]
	public class Plugin : ExtendedPlugin
	{
		public override string PluginName => "Godot Fixed Volatile Physics";

		protected override void Load()
		{
			AssetsRegistry = new EditorAssetsRegistry(this);
			AddSubPlugin(new VolatilePolygonPlugin());
			AddSubPlugin(new VolatileRectPlugin());
			AddSubPlugin(new VolatileCirclePlugin());
			AddSubPlugin(new VoltTypesPlugin());
			AddSubPlugin(new VolatileNodesPlugin());
		}

		protected override void Unload()
		{

		}
	}
}
#endif