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
  public sealed class VoltPolygon : VoltShape
  {
    #region Factory Functions
    internal void InitializeFromWorldVertices(
      VoltVector2[] vertices,
      Fix64 density,
      Fix64 friction,
      Fix64 restitution)
    {
      base.Initialize(density, friction, restitution);
      this.UpdateArrays(vertices.Length);

      this.countWorld = vertices.Length;
      Array.Copy(vertices, this.worldVertices, this.countWorld);
      VoltPolygon.ComputeAxes(vertices, this.countWorld, ref this.worldAxes);
      this.worldSpaceAABB = 
        VoltPolygon.ComputeBounds(vertices, this.countWorld);

      this.countBody = 0; // Needs to be set on metric compute
    }

    internal void InitializeFromBodyVertices(
      VoltVector2[] vertices,
      Fix64 density,
      Fix64 friction,
      Fix64 restitution)
    {
      base.Initialize(density, friction, restitution);
      this.UpdateArrays(vertices.Length);

      // World vertices will be computed on position update
      this.countWorld = vertices.Length;

      this.countBody = vertices.Length;
      Array.Copy(vertices, this.bodyVertices, vertices.Length);
      VoltPolygon.ComputeAxes(vertices, this.countBody, ref this.bodyAxes);
      this.bodySpaceAABB = 
        VoltPolygon.ComputeBounds(vertices, this.countBody);

    }
    #endregion

    #region Static Helpers
    private static void WorldToBody(
      VoltBody body,
      VoltVector2[] worldVertices, 
      VoltVector2[] bodyVertices, 
      int count)
    {
      for (int i = 0; i < count; i++)
        bodyVertices[i] = body.WorldToBodyPointCurrent(worldVertices[i]);
    }

    private static void ComputeAxes(
      VoltVector2[] vertices,
      int count,
      ref Axis[] destination)
    {
      if (destination.Length < count)
        destination = new Axis[count];

      for (int i = 0; i < count; i++)
      {
        VoltVector2 u = vertices[i];
        VoltVector2 v = vertices[(i + 1) % count];
        VoltVector2 normal = (v - u).Left().normalized;
        destination[i] = new Axis(normal, VoltVector2.Dot(normal, u));
      }
    }

    private static VoltAABB ComputeBounds(
      VoltVector2[] vertices,
      int count)
    {
      Fix64 top = vertices[0].y;
      Fix64 bottom = vertices[0].y;
      Fix64 left = vertices[0].x;
      Fix64 right = vertices[0].x;

      for (int i = 1; i < count; i++)
      {
        top = VoltMath.Max(top, vertices[i].y);
        bottom = VoltMath.Min(bottom, vertices[i].y);
        left = VoltMath.Min(left, vertices[i].x);
        right = VoltMath.Max(right, vertices[i].x);
      }

      return new VoltAABB(top, bottom, left, right);
    }
    #endregion

    #region Properties
    public override VoltShape.ShapeType Type { get { return ShapeType.Polygon; } }
    #endregion

    #region Fields
    internal VoltVector2[] worldVertices;
    internal Axis[] worldAxes;
    internal int countWorld;

    // Precomputed body-space values (these should never change unless we
    // want to support moving shapes relative to their body root later on)
    internal VoltVector2[] bodyVertices;
    internal Axis[] bodyAxes;
    internal int countBody;
    #endregion

    public VoltPolygon() 
    {
      this.Reset();
    }

    protected override void Reset()
    {
      base.Reset();

      this.countWorld = 0;
      this.countBody = 0;
    }

    #region Functionalty Overrides
    protected override void ComputeMetrics()
    {
      // If we were initialized with world points, we need to compute body
      if (this.countBody == 0)
      {
        // Compute body-space geometry data (only need to do this once)
        VoltPolygon.WorldToBody(
          this.Body, 
          this.worldVertices,
          this.bodyVertices, 
          this.countWorld);
        this.countBody = this.countWorld;
        VoltPolygon.ComputeAxes(this.bodyVertices, this.countBody, ref this.bodyAxes);
        this.bodySpaceAABB = 
          VoltPolygon.ComputeBounds(this.bodyVertices, this.countBody);
      }

      this.Area = this.ComputeArea();
      this.Mass = this.Area * this.Density * VoltConfig.AreaMassRatio;
      this.Inertia = this.ComputeInertia();
    }

    protected override void ApplyBodyPosition()
    {
      for (int i = 0; i < this.countWorld; i++)
      {
        this.worldVertices[i] =
          this.Body.BodyToWorldPointCurrent(this.bodyVertices[i]);
        this.worldAxes[i] =
          this.Body.BodyToWorldAxisCurrent(this.bodyAxes[i]);
      }

      this.worldSpaceAABB = 
        VoltPolygon.ComputeBounds(this.worldVertices, this.countWorld);
    }
    #endregion

    #region Test Overrides
    protected override bool ShapeQueryPoint(
      VoltVector2 bodySpacePoint)
    {
      for (int i = 0; i < this.countBody; i++)
      {
        Axis axis = this.bodyAxes[i];
        if (VoltVector2.Dot(axis.Normal, bodySpacePoint) > axis.Width)
          return false;
      }
      return true;
    }

    protected override bool ShapeQueryCircle(
      VoltVector2 bodySpaceOrigin,
      Fix64 radius)
    {
      // Get the axis on the polygon closest to the circle's origin
      Fix64 penetration;
      int foundIndex =
        Collision.FindAxisMaxPenetration(
          bodySpaceOrigin,
          radius,
          this,
          out penetration);

      if (foundIndex < 0)
        return false;

      int numVertices = this.countBody;
      VoltVector2 a = this.bodyVertices[foundIndex];
      VoltVector2 b = this.bodyVertices[(foundIndex + 1) % numVertices];
      Axis axis = this.bodyAxes[foundIndex];

      // If the circle is past one of the two vertices, check it like
      // a circle-circle intersection where the vertex has radius 0
      Fix64 d = VoltMath.Cross(axis.Normal, bodySpaceOrigin);
      if (d > VoltMath.Cross(axis.Normal, a))
        return Collision.TestPointCircleSimple(a, bodySpaceOrigin, radius);
      if (d < VoltMath.Cross(axis.Normal, b))
        return Collision.TestPointCircleSimple(b, bodySpaceOrigin, radius);
      return true;
    }

    protected override bool ShapeRayCast(
      ref VoltRayCast bodySpaceRay,
      ref VoltRayResult result)
    {
      int foundIndex = -1;
      Fix64 inner = Fix64.MaxValue;
      Fix64 outer = Fix64.Zero;
      bool couldBeContained = true;

      for (int i = 0; i < this.countBody; i++)
      {
        Axis curAxis = this.bodyAxes[i];

        // Distance between the ray origin and the axis/edge along the 
        // normal (i.e., shortest distance between ray origin and the edge)
        Fix64 proj = 
          VoltVector2.Dot(curAxis.Normal, bodySpaceRay.origin) - curAxis.Width;

        // See if the point is outside of any of the axes
        if (proj > Fix64.Zero)
          couldBeContained = false;

        // Projection of the ray direction onto the axis normal (use 
        // negative normal because we want to get the penetration length)
        Fix64 slope = VoltVector2.Dot(-curAxis.Normal, bodySpaceRay.direction);

        if (slope == Fix64.Zero)
          continue;
        Fix64 dist = proj / slope;

        // The ray is pointing opposite the edge normal (towards the edge)
        if (slope > Fix64.Zero)
        {
          if (dist > inner)
          {
            return false;
          }
          if (dist > outer)
          {
            outer = dist;
            foundIndex = i;
          }
        }
        // The ray is pointing along the edge normal (away from the edge)
        else
        {
          if (dist < outer)
          {
            return false;
          }
          if (dist < inner)
          {
            inner = dist;
          }
        }
      }

      if (couldBeContained == true)
      {
        result.SetContained(this);
        return true;
      }
      else if (foundIndex >= 0 && outer <= bodySpaceRay.distance)
      {
        result.Set(
          this,
          outer,
          this.bodyAxes[foundIndex].Normal);
        return true;
      }

      return false;
    }

    protected override bool ShapeCircleCast(
      ref VoltRayCast bodySpaceRay,
      Fix64 radius,
      ref VoltRayResult result)
    {
      bool checkVertices =
        this.CircleCastVertices(
          ref bodySpaceRay,
          radius,
          ref result);

      bool checkEdges =
        this.CircleCastEdges(
          ref bodySpaceRay,
          radius,
          ref result);

      // We need to check both to get the closest hit distance
      return checkVertices || checkEdges;
    }
    #endregion

    #region Collision Helpers
    /// <summary>
    /// Gets the vertices defining an edge of the polygon.
    /// </summary>
    internal void GetEdge(int indexFirst, out VoltVector2 a, out VoltVector2 b)
    {
      a = this.worldVertices[indexFirst];
      b = this.worldVertices[(indexFirst + 1) % this.countWorld];
    }

    /// <summary>
    /// Returns the axis at the given index.
    /// </summary>
    internal Axis GetWorldAxis(int index)
    {
      return this.worldAxes[index];
    }

    /// <summary>
    /// A world-space point query, used as a shortcut in collision tests.
    /// </summary>
    internal bool ContainsPoint(
      VoltVector2 worldSpacePoint)
    {
      for (int i = 0; i < this.countWorld; i++)
      {
        Axis axis = this.worldAxes[i];
        if (VoltVector2.Dot(axis.Normal, worldSpacePoint) > axis.Width)
          return false;
      }
      return true;
    }

    /// <summary>
    /// Special case that ignores axes pointing away from the normal.
    /// </summary>
    internal bool ContainsPointPartial(
      VoltVector2 worldSpacePoint,
      VoltVector2 worldSpaceNormal)
    {
      foreach (Axis axis in this.worldAxes)
        if (VoltVector2.Dot(axis.Normal, worldSpaceNormal) >= Fix64.Zero &&
            VoltVector2.Dot(axis.Normal, worldSpacePoint) > axis.Width)
          return false;
      return true;
    }
    #endregion

    #region Internals
    private void UpdateArrays(int length)
    {
      if ((this.worldVertices == null) ||
          (this.worldVertices.Length < length))
      {
        this.worldVertices = new VoltVector2[length];
        this.worldAxes = new Axis[length];
      }

      if ((this.bodyVertices == null) ||
          (this.bodyVertices.Length < length))
      {
        this.bodyVertices = new VoltVector2[length];
        this.bodyAxes = new Axis[length];
      }
    }

    private Fix64 ComputeArea()
    {
      Fix64 sum = Fix64.Zero;

      for (int i = 0; i < this.countBody; i++)
      {
        VoltVector2 v = this.bodyVertices[i];
        VoltVector2 u = this.bodyVertices[(i + 1) % this.countBody];
        VoltVector2 w = this.bodyVertices[(i + 2) % this.countBody];

        sum += u.x * (v.y - w.y);
      }

      return sum / (Fix64)2;
    }

    private Fix64 ComputeInertia()
    {
      Fix64 s1 = Fix64.Zero;
      Fix64 s2 = Fix64.Zero;

      for (int i = 0; i < this.countBody; i++)
      {
        VoltVector2 v = this.bodyVertices[i];
        VoltVector2 u = this.bodyVertices[(i + 1) % this.countBody];

        Fix64 a = VoltMath.Cross(u, v);
        Fix64 b = v.sqrMagnitude + u.sqrMagnitude + VoltVector2.Dot(v, u);
        s1 += a * b;
        s2 += a;
      }

      return s1 / ((Fix64)6 * s2);
    }

    private bool CircleCastEdges(
      ref VoltRayCast bodySpaceRay,
      Fix64 radius,
      ref VoltRayResult result)
    {
      int foundIndex = -1;
      bool couldBeContained = true;

      // Pre-compute and initialize values
      Fix64 shortestDist = Fix64.MaxValue;
      VoltVector2 v3 = bodySpaceRay.direction.Left();

      // Check the edges -- this will be different from the raycast because
      // we care about staying within the ends of the edge line segment
      for (int i = 0; i < this.countBody; i++)
      {
        Axis curAxis = this.bodyAxes[i];

        // Push the edges out by the radius
        VoltVector2 extension = curAxis.Normal * radius;
        VoltVector2 a = this.bodyVertices[i] + extension;
        VoltVector2 b = this.bodyVertices[(i + 1) % this.countBody] + extension;

        // Update the check for containment
        if (couldBeContained == true)
        {
          Fix64 proj = 
            VoltVector2.Dot(curAxis.Normal, bodySpaceRay.origin) - curAxis.Width;

          // The point lies outside of the outer layer
          if (proj > radius)
          {
            couldBeContained = false;
          }
          // The point lies between the outer and inner layer
          else if (proj > Fix64.Zero)
          {
            // See if the point is within the center Vornoi region of the edge
            Fix64 d = VoltMath.Cross(curAxis.Normal, bodySpaceRay.origin);
            if (d > VoltMath.Cross(curAxis.Normal, a))
              couldBeContained = false;
            if (d < VoltMath.Cross(curAxis.Normal, b))
              couldBeContained = false;
          }
        }

        // For the cast, only consider rays pointing towards the edge
        if (VoltVector2.Dot(curAxis.Normal, bodySpaceRay.direction) >= Fix64.Zero)
          continue;

        // See: 
        // https://rootllama.wordpress.com/2014/06/20/ray-line-segment-intersection-test-in-2d/
        VoltVector2 v1 = bodySpaceRay.origin - a;
        VoltVector2 v2 = b - a;

        Fix64 denominator = VoltVector2.Dot(v2, v3);
        Fix64 t1 = VoltMath.Cross(v2, v1) / denominator;
        Fix64 t2 = VoltVector2.Dot(v1, v3) / denominator;

        if ((t2 >= Fix64.Zero) && (t2 <= Fix64.One) && (t1 > Fix64.Zero) && (t1 < shortestDist))
        {
          // See if the point is outside of any of the axes
          shortestDist = t1;
          foundIndex = i;
        }
      }

      // Report results
      if (couldBeContained == true)
      {
        result.SetContained(this);
        return true;
      }
      else if (foundIndex >= 0 && shortestDist <= bodySpaceRay.distance)
      {
        result.Set(
          this,
          shortestDist,
          this.bodyAxes[foundIndex].Normal);
        return true;
      }
      return false;
    }

    private bool CircleCastVertices(
      ref VoltRayCast bodySpaceRay,
      Fix64 radius,
      ref VoltRayResult result)
    {
      Fix64 sqrRadius = radius * radius;
      bool castHit = false;

      for (int i = 0; i < this.countBody; i++)
      {
        castHit |=
          Collision.CircleRayCast(
            this,
            this.bodyVertices[i],
            sqrRadius,
            ref bodySpaceRay,
            ref result);
        if (result.IsContained == true)
          return true;
      }

      return castHit;
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

      for (int i = 0; i < this.countWorld; i++)
      {
        Vector2 u = this.worldVertices[i];
        Vector2 v = this.worldVertices[(i + 1) % this.countWorld];
        Vector2 n = worldAxes[i].Normal;

        Vector2 delta = v - u;
        Vector2 midPoint = u + (delta * 0.5f);

        // Draw edge
        Gizmos.color = edgeColor;
        Gizmos.DrawLine(u, v);

        // Draw normal
        Gizmos.color = normalColor;
        Gizmos.DrawLine(midPoint, midPoint + (n * normalLength));
      }

      this.AABB.GizmoDraw(aabbColor);

      Gizmos.color = current;
    }
#endif
    #endregion
  }
}
