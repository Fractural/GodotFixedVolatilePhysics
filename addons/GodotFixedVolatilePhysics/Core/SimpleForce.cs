using Godot;

namespace Volatile.GodotEngine
{
    [Tool]
    public class SimpleForce : VoltNode2D
    {
        #region Force
        private VoltVector2 force;
        public VoltVector2 Force
        {
            get
            {
#if TOOLS
                if (Engine.EditorHint)
                    return VoltType.DeserializeOrDefault<VoltVector2>(_force);
                else
#endif
                    return force;
            }
            set
            {
#if TOOLS
                if (Engine.EditorHint)
                    _force = VoltType.Serialize(value);
                else
#endif
                    force = value;
            }
        }
        [Export(hintString: VoltPropertyHint.VoltVector2)]
        private byte[] _force;
        #endregion

        private VolatileRigidBody body;

        public override void _Ready()
        {
            base._Ready();
            Force = VoltType.DeserializeOrDefault<VoltVector2>(_force);

            if (Engine.EditorHint)
            {
                SetPhysicsProcess(false);
                return;
            }
            body = GetParent<VolatileRigidBody>();
        }

        public override string _GetConfigurationWarning()
        {
            if (!(GetParent() is VolatileRigidBody))
                return $"{nameof(SimpleForce)} must be a child of a {nameof(VolatileRigidBody)}!";
            return "";
        }

        public override void _PhysicsProcess(float delta)
        {
            if (body != null) body.AddForce(Force);
        }
    }
}
