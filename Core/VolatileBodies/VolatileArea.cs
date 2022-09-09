using Godot;
using System.Collections.Generic;

namespace Volatile.GodotEngine
{
	public delegate void BodyEnteredDelegate(IVolatileBody body);
	public delegate void BodyExitedDelegate(IVolatileBody body);

	public interface IVolatileArea : IVolatileBody
	{
		event BodyEnteredDelegate BodyEntered;
		event BodyExitedDelegate BodyExited;

		VoltBodyCollisionResult QueryCollisions(bool collideDynamic = false);
		VoltBodyCollisionResult QueryCollisions(bool collideDynamic, VoltCollisionFilter filter);

		bool AutoQuery { get; set; }
		bool AutoQueryDynamicBodies { get; set; }
	}

	[Tool]
	public class VolatileArea : VolatileBody, IVolatileArea
	{
		public event BodyEnteredDelegate BodyEntered;
		public event BodyExitedDelegate BodyExited;

		[Export]
		public bool AutoQuery { get; set; } = false;
		[Export]
		public bool AutoQueryDynamicBodies { get; set; } = true;

		/// <summary>
		/// All the bodies that this area was previous colliding with.
		/// </summary>
		protected HashSet<IVolatileBody> PrevCollidingWith { get; set; }
		/// <summary>
		/// All the bodies that this area is currently colliding with.
		/// </summary>
		protected HashSet<IVolatileBody> CurrCollidingWith { get; set; }

		protected override VoltBody CreateBody(VoltWorld world, VoltShape[] shapes)
			=> world.CreateTriggerBody(GlobalFixedPosition, GlobalFixedRotation, shapes, Layer, Mask);

		public VoltBodyCollisionResult QueryCollisions(bool collideDynamic = false)
			=> QueryCollisions(collideDynamic, VoltCollisionFilters.DefaultWorldCollisionFilter);
		public VoltBodyCollisionResult QueryCollisions(bool collideDynamic, VoltCollisionFilter filter)
		{
			return Body.QueryTriggerCollisions(collideDynamic, filter);
		}

		public override void _Ready()
		{
			base._Ready();
			if (Engine.EditorHint) return;
			if (AutoQuery)
			{
				PrevCollidingWith = new HashSet<IVolatileBody>();
				CurrCollidingWith = new HashSet<IVolatileBody>();
			}
		}

		protected override void OnBodyCollided(VoltBody body)
		{
			base.OnBodyCollided(body);
			if (AutoQuery && body.UserData is IVolatileBody IVolatileBody)
				CurrCollidingWith.Add(IVolatileBody);
		}

		public override void _PhysicsProcess(float delta)
		{
			base._PhysicsProcess(delta);
			if (Engine.EditorHint) return;
			if (AutoQuery)
			{
				foreach (var oldCollidingBody in PrevCollidingWith)
					if (!CurrCollidingWith.Contains(oldCollidingBody))
						OnBodyExited(oldCollidingBody);

				foreach (var newCollidingBody in CurrCollidingWith)
					if (!PrevCollidingWith.Contains(newCollidingBody))
						OnBodyEntered(newCollidingBody);

				// Swap PreviousColliding with CurrColliding, and then clear CurrColliding
				// This effectively transfers CurrColliding into PreviousColliding, and 
				// clears CurrColliding for future use.
				var temp = PrevCollidingWith;
				PrevCollidingWith = CurrCollidingWith;
				CurrCollidingWith = temp;
				CurrCollidingWith.Clear();
			}
		}

		protected virtual void OnBodyEntered(IVolatileBody body)
		{
			BodyEntered?.Invoke(body);
		}

		protected virtual void OnBodyExited(IVolatileBody body)
		{
			BodyExited?.Invoke(body);
		}
	}
}
