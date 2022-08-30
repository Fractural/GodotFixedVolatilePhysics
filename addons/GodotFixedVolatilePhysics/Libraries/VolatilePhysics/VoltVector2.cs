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

namespace Volatile
{
    public struct VoltVector2
    {
        public readonly Fix64 x;
        public readonly Fix64 y;

        public static VoltVector2 Zero => new VoltVector2();
        public static VoltVector2 One => new VoltVector2(Fix64.One, Fix64.One);

        public static VoltVector2 From(string x, string y)
        {
            return new VoltVector2(Fix64.From(x), Fix64.From(y));
        }

        public static Fix64 Dot(VoltVector2 a, VoltVector2 b)
        {
            return (a.x * b.x) + (a.y * b.y);
        }

        public Fix64 SqrMagnitude
        {
            get
            {
                return (this.x * this.x) + (this.y * this.y);
            }
        }

        public Fix64 Magnitude
        {
            get
            {
                return VoltMath.Sqrt(this.SqrMagnitude);
            }
        }

        public VoltVector2 Normalized
        {
            get
            {
                Fix64 magnitude = this.Magnitude;
                return new VoltVector2(this.x / magnitude, this.y / magnitude);
            }
        }

        public VoltVector2(Fix64 x, Fix64 y)
        {
            this.x = x;
            this.y = y;
        }

        public static bool Approx(VoltVector2 a, VoltVector2 b) => Approx(a, b, Fix64.Epsilon);
        public static bool Approx(VoltVector2 a, VoltVector2 b, Fix64 error)
        {
            return Fix64.Approx(a.x, b.x, error) && Fix64.Approx(a.y, b.y, error);
        }

        public static VoltVector2 operator *(VoltVector2 a, Fix64 b)
        {
            return new VoltVector2(a.x * b, a.y * b);
        }

        public static VoltVector2 operator *(Fix64 a, VoltVector2 b)
        {
            return new VoltVector2(b.x * a, b.y * a);
        }

        public static VoltVector2 operator +(VoltVector2 a, VoltVector2 b)
        {
            return new VoltVector2(a.x + b.x, a.y + b.y);
        }

        public static VoltVector2 operator -(VoltVector2 a, VoltVector2 b)
        {
            return new VoltVector2(a.x - b.x, a.y - b.y);
        }

        public static VoltVector2 operator -(VoltVector2 a)
        {
            return new VoltVector2(-a.x, -a.y);
        }

        public override bool Equals(object obj)
        {
            if (obj is VoltVector2 v)
                return v == this;
            return false;
        }

        public override int GetHashCode()
        {
            int hashCode = 17;
            hashCode = hashCode * 31 + x.GetHashCode();
            hashCode = hashCode * 31 + y.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(VoltVector2 a, VoltVector2 b)
        {
            return a.x == b.x && a.y == b.y;
        }

        public static bool operator !=(VoltVector2 a, VoltVector2 b)
        {
            return !(a == b);
        }

        public VoltMatrix ToVoltMatrix()
        {

            return new VoltMatrix(new Fix64[,] {
                { x },
                { y },
                { Fix64.One }
            });
        }

        /// <summary>
        /// Calculates the dot product of "this" with <paramref name="v"/>.
        /// </summary>
        /// <param name="v">Vector</param>
        /// <returns>Dot product</returns>
        public Fix64 Dot(VoltVector2 v)
        {
            return x * v.x + y * v.y;
        }

        /// <summary>
        /// Calculates the signed magnitude of the cross product of two 2D vectors. Note that cross product really only exists for 3D vectors, so we temporarily treat the two 2D vectors as 3D on the same plane.
        /// </summary>
        /// <param name="v">Vector</param>
        /// <returns>Signed magnitude of cross product<paramref name="v"/></returns>
        public Fix64 Cross(VoltVector2 v)
        {
            return x * v.y - y * v.x;
        }

        public static VoltVector2 Lerp(VoltVector2 a, VoltVector2 b, Fix64 t)
        {
            return new VoltVector2(Fix64.Lerp(a.x, b.x, t), Fix64.Lerp(a.y, b.y, t));
        }

        public override string ToString()
        {
            return $"({x}, {y})";
        }
    }
}