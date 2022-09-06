using FixMath.NET;
using Godot;

namespace Volatile.GodotEngine
{
    [Tool]
    public class VolatileRigidBody : VolatileBody
    {
        protected override VoltBody CreateBody(VoltWorld world, VoltShape[] shapes)
            => world.CreateDynamicBody(GlobalFixedPosition, GlobalFixedRotation, shapes);

        public void AddForce(VoltVector2 force)
        {
            Body.AddForce(force);
        }

        public void AddTorque(Fix64 radians)
        {
            Body.AddTorque(radians);
        }

        public void Set(VoltVector2 position, Fix64 radians)
        {
            Body.Set(position, radians);
        }

        public void SetVelocity(VoltVector2 linearVelocity, Fix64 angularVelocity)
        {
            Body.LinearVelocity = linearVelocity;
            Body.AngularVelocity = angularVelocity;
        }

        public void SetForce(VoltVector2 force, Fix64 torque, VoltVector2 biasVelocity, Fix64 biasRotation)
        {
            Body.SetForce(force, torque, biasVelocity, biasRotation);
        }
    }
}
