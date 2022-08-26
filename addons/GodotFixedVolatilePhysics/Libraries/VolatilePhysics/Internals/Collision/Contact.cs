/*
 *  VolatilePhysics - A 2D Physics Library for Networked Games
 *  Copyright (c) 2015-2016 - Alexander Shoulson - http://ashoulson.com
 *
 *  This software is provided 'as-is', without any express or implied
 *  warranty. In no event will the authors be held liable for any damages
 *  arising from the use of this software.
 *  Permission is granted to anyone to use this software for any purpose,
 *  including commercial applications, and to alter it and redistribute it
 *  freely, subject to the following restrictions:
 *  
 *  1. The origin of this software must not be misrepresented; you must not
 *     claim that you wrote the original software. If you use this software
 *     in a product, an acknowledgment in the product documentation would be
 *     appreciated but is not required.
 *  2. Altered source versions must be plainly marked as such, and must not be
 *     misrepresented as being the original software.
 *  3. This notice may not be removed or altered from any source distribution.
*/

#if UNITY
using UnityEngine;
#endif

using FixMath.NET;

namespace Volatile
{
  internal sealed class Contact 
    : IVoltPoolable<Contact>
  {
    #region Interface
    IVoltPool<Contact> IVoltPoolable<Contact>.Pool { get; set; }
    void IVoltPoolable<Contact>.Reset() { this.Reset(); }
    #endregion

    #region Static Methods
    private static Fix64 BiasDist(Fix64 dist)
    {
      return VoltConfig.ResolveRate * VoltMath.Min(Fix64.Zero, dist + VoltConfig.ResolveSlop);
    }
    #endregion

    private VoltVector2 position;
    private VoltVector2 normal;
    private Fix64 penetration;

    private VoltVector2 toA;
    private VoltVector2 toB;
    private VoltVector2 toALeft;
    private VoltVector2 toBLeft;

    private Fix64 nMass;
    private Fix64 tMass;
    private Fix64 restitution;
    private Fix64 bias;
    private Fix64 jBias;

    private Fix64 cachedNormalImpulse;
    private Fix64 cachedTangentImpulse;

    public Contact()
    {
      this.Reset();
    }

    internal Contact Assign(
      VoltVector2 position,
      VoltVector2 normal,
      Fix64 penetration)
    {
      this.Reset();

      this.position = position;
      this.normal = normal;
      this.penetration = penetration;

      return this;
    }

    internal void PreStep(Manifold manifold)
    {
      VoltBody bodyA = manifold.ShapeA.Body;
      VoltBody bodyB = manifold.ShapeB.Body;

      this.toA = this.position - bodyA.Position;
      this.toB = this.position - bodyB.Position;
      this.toALeft = this.toA.Left();
      this.toBLeft = this.toB.Left();

      this.nMass = Fix64.One / this.KScalar(bodyA, bodyB, this.normal);
      this.tMass = Fix64.One / this.KScalar(bodyA, bodyB, this.normal.Left());

      this.bias = Contact.BiasDist(penetration);
      this.jBias = Fix64.Zero;
      this.restitution =
        manifold.Restitution *
        VoltVector2.Dot(
          this.normal,
          this.RelativeVelocity(bodyA, bodyB));
    }

    internal void SolveCached(Manifold manifold)
    {
      this.ApplyContactImpulse(
        manifold.ShapeA.Body,
        manifold.ShapeB.Body,
        this.cachedNormalImpulse,
        this.cachedTangentImpulse);
    }

    internal void Solve(Manifold manifold)
    {
      VoltBody bodyA = manifold.ShapeA.Body;
      VoltBody bodyB = manifold.ShapeB.Body;
      Fix64 elasticity = bodyA.World.Elasticity;

      // Calculate relative bias velocity
      VoltVector2 vb1 = bodyA.BiasVelocity + (bodyA.BiasRotation * this.toALeft);
      VoltVector2 vb2 = bodyB.BiasVelocity + (bodyB.BiasRotation * this.toBLeft);
      Fix64 vbn = VoltVector2.Dot((vb1 - vb2), this.normal);

      // Calculate and clamp the bias impulse
      Fix64 jbn = this.nMass * (vbn - this.bias);
      jbn = VoltMath.Max(-this.jBias, jbn);
      this.jBias += jbn;

      // Apply the bias impulse
      this.ApplyNormalBiasImpulse(bodyA, bodyB, jbn);

      // Calculate relative velocity
      VoltVector2 vr = this.RelativeVelocity(bodyA, bodyB);
      Fix64 vrn = VoltVector2.Dot(vr, this.normal);

      // Calculate and clamp the normal impulse
      Fix64 jn = nMass * (vrn + (this.restitution * elasticity));
      jn = VoltMath.Max(-this.cachedNormalImpulse, jn);
      this.cachedNormalImpulse += jn;

      // Calculate the relative tangent velocity
      Fix64 vrt = VoltVector2.Dot(vr, this.normal.Left());

      // Calculate and clamp the friction impulse
      Fix64 jtMax = manifold.Friction * this.cachedNormalImpulse;
      Fix64 jt = vrt * tMass;
      Fix64 result = VoltMath.Clamp(this.cachedTangentImpulse + jt, -jtMax, jtMax);
      jt = result - this.cachedTangentImpulse;
      this.cachedTangentImpulse = result;

      // Apply the normal and tangent impulse
      this.ApplyContactImpulse(bodyA, bodyB, jn, jt);
    }

    #region Internals
    private void Reset()
    {
      this.position = VoltVector2.zero;
      this.normal = VoltVector2.zero;
      this.penetration = Fix64.Zero;

      this.toA = VoltVector2.zero;
      this.toB = VoltVector2.zero;
      this.toALeft = VoltVector2.zero;
      this.toBLeft = VoltVector2.zero;

      this.nMass = Fix64.Zero;
      this.tMass = Fix64.Zero;
      this.restitution = Fix64.Zero;
      this.bias = Fix64.Zero;
      this.jBias = Fix64.Zero;

      this.cachedNormalImpulse = Fix64.Zero;
      this.cachedTangentImpulse = Fix64.Zero;
    }

    private Fix64 KScalar(
      VoltBody bodyA,
      VoltBody bodyB,
      VoltVector2 normal)
    {
      Fix64 massSum = bodyA.InvMass + bodyB.InvMass;
      Fix64 r1cnSqr = VoltMath.Square(VoltMath.Cross(this.toA, normal));
      Fix64 r2cnSqr = VoltMath.Square(VoltMath.Cross(this.toB, normal));
      return
        massSum +
        bodyA.InvInertia * r1cnSqr +
        bodyB.InvInertia * r2cnSqr;
    }

    private VoltVector2 RelativeVelocity(VoltBody bodyA, VoltBody bodyB)
    {
      return
        (bodyA.AngularVelocity * this.toALeft + bodyA.LinearVelocity) -
        (bodyB.AngularVelocity * this.toBLeft + bodyB.LinearVelocity);
    }

    private void ApplyNormalBiasImpulse(
      VoltBody bodyA,
      VoltBody bodyB,
      Fix64 normalBiasImpulse)
    {
      VoltVector2 impulse = normalBiasImpulse * this.normal;
      bodyA.ApplyBias(-impulse, this.toA);
      bodyB.ApplyBias(impulse, this.toB);
    }

    private void ApplyContactImpulse(
      VoltBody bodyA,
      VoltBody bodyB,
      Fix64 normalImpulseMagnitude,
      Fix64 tangentImpulseMagnitude)
    {
      VoltVector2 impulseWorld =
        new VoltVector2(normalImpulseMagnitude, tangentImpulseMagnitude);
      VoltVector2 impulse = impulseWorld.Rotate(this.normal);

      bodyA.ApplyImpulse(-impulse, this.toA);
      bodyB.ApplyImpulse(impulse, this.toB);
    }
    #endregion
  }
}