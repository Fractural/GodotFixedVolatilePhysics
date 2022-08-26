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

using FixMath.NET;
using System;
using System.Collections.Generic;

#if UNITY
using UnityEngine;
#endif

namespace Volatile
{
  public sealed class VoltCircle : VoltShape
  {
    #region Factory Functions
    internal void InitializeFromWorldSpace(
      VoltVector2 worldSpaceOrigin, 
      Fix64 radius,
      Fix64 density,
      Fix64 friction,
      Fix64 restitution)
    {
      base.Initialize(density, friction, restitution);

      this.worldSpaceOrigin = worldSpaceOrigin;
      this.radius = radius;
      this.sqrRadius = radius * radius;
      this.worldSpaceAABB = new VoltAABB(worldSpaceOrigin, radius);
    }
    #endregion

    #region Properties
    public override VoltShape.ShapeType Type { get { return ShapeType.Circle; } }

    public VoltVector2 Origin { get { return this.worldSpaceOrigin; } }
    public Fix64 Radius { get { return this.radius; } }
    #endregion

    #region Fields
    internal VoltVector2 worldSpaceOrigin;
    internal Fix64 radius;
    internal Fix64 sqrRadius;

    // Precomputed body-space values (these should never change unless we
    // want to support moving shapes relative to their body root later on)
    private VoltVector2 bodySpaceOrigin;
    #endregion

    public VoltCircle() 
    {
      this.Reset();
    }

    protected override void Reset()
    {
      base.Reset();

      this.worldSpaceOrigin = VoltVector2.zero;
      this.radius = Fix64.Zero;
      this.sqrRadius = Fix64.Zero;
      this.bodySpaceOrigin = VoltVector2.zero;
    }

    #region Functionality Overrides
    protected override void ComputeMetrics()
    {
      this.bodySpaceOrigin =
        this.Body.WorldToBodyPointCurrent(this.worldSpaceOrigin);
      this.bodySpaceAABB = new VoltAABB(this.bodySpaceOrigin, this.radius);

      this.Area = this.sqrRadius * VoltMath.PI;
      this.Mass = this.Area * this.Density * VoltConfig.AreaMassRatio;
      this.Inertia =
        this.sqrRadius / (Fix64)2 + this.bodySpaceOrigin.sqrMagnitude;
    }

    protected override void ApplyBodyPosition()
    {
      this.worldSpaceOrigin =
        this.Body.BodyToWorldPointCurrent(this.bodySpaceOrigin);
      this.worldSpaceAABB = new VoltAABB(this.worldSpaceOrigin, this.radius);
    }
    #endregion

    #region Test Overrides
    protected override bool ShapeQueryPoint(
      VoltVector2 bodySpacePoint)
    {
      return 
        Collision.TestPointCircleSimple(
          this.bodySpaceOrigin,
          bodySpacePoint, 
          this.radius);
    }

    protected override bool ShapeQueryCircle(
      VoltVector2 bodySpaceOrigin, 
      Fix64 radius)
    {
      return 
        Collision.TestCircleCircleSimple(
          this.bodySpaceOrigin,
          bodySpaceOrigin, 
          this.radius, 
          radius);
    }

    protected override bool ShapeRayCast(
      ref VoltRayCast bodySpaceRay, 
      ref VoltRayResult result)
    {
      return Collision.CircleRayCast(
        this,
        this.bodySpaceOrigin,
        this.sqrRadius,
        ref bodySpaceRay, 
        ref result);
    }

    protected override bool ShapeCircleCast(
      ref VoltRayCast bodySpaceRay, 
      Fix64 radius,
      ref VoltRayResult result)
    {
      Fix64 totalRadius = this.radius + radius;
      return Collision.CircleRayCast(
        this,
        this.bodySpaceOrigin,
        totalRadius * totalRadius,
        ref bodySpaceRay,
        ref result);
    }
    #endregion

    #region Debug
#if UNITY && DEBUG
    public override void GizmoDraw(
      Color edgeColor, 
      Color normalColor, 
      Color originColor, 
      Color aabbColor, 
      Fix64 normalLength)
    {
      Color current = Gizmos.color;

      Gizmos.color = edgeColor;
      Gizmos.DrawWireSphere(this.worldSpaceOrigin, this.radius);

      this.AABB.GizmoDraw(aabbColor);

      Gizmos.color = current;
    }
#endif
    #endregion
  }
}
