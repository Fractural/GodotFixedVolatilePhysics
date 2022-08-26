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
  public static class VoltConfig
  {
    public static Fix64 ResolveSlop = (Fix64)0.01M;
    public static Fix64 ResolveRate = (Fix64)0.1M;
    public static Fix64 AreaMassRatio = (Fix64)0.01M;

    // Defaults
    public static readonly Fix64 DEFAULT_DENSITY = Fix64.One;
    public static readonly Fix64 DEFAULT_RESTITUTION = (Fix64)0.5M;
    public static readonly Fix64 DEFAULT_FRICTION = (Fix64)0.8M;

    internal static readonly Fix64 DEFAULT_DELTA_TIME = (Fix64)0.02M;
    internal static readonly Fix64 DEFAULT_DAMPING = (Fix64)0.999M;
    internal const int DEFAULT_ITERATION_COUNT = 20;

    // AABB extension for the dynamic tree
    internal static readonly Fix64 AABB_EXTENSION = (Fix64)0.2M;

    // Maximum contacts for collision resolution.
    internal const int MAX_CONTACTS = 3;

    // Used for initializing timesteps
    internal const int INVALID_TIME = -1;

    // The minimum mass a dynamic object can have before it is
    // converted to a static object
    internal static readonly Fix64 MINIMUM_DYNAMIC_MASS = (Fix64)0.00001M;
  }
}
