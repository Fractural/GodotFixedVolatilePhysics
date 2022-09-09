using Godot;

namespace Volatile.GodotEngine
{
	public interface IVolatileKinematicBody : IVolatileBody
	{
		VoltKinematicCollisionResult MoveAndCollide(VoltVector2 linearVelocity);
		VoltVector2 MoveAndSlide(VoltVector2 linearVelocity, int maxSlides = 4);
	}

	[Tool]
	public class VolatileKinematicBody : VolatileBody, IVolatileKinematicBody
	{
		protected override VoltBody CreateBody(VoltWorld world, VoltShape[] shapes)
			=> world.CreateKinematicBody(GlobalFixedPosition, GlobalFixedRotation, shapes, Layer, Mask);

		public VoltKinematicCollisionResult MoveAndCollide(VoltVector2 linearVelocity)
		{
			return Body.MoveAndCollide(linearVelocity);
		}

		public VoltVector2 MoveAndSlide(VoltVector2 linearVelocity, int maxSlides = 4)
		{
			return Body.MoveAndSlide(linearVelocity, maxSlides);
		}
	}
}
