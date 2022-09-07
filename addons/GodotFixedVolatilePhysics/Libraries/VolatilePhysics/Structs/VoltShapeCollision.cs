using FixMath.NET;
using System.Collections.Generic;

namespace Volatile
{
    public struct VoltKinematicCollisionResult
    {
        public readonly VoltBodyCollisionResult BestCollision;
        public readonly bool HasCollision;
        public readonly VoltVector2 RemainingVelocity;

        public VoltVector2 CollisionNormal => BestCollision.CumulativeNormal();

        public VoltKinematicCollisionResult(VoltBodyCollisionResult bestCollision, VoltVector2 remainingVelocity)
        {
            BestCollision = bestCollision;
            RemainingVelocity = remainingVelocity;
            HasCollision = true;
        }
    }

    public struct VoltBodyCollisionResult
    {
        public readonly VoltBody QueryBody;
        public readonly VoltShapeCollision[] ShapeCollisions;
        public readonly bool HasCollision;

        public VoltBodyCollisionResult(VoltBody queryBody, VoltShapeCollision[] shapeCollisions)
        {
            QueryBody = queryBody;
            ShapeCollisions = shapeCollisions;
            HasCollision = true;
            collidingBodies = null;
        }

        public VoltVector2 CumulativeNormal()
        {
            var cumulativeNormal = VoltVector2.Zero;
            foreach (var collision in ShapeCollisions)
                cumulativeNormal += collision.CumulativeNormal();
            return cumulativeNormal.TryNormalized;
        }
        public VoltVector2 CumulativePenetrationVector()
        {
            var cumulativeNormal = VoltVector2.Zero;
            foreach (var collision in ShapeCollisions)
                cumulativeNormal += collision.CumulativePenetrationVector();
            return cumulativeNormal;
        }

        private VoltBody[] collidingBodies;
        // CollidingBodies is lazy initialized
        public VoltBody[] CollidingBodies
        {
            get
            {
                if (collidingBodies == null)
                {
                    var collidingBodiesHashset = new HashSet<VoltBody>();
                    foreach (VoltShapeCollision shapeCollision in ShapeCollisions)
                        collidingBodiesHashset.Add(shapeCollision.CollidingShape.Body);
                    collidingBodies = new VoltBody[collidingBodiesHashset.Count];
                    collidingBodiesHashset.CopyTo(collidingBodies);
                }
                return collidingBodies;
            }
        }
    }

    /// <summary>
    /// Stores collision information. Basically a struct version of a <see cref="Manifold"/> that's publicly accessible.
    /// </summary>
    public struct VoltShapeCollision
    {
        public readonly VoltShape QueryShape;
        public readonly VoltShape CollidingShape;
        public readonly VoltCollisionContact[] Contacts;

        public VoltShapeCollision(VoltShape queryShape, VoltShape collidingShape, VoltCollisionContact[] contacts)
        {
            this.QueryShape = queryShape;
            this.CollidingShape = collidingShape;
            this.Contacts = contacts;
        }

        public VoltVector2 CumulativeNormal()
        {
            var cumulativeNormal = VoltVector2.Zero;
            foreach (var contact in Contacts)
                cumulativeNormal += contact.Normal;
            return cumulativeNormal.TryNormalized;
        }

        public VoltVector2 CumulativePenetrationVector()
        {
            var cumulativeNormal = VoltVector2.Zero;
            foreach (var contact in Contacts)
                cumulativeNormal += contact.PenetrationVector();
            return cumulativeNormal;
        }
    }

    /// <summary>
    /// Stores contact information for a collision
    /// </summary>
    public struct VoltCollisionContact
    {
        public readonly VoltVector2 Position;
        public readonly VoltVector2 Normal;
        public readonly Fix64 Penetration;

        public VoltCollisionContact(VoltVector2 position, VoltVector2 normal, Fix64 penetration) : this()
        {
            this.Position = position;
            this.Normal = normal;
            this.Penetration = penetration;
        }

        public VoltVector2 PenetrationVector()
        {
            return Penetration * Normal;
        }
    }
}