using Godot;

namespace Volatile.GodotEngine
{
	public interface IVolatileStaticBody : IVolatileBody { }

	[Tool]
	public class VolatileStaticBody : VolatileBody, IVolatileStaticBody
	{
		protected override VoltBody CreateBody(VoltWorld world, VoltShape[] shapes)
			=> world.CreateStaticBody(GlobalFixedPosition, GlobalFixedRotation, shapes, Layer, Mask);

	}
}
