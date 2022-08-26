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
  /// <summary>
  /// A semi-precomputed ray optimized for fast AABB tests.
  /// </summary>
  public struct VoltRayCast
  {
    internal readonly VoltVector2 origin;
    internal readonly VoltVector2 direction;
    internal readonly VoltVector2 invDirection;
    internal readonly Fix64 distance;
    internal readonly bool signX;
    internal readonly bool signY;

    public VoltRayCast(VoltVector2 origin, VoltVector2 destination)
    {
      VoltVector2 delta = destination - origin;

      this.origin = origin;
      this.direction = delta.normalized;
      this.distance = delta.magnitude;
      this.signX = direction.x < Fix64.Zero;
      this.signY = direction.y < Fix64.Zero;
      this.invDirection = 
        new VoltVector2(Fix64.One / direction.x, Fix64.One / direction.y);
    }

    public VoltRayCast(VoltVector2 origin, VoltVector2 direction, Fix64 distance)
    {
      this.origin = origin;
      this.direction = direction;
      this.distance = distance;
      this.signX = direction.x < Fix64.Zero;
      this.signY = direction.y < Fix64.Zero;
      this.invDirection = 
        new VoltVector2(Fix64.One / direction.x, Fix64.One / direction.y);
    }
  }
}