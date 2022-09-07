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

namespace Volatile
{
    /// <summary>
    /// Holds a set of premade collision filters
    /// </summary>
    public static class VoltCollisionFilters
    {
        #region Filters
        /// <summary>
        /// Base filter used by every other filter. This filter implements collision mask functionality
        /// and prevents an object from colliding with itself.
        /// </summary>
        /// <param name="one"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        private static bool BaseCollisionFilter(VoltBody one, VoltBody other)
        {
            if ((one == other)
                || !CanCollideWithLayersAndMask(one, other))
                return false;
            return true;
        }

        /// <summary>
        /// Default collision filter used to handle world collisions
        /// </summary>
        /// <param name="one"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool DefaultWorldCollisionFilter(VoltBody one, VoltBody other)
        {
            // Ignore self collisions
            // Ignore static-static collisions
            // Ignore kinematic and static collisions
            // Ignore kinematic and dynamic collisions
            if (!BaseCollisionFilter(one, other)
                || BothOfType(one, other, VoltBodyType.Static)
                || BothOfTypes(one, other, VoltBodyType.Kinematic, VoltBodyType.Static)
                || BothOfTypes(one, other, VoltBodyType.Kinematic, VoltBodyType.Kinematic))
                return false;
            return true;
        }

        /// <summary>
        /// Default collision filter used by triggers when querying for collisions
        /// </summary>
        /// <param name="one"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool DefaultTriggerQueryFilter(VoltBody one, VoltBody other)
        {
            if (!BaseCollisionFilter(one, other))
                return false;
            return true;
        }

        /// <summary>
        /// Default collision filter used by kinematic bodies when 
        /// finding static bodies to move and collide with.
        /// </summary>
        /// <param name="one"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool DefaultMoveAndCollideFilter(VoltBody one, VoltBody other)
        {
            if (!BaseCollisionFilter(one, other)
                || AtleastOneOfType(one, other, VoltBodyType.Dynamic)
                || AtleastOneOfType(one, other, VoltBodyType.Trigger))
                return false;
            return true;
        }
        #endregion

        #region Helpers
        public static bool CanCollideWithLayersAndMask(VoltBody one, VoltBody other)
        {
            return (one.Mask & other.Layer) > 0;
        }
        public static bool BothOfType(VoltBody one, VoltBody other, VoltBodyType type)
        {
            return one.BodyType == type && other.BodyType == type;
        }

        public static bool OneOfType(VoltBody one, VoltBody other, VoltBodyType type)
        {
            return (one.BodyType != type && other.BodyType == type) || (one.BodyType == type && other.BodyType != type);
        }

        public static bool AtleastOneOfType(VoltBody one, VoltBody other, VoltBodyType type)
        {
            return one.BodyType == type || other.BodyType == type;
        }

        public static bool BothOfTypes(VoltBody one, VoltBody other, VoltBodyType oneType, VoltBodyType otherType)
        {
            return (one.BodyType == oneType && other.BodyType == otherType) || (one.BodyType == otherType && other.BodyType == oneType);
        }
        #endregion

    }
}