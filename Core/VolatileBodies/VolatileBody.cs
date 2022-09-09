using FixMath.NET;
using Godot;
using System.Linq;
using Fractural.Utils;

namespace Volatile.GodotEngine
{
	public delegate void BodyCollidedDelegate(IVolatileBody body);

	public interface IVolatileBody
	{
		VoltNode2D Node { get; }
		event BodyCollidedDelegate BodyCollided;

		VolatileShape[] Shapes { get; set; }
		bool DoInterpolation { get; set; }
		int Layer { get; set; }
		int Mask { get; set; }
		VoltBody Body { get; }
	}

	[Tool]
	public abstract class VolatileBody : VoltNode2D, IVolatileBody
	{
		VoltNode2D IVolatileBody.Node => this;
		public event BodyCollidedDelegate BodyCollided;

		public VolatileShape[] Shapes { get; set; }
		[Export]
		public bool DoInterpolation { get; set; } = true;
		[Export]
		public bool ProcessSelf { get; set; } = true;
		[Export(PropertyHint.Layers2dPhysics)]
		public int Layer { get; set; } = 1;
		[Export(PropertyHint.Layers2dPhysics)]
		public int Mask { get; set; } = 1;
		public VoltBody Body { get; private set; }

		// Interpolation
		private VoltVector2 lastPosition;
		private VoltVector2 nextPosition;

		private Fix64 lastAngle;
		private Fix64 nextAngle;

		public override string _GetConfigurationWarning()
		{
			var volatileWorld = this.GetAncestor<VolatileWorld>(false);
			if (volatileWorld == null)
				return $"This node must be a descendant of a VolatileWorld.";

			var shapes = this.GetDescendants<VolatileShape>();
			if (shapes.Count == 0)
				return "This node has no shape, so it can't collide or interact with other objects.\nConsider addinga VolatileShape (VolatilePolygon, VolatileRect, VolatileRect) as a child to define its shape.";
			return "";
		}

		public override void _Ready()
		{
			base._Ready();
#if TOOLS
			if (Engine.EditorHint)
			{
				SetPhysicsProcess(false);
				return;
			}
#endif
			var volatileWorldNode = this.GetAncestor<VolatileWorld>(false);
			if (volatileWorldNode == null)
				return;

			var shapeNodes = this.GetDescendants<VolatileShape>();
			if (shapeNodes.Count == 0)
				return;

			var world = volatileWorldNode.World;
			var shapes = shapeNodes.Select(x => x.PrepareShape(world)).ToArray();

			Body = CreateBody(world, shapes);
			Body.UserData = this;
			Body.BodyCollided += OnBodyCollided;

			lastPosition = nextPosition = GlobalFixedPosition;
			lastAngle = nextAngle = GlobalFixedRotation;
		}

		protected override void FixedTransformChanged()
		{
			if (!Engine.EditorHint && Body != null)
				Body.Set(FixedPosition, FixedRotation);
			base.FixedTransformChanged();
		}

		protected virtual void OnBodyCollided(VoltBody body)
		{
			if (body.UserData is IVolatileBody volatileBody)
				BodyCollided?.Invoke(volatileBody);
		}

		protected abstract VoltBody CreateBody(VoltWorld world, VoltShape[] shapes);

		public override void _Process(float delta)
		{
			base._Process(delta);

			if (Engine.EditorHint) return;
			if (DoInterpolation)
			{
				Fix64 t = (Fix64)Engine.GetPhysicsInterpolationFraction();

				GlobalFixedPosition = VoltVector2.Lerp(lastPosition, nextPosition, t);
				Fix64 angle = Fix64.Lerp(lastAngle, nextAngle, t);
				GlobalFixedRotation = angle;
			}
		}

		public override void _PhysicsProcess(float delta)
		{
			if (ProcessSelf)
			{
				if (DoInterpolation)
				{
					lastPosition = nextPosition;
					lastAngle = nextAngle;
					nextPosition = Body.Position;
					nextAngle = Body.Angle;
				}
				else
				{
					GlobalFixedPosition = Body.Position;
					GlobalFixedRotation = Body.Angle;
				}
			}
		}
	}
}
