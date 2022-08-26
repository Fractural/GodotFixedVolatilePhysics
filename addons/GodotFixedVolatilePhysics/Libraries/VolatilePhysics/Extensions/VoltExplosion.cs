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

#if UNITY
using UnityEngine;
#endif

namespace Volatile
{
  public delegate void VoltExplosionCallback(
    VoltRayCast rayCast,
    VoltRayResult rayResult,
    Fix64 rayWeight);

  public partial class VoltWorld
  {
    // We'll increase the minimum occluder range by this amount when testing.
    // This way, if an occluder is also a target, we will catch that target
    // within the occluder range. Also allows us to handle the case where the
    // explosion origin is within both targets' and occluders' shapes.
    private static readonly Fix64 EXPLOSION_OCCLUDER_SLOP = (Fix64)0.05M;

    private VoltBuffer<VoltBody> targetBodies;
    private VoltBuffer<VoltBody> occludingBodies;

    public void PerformExplosion(
      VoltVector2 origin,
      Fix64 radius,
      VoltExplosionCallback callback,
      VoltBodyFilter targetFilter = null,
      VoltBodyFilter occlusionFilter = null,
      int ticksBehind = 0,
      int rayCount = 32)
    {
      if (ticksBehind < 0)
        throw new ArgumentOutOfRangeException("ticksBehind");

      // Get all target bodies
      this.PopulateFiltered(
        origin, 
        radius, 
        targetFilter, 
        ticksBehind, 
        ref this.targetBodies);

      // Get all occluding bodies
      this.PopulateFiltered(
        origin,
        radius,
        occlusionFilter,
        ticksBehind,
        ref this.occludingBodies);

      VoltRayCast ray;
      Fix64 rayWeight = Fix64.One / (Fix64)rayCount;
      Fix64 angleIncrement = Fix64.PiTimes2 * rayWeight;

      for (int i = 0; i < rayCount; i++)
      {
        VoltVector2 normal = VoltMath.Polar(angleIncrement * (Fix64)i);
        ray = new VoltRayCast(origin, normal, radius);

        Fix64 minDistance = 
          this.GetOccludingDistance(ray, ticksBehind);
        minDistance += VoltWorld.EXPLOSION_OCCLUDER_SLOP;

        this.TestTargets(ray, callback, ticksBehind, minDistance, rayWeight);
      }
    }

    /// <summary>
    /// Gets the distance to the closest occluder for the given ray.
    /// </summary>
    private Fix64 GetOccludingDistance(
      VoltRayCast ray,
      int ticksBehind)
    {
      Fix64 distance = Fix64.MaxValue;
      VoltRayResult result = default(VoltRayResult);

      for (int i = 0; i < this.occludingBodies.Count; i++)
      {
        if (this.occludingBodies[i].RayCast(ref ray, ref result, ticksBehind))
          distance = result.Distance;
        if (result.IsContained)
          break;
      }

      return distance;
    }

    /// <summary>
    /// Tests all valid explosion targets for a given ray.
    /// </summary>
    private void TestTargets(
      VoltRayCast ray,
      VoltExplosionCallback callback,
      int ticksBehind,
      Fix64 minOccluderDistance,
      Fix64 rayWeight)
    {
      for (int i = 0; i < this.targetBodies.Count; i++)
      {
        VoltBody targetBody = this.targetBodies[i];
        VoltRayResult result = default(VoltRayResult);

        if (targetBody.RayCast(ref ray, ref result, ticksBehind))
          if (result.Distance < minOccluderDistance)
            callback.Invoke(ray, result, rayWeight);
      }
    }

    /// <summary>
    /// Finds all dynamic bodies that overlap with the explosion AABB
    /// and pass the target filter test. Does not test actual shapes.
    /// </summary>
    private void PopulateFiltered(
      VoltVector2 origin,
      Fix64 radius,
      VoltBodyFilter targetFilter,
      int ticksBehind,
      ref VoltBuffer<VoltBody> filterBuffer)
    {
      if (filterBuffer == null)
        filterBuffer = new VoltBuffer<VoltBody>();
      filterBuffer.Clear();

      this.reusableBuffer.Clear();
      this.staticBroadphase.QueryCircle(origin, radius, this.reusableBuffer);
      this.dynamicBroadphase.QueryCircle(origin, radius, this.reusableBuffer);

      VoltAABB aabb = new VoltAABB(origin, radius);
      for (int i = 0; i < this.reusableBuffer.Count; i++)
      {
        VoltBody body = this.reusableBuffer[i];
        if ((targetFilter == null) || targetFilter.Invoke(body))
          if (body.QueryAABBOnly(aabb, ticksBehind))
            filterBuffer.Add(body);
      }
    }
  }
}
