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
  public struct VoltAABB
  {
    #region Static Methods
    public static VoltAABB CreateExpanded(VoltAABB aabb, Fix64 expansionAmount)
    {
      return new VoltAABB(
        aabb.top + expansionAmount,
        aabb.bottom - expansionAmount,
        aabb.left - expansionAmount,
        aabb.right + expansionAmount);
    }

    public static VoltAABB CreateMerged(VoltAABB aabb1, VoltAABB aabb2)
    {
      return new VoltAABB(
        VoltMath.Max(aabb1.top, aabb2.top),
        VoltMath.Min(aabb1.bottom, aabb2.bottom),
        VoltMath.Min(aabb1.left, aabb2.left),
        VoltMath.Max(aabb1.right, aabb2.right));
    }

    public static VoltAABB CreateSwept(VoltAABB source, VoltVector2 vector)
    {
      Fix64 top = source.top;
      Fix64 bottom = source.bottom;
      Fix64 left = source.left;
      Fix64 right = source.right;

      if (vector.x < Fix64.Zero)
        left += vector.x;
      else
        right += vector.x;

      if (vector.y < Fix64.Zero)
        bottom += vector.y;
      else
        top += vector.y;

      return new VoltAABB(top, bottom, left, right);
    }

    /// <summary>
    /// A cheap ray test that requires some precomputed information.
    /// Adapted from: http://www.cs.utah.edu/~awilliam/box/box.pdf
    /// </summary>
    private static bool RayCast(
      ref VoltRayCast ray,
      Fix64 top,
      Fix64 bottom,
      Fix64 left,
      Fix64 right)
    {
      Fix64 txmin =
        ((ray.signX ? right : left) - ray.origin.x) *
        ray.invDirection.x;
      Fix64 txmax =
        ((ray.signX ? left : right) - ray.origin.x) *
        ray.invDirection.x;

      Fix64 tymin =
        ((ray.signY ? top : bottom) - ray.origin.y) *
        ray.invDirection.y;
      Fix64 tymax =
        ((ray.signY ? bottom : top) - ray.origin.y) *
        ray.invDirection.y;

      if ((txmin > tymax) || (tymin > txmax))
        return false;
      if (tymin > txmin)
        txmin = tymin;
      if (tymax < txmax)
        txmax = tymax;
      return (txmax > Fix64.Zero) && (txmin < ray.distance);
    }
    #endregion

    public VoltVector2 TopLeft 
    { 
      get { return new VoltVector2(this.left, this.top); } 
    }

    public VoltVector2 TopRight 
    { 
      get { return new VoltVector2(this.right, this.top); } 
    }

    public VoltVector2 BottomLeft 
    { 
      get { return new VoltVector2(this.left, this.bottom); } 
    }

    public VoltVector2 BottomRight 
    { 
      get { return new VoltVector2(this.right, this.bottom); } 
    }

    public Fix64 Top { get { return this.top; } }
    public Fix64 Bottom { get { return this.bottom; } }
    public Fix64 Left { get { return this.left; } }
    public Fix64 Right { get { return this.right; } }

    public Fix64 Width { get { return this.Right - this.Left; } }
    public Fix64 Height { get { return this.Top - this.Bottom; } }

    public Fix64 Area { get { return this.Width * this.Height; } }
    public Fix64 Perimeter 
    { 
      get { return (Fix64)2 * (this.Width + this.Height); } 
    }

    public VoltVector2 Center { get { return this.ComputeCenter(); } }
    public VoltVector2 Extent 
    { 
      get { return new VoltVector2(this.Width / (Fix64)2, this.Height / (Fix64)2); } 
    }

    private readonly Fix64 top;
    private readonly Fix64 bottom;
    private readonly Fix64 left;
    private readonly Fix64 right;

    #region Tests
    /// <summary>
    /// Performs a point test on the AABB.
    /// </summary>
    public bool QueryPoint(VoltVector2 point)
    {
      return 
        this.left <= point.x && 
        this.right >= point.x &&
        this.bottom <= point.y &&
        this.top >= point.y;
    }

    /// <summary>
    /// Note: This doesn't take rounded edges into account.
    /// </summary>
    public bool QueryCircleApprox(VoltVector2 origin, Fix64 radius)
    {
      return
        (this.left - radius) <= origin.x &&
        (this.right + radius) >= origin.x &&
        (this.bottom - radius) <= origin.y &&
        (this.top + radius) >= origin.y;
    }

    public bool RayCast(ref VoltRayCast ray)
    {
      return VoltAABB.RayCast(
        ref ray, 
        this.top, 
        this.bottom, 
        this.left, 
        this.right);
    }

    /// <summary>
    /// Note: This doesn't take rounded edges into account.
    /// </summary>
    public bool CircleCastApprox(ref VoltRayCast ray, Fix64 radius)
    {
      return VoltAABB.RayCast(
        ref ray,
        this.top + radius,
        this.bottom - radius,
        this.left - radius,
        this.right + radius);
    }

    public bool Intersect(VoltAABB other)
    {
      bool outside =
        this.right <= other.left ||
        this.left >= other.right ||
        this.bottom >= other.top ||
        this.top <= other.bottom;
      return (outside == false);
    }

    public bool Contains(VoltAABB other)
    {
      return
        this.top >= other.Top &&
        this.bottom <= other.Bottom &&
        this.right >= other.right &&
        this.left <= other.left;
    }
    #endregion

    public VoltAABB(Fix64 top, Fix64 bottom, Fix64 left, Fix64 right)
    {
      this.top = top;
      this.bottom = bottom;
      this.left = left;
      this.right = right;
    }

    public VoltAABB(VoltVector2 center, VoltVector2 extents)
    {
      VoltVector2 topRight = center + extents;
      VoltVector2 bottomLeft = center - extents;

      this.top = topRight.y;
      this.right = topRight.x;
      this.bottom = bottomLeft.y;
      this.left = bottomLeft.x;
    }

    public VoltAABB(VoltVector2 center, Fix64 radius)
      : this (center, new VoltVector2(radius, radius))
    {
    }

    public VoltAABB ComputeTopLeft(VoltVector2 center)
    {
      return new VoltAABB(this.top, center.y, this.left, center.x);
    }

    public VoltAABB ComputeTopRight(VoltVector2 center)
    {
      return new VoltAABB(this.top, center.y, center.x, this.right);
    }

    public VoltAABB ComputeBottomLeft(VoltVector2 center)
    {
      return new VoltAABB(center.y, this.bottom, this.left, center.x);
    }

    public VoltAABB ComputeBottomRight(VoltVector2 center)
    {
      return new VoltAABB(center.y, this.bottom, center.x, this.right);
    }

    private VoltVector2 ComputeCenter()
    {
      return new VoltVector2(
        (this.Width / (Fix64)2) + this.left, 
        (this.Height / (Fix64)2) + this.bottom);
    }

    #region Debug
#if UNITY && DEBUG
    public void GizmoDraw(Color aabbColor)
    {
      Color current = Gizmos.color;

      Vector2 A = new Vector2(this.Left, this.Top);
      Vector2 B = new Vector2(this.Right, this.Top);
      Vector2 C = new Vector2(this.Right, this.Bottom);
      Vector2 D = new Vector2(this.Left, this.Bottom);

      Gizmos.color = aabbColor;
      Gizmos.DrawLine(A, B);
      Gizmos.DrawLine(B, C);
      Gizmos.DrawLine(C, D);
      Gizmos.DrawLine(D, A);

      Gizmos.color = current;
    }
#endif
    #endregion
  }
}
