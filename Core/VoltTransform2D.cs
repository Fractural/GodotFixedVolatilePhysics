using FixMath.NET;
using Godot;
using System;
using Volatile;
using Volatile.GodotEngine;

namespace Volatile
{
    public struct VoltTransform2D
    {
        public static VoltTransform2D Default()
        {
            return new VoltTransform2D(
                new VoltVector2(Fix64.One, Fix64.Zero),
                new VoltVector2(Fix64.Zero, Fix64.One),
                VoltVector2.Zero
            );
        }

        // Transform2D are known as an "affine matrix"
        // Affine matrix has 3 vectors of size 2.
        // Two vectors form a 2D transformation matrix, while the 3rd vector allows for translation.
        // When expressed as an actual matrix, a bottom row of [0 0 1] is added to make the matrix square.
        //
        // ie.
        //      [ X.x  Y.x  T.x ]
        //      [ X.y  Y.y  T.y ]
        //      [ 0    0    1   ]
        //
        // Where X is the first vector, Y is the second vector, and T is the translation vector

        public VoltTransform2D(Fix64 rotation, VoltVector2 position)
        {
            var cosRotation = Fix64.Cos(rotation);
            var sinRotation = Fix64.Sin(rotation);
            X = new VoltVector2(cosRotation, sinRotation);
            Y = new VoltVector2(-sinRotation, cosRotation);
            Origin = position;
        }
        public VoltTransform2D(VoltVector2 x, VoltVector2 y) : this(x, y, VoltVector2.One) { }

        public VoltTransform2D(VoltVector2 x, VoltVector2 y, VoltVector2 origin)
        {
            X = x;
            Y = y;
            Origin = origin;
        }

        public VoltVector2 X { get; set; }
        public VoltVector2 Y { get; set; }
        public VoltVector2 Origin { get; set; }

        // Because the bottom row is [0 0 1], the determinant is actually just the 2D determinant of [X Y].
        public Fix64 Determinant => X.x * Y.y - Y.x * X.y;

        public VoltMatrix ToVoltMatrix()
        {
            return new VoltMatrix(new Fix64[,] {
                { X.x, Y.x, Origin.x },
                { X.y, Y.y, Origin.y },
                { Fix64.Zero, Fix64.Zero, Fix64.One }
            });
        }

        /// <summary>
        /// Inverts the transform, under the assumption that the transformation is composed of rotation and 
        /// translation (no scaling, use <see cref="AffineInverse"/> for transforms with scaling).
        /// </summary>
        public void Invert()
        {
            // Calculate inversed basis matrix for rotation matrix.
            // (Only requires swapping bottom left with top right)
            //
            // Rotation matrix
            // [ cos0, -sin0 ] -> [  cos0, sin0 ]
            // [ sin0,  cos0 ]    [ -sin0, cos0 ]
            var x = new VoltVector2(X.x, Y.x);
            var y = new VoltVector2(X.y, Y.y);

            X = x;
            Y = y;

            // Calculate inversed origin/translation
            Origin = BasisXForm(-Origin);
        }

        /// <summary>
        /// Returns the inverse of the transform, under the assumption that the transformation is composed of rotation 
        /// and translation (no scaling, use <see cref="AffineInverse"/> for transforms with scaling).
        /// </summary>
        /// <returns>Inverse Transform2D</returns>
        public VoltTransform2D Inverse()
        {
            var copy = this;
            copy.Invert();
            return copy;
        }

        /// <summary>
        /// Inverts the transform, accounting for rotation, scaling, and translation.
        /// </summary>
        public void AffineInvert()
        {
            var det = Determinant;
            if (det == Fix64.Zero)
                throw new InvalidOperationException("Cannot find inverse when det == 0!");
            // Calculate inversed basis matrix (Formula for 2x2 is very simple)
            var x = new VoltVector2(Y.y / det, X.y / -det);
            var y = new VoltVector2(Y.x / -det, X.x / det);

            X = x;
            Y = y;

            // Calculate the inversed origin/translation
            Origin = BasisXForm(-Origin);
        }

        /// <summary>
        /// Returns the inverse of the transform, accounting for rotation, scaling, and translation.
        /// </summary>
        /// <returns>Inverse Transform2D</returns>
        public VoltTransform2D AffineInverse()
        {
            var copy = this;
            copy.AffineInvert();
            return copy;
        }

        /// <summary>
        /// Rotation of the matrix
        /// </summary>
        public Fix64 Rotation
        {
            get => Fix64.Atan2(X.y, X.x);
            set
            {
                var scale = Scale;
                var cosRotation = Fix64.Cos(value);
                var sinRotation = Fix64.Sin(value);
                X = new VoltVector2(cosRotation, sinRotation);
                Y = new VoltVector2(-sinRotation, cosRotation);
                // Reassign scale after changing the rotation
                // to ensure scale updates correctly.
                Scale = scale;
            }
        }

        /// <summary>
        /// Scale of the matrix
        /// </summary>
        public VoltVector2 Scale
        {
            get => new VoltVector2(X.Magnitude, (Fix64)Fix64.Sign(BasisDeterminant()) * Y.Magnitude);
            set
            {
                X = X.Normalized;
                Y = Y.Normalized;
                if (!Fix64.Approx(value.x, Fix64.One))
                    X *= value.x;
                if (!Fix64.Approx(value.x, Fix64.One))
                    Y *= value.y;
            }
        }

        /// <summary>
        /// Scales entire affine matrix (basis + origin) by <paramref name="scale"/>
        /// </summary>
        /// <param name="scale"></param>
        public void ScaleEntireMatrix(VoltVector2 scale)
        {
            ScaleBasis(scale);
            Origin = new VoltVector2(
                Origin.x * scale.x,
                Origin.y * scale.y
            );
        }

        /// <summary>
        /// Scales the basis matrix (X, Y) by <paramref name="scale"/>
        /// </summary>
        /// <param name="scale"></param>
        private void ScaleBasis(VoltVector2 scale)
        {
            X = new VoltVector2(
                X.x * scale.x,
                X.y * scale.y
            );
            Y = new VoltVector2(
                Y.x * scale.x,
                Y.y * scale.y
            );
        }

        /// <summary>
        /// Translates the transform by (<paramref name="x"/>, <paramref name="y"/>), relative to the basis vectors.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void Translate(Fix64 x, Fix64 y) => Translate(new VoltVector2(x, y));

        /// <summary>
        /// Translates the transform by <paramref name="translation"/>, relative to the basis vectors.
        /// </summary>
        /// <param name="translation"></param>
        public void Translate(VoltVector2 translation)
        {
            Origin += BasisXForm(translation);
        }

        /// <summary>
        /// Translates the transform by <paramref name="translation"/>, relative to the basis vectors.
        /// </summary>
        /// <param name="translation"></param>
        public VoltTransform2D Translated(VoltVector2 translation)
        {
            var copy = this;
            copy.Translate(translation);
            return copy;
        }

        /// <summary>
        /// Makes the X Y components orthogonal to each other, as well as normalized.
        /// </summary>
        public void Orthonormalize()
        {
            // Treat x as our starting orthonormal vector https://www.youtube.com/watch?v=TRktLuAktBQ
            var x = X.Normalized;

            // Lesson in orthonormalizing vectors
            //         
            //    y     ..>
            //      ..''  | c
            //  ..''      |
            // /----------|--->
            //      d        x
            // |----..----|
            //      \/
            //
            // y dot x = ||y|| ||x|| cosθ       <-- One definition of dot product
            //
            // y dot x = ||y|| cosθ
            //
            // y dot x
            // ------- = ||y|| cos0
            //  ||x||
            //
            // Via pythagorean's theorem, we know that ||y|| cos0 represents the length of 'y' that points in the direction of 'x'.
            //
            // To solve for this, we have to use the left side of this equation.
            //
            // Since we're dealing with a projection onto a normalized vector, we can omit the division by ||x|| (because it's always 1).
            //
            // What we're left with is:
            //      y dot x = length of projection from 'y' onto a normalized 'x'
            // 
            // To find the projected vector (d), we need to scale the normalized 'x' by the length of the projection:
            //      d = x * (y dot x)
            //
            // Finally, we can subtract 'x' from 'd' to get a vector 'c' that points from 'd' to 'y' and is orthogonal to 'x'. (See the image above)
            //
            // And don't forget to normalize this orthogonal vector!
            var y = (Y - (x.Dot(Y) * x)).Normalized;

            X = x;
            Y = y;
        }

        /// <summary>
        /// Returns a new orthonormalized transform
        /// </summary>
        /// <returns></returns>
        public VoltTransform2D Orthonormalized()
        {
            var copy = this;
            copy.Orthonormalize();
            return copy;
        }

        public override bool Equals(object obj)
        {
            if (obj is VoltTransform2D trans)
                return this == trans;
            return false;
        }

        public static bool operator ==(VoltTransform2D a, VoltTransform2D b)
        {
            return a.X == b.X && a.Y == b.Y && a.Origin == b.Origin;
        }

        public static bool operator !=(VoltTransform2D a, VoltTransform2D b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            var hashCode = 17;
            hashCode = hashCode * 31 + X.GetHashCode();
            hashCode = hashCode * 31 + Y.GetHashCode();
            hashCode = hashCode * 31 + Origin.GetHashCode();
            return hashCode;
        }

        /// <summary>
        /// Approximates <paramref name="a"/> == <paramref name="b"/> with a margin of <paramref name="error"/>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="error"></param>
        /// <returns>Whether <paramref name="a"/> approximately == <paramref name="b"/></returns>
        public static bool Approx(VoltTransform2D a, VoltTransform2D b, Fix64 error)
        {
            return VoltVector2.Approx(a.X, b.X, error)
                && VoltVector2.Approx(a.Y, b.Y, error)
                && VoltVector2.Approx(a.Origin, b.Origin, error);
        }

        /// <summary>
        /// Performs matrix multiplication of <paramref name="a"/> * <paramref name="b"/>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static VoltTransform2D operator *(VoltTransform2D a, VoltTransform2D b)
        {
            // Structs are copy on write, therefore the structs we receive
            // as parameters are actually copies.
            a.Origin = a.XForm(b.Origin);

            var x = new VoltVector2(
                a.TransposedDotX(b.X),
                a.TransposedDotY(b.X)
            );
            var y = new VoltVector2(
                a.TransposedDotX(b.Y),
                a.TransposedDotY(b.Y)
            );
            // We assign after calculations, to ensure our results for X
            // don't interfere with the calculations for Y.
            a.X = x;
            a.Y = y;

            return a;
        }

        /// <summary>
        /// Performs matrix vector multiplication to transform <paramref name="b"/> by <paramref name="a"/>.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static VoltVector2 operator *(VoltTransform2D a, VoltVector2 b)
        {
            return a.XForm(b);
        }

        /// <summary>
        /// Transforms <paramref name="v"/> by the basis matrix (Excludes translation).
        /// </summary>
        /// <param name="v"></param>
        /// <returns>Transformed vector.</returns>
        public VoltVector2 BasisXForm(VoltVector2 v)
        {
            return new VoltVector2(TransposedDotX(v), TransposedDotY(v));
        }

        /// <summary>
        /// Transforms <paramref name="v"/> by the tranposed basis matrix (Excludes translation). 
        /// This results in a multiplication by the inverse of the matrix only if it represents a 
        /// rotation/reflection.
        /// </summary>
        /// <param name="v"></param>
        /// <returns>Transformed vector</returns>
        public VoltVector2 BasisXFormInverse(VoltVector2 v)
        {
            return new VoltVector2(X.Dot(v), Y.Dot(v));
        }

        /// <summary>
        /// Transforms <paramref name="v"/> by the affine matrix.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public VoltVector2 XForm(VoltVector2 v)
        {
            // Transpose dot product is equivalent to matrix vector multiplication
            // Also the bottom row of affine matrix being [0 0 1] means the origin
            // is just added onto first transformation.
            return new VoltVector2(TransposedDotX(v), TransposedDotY(v)) + Origin;
        }

        /// <summary>
        /// Transforms <paramref name="v"/> by the transposed matrix (Excludes scaling). 
        /// This results in a multiplication by the inverse of the matrix only if it represents a rotation and/or translation.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public VoltVector2 XFormInverse(VoltVector2 v)
        {
            v -= Origin;
            return new VoltVector2(X.Dot(v), Y.Dot(v));
        }

        /// <summary>
        /// Sets rotation and scale simultaneously. Is more efficient than setting rotation and scale individually.
        /// </summary>
        /// <param name="rotation"></param>
        /// <param name="scale"></param>
        public void SetRotationAndScale(Fix64 rotation, VoltVector2 scale)
        {
            X = new VoltVector2(
                Fix64.Cos(rotation) * scale.x,
                Fix64.Sin(rotation) * scale.x
            );
            Y = new VoltVector2(
                -Fix64.Sin(rotation) * scale.y,
                Fix64.Cos(rotation) * scale.y
            );
        }

        /// <summary>
        /// Transposes the basis
        /// </summary>
        public void TransposeBasis()
        {
            var x = new VoltVector2(X.x, Y.x);
            var y = new VoltVector2(X.y, Y.y);
            X = x;
            Y = y;
        }

        /// <summary>
        /// Returns the transposed basis
        /// </summary>
        /// <returns></returns>
        public VoltTransform2D TransposedBasis()
        {
            var copy = this;
            copy.TransposeBasis();
            return copy;
        }

        /// <summary>
        /// Calculates the dot product of <paramref name="v"/> with the x vector of the transposed basis.
        /// </summary>
        /// <param name="v"></param>
        /// <returns>Dot product</returns>
        private Fix64 TransposedDotX(VoltVector2 v) => X.x * v.x + Y.x * v.y;

        /// <summary>
        /// Calculates the dot product of <paramref name="v"/> with the y vector of the transposed basis.
        /// </summary>
        /// <param name="v"></param>
        /// <returns>Dot product</returns>
        private Fix64 TransposedDotY(VoltVector2 v) => X.y * v.x + Y.y * v.y;

        /// <summary>
        /// Returns the determinant of the basis matrix. Note that this determinant is also the determinant of the affine matrix (3x3).
        /// </summary>
        /// <returns>Determinant of the basis matrix</returns>
        public Fix64 BasisDeterminant() => X.Cross(Y);

        private static readonly Fix64 FIXED_0_9995 = Fix64.From("0.9995");

        public VoltTransform2D InterpolateWith(VoltTransform2D transform, Fix64 time)
        {
            var pos1 = Origin;
            var pos2 = transform.Origin;

            var rot1 = Rotation;
            var rot2 = transform.Rotation;

            var scale1 = Scale;
            var scale2 = transform.Scale;

            var rotVector1 = new VoltVector2(Fix64.Cos(rot1), Fix64.Sin(rot1));
            var rotVector2 = new VoltVector2(Fix64.Cos(rot2), Fix64.Sin(rot2));

            var dot = rotVector1.Dot(rotVector2);
            dot = VoltMath.Clamp(dot, -Fix64.One, Fix64.One);

            VoltVector2 rotVector;
            if (dot > FIXED_0_9995)
            {
                // Lerping for acute angles
                // The two rotations are parellel (the same)
                rotVector = VoltVector2.Lerp(rotVector1, rotVector2, time).Normalized;
            }
            else
            {
                // Solve for angle from dot product formula
                //
                // a * b = |a||b|cos0
                //        a * b
                // cos0 = ------
                //        |a||b|
                // cos0 = a * b   <- (a & b are normalized, therefore |a| = |b| = 1)
                // 0 = arcos(a * b)
                var angle = time * Fix64.Acos(dot);

                // Improved lerping for obtuse angles?
                // See https://chortle.ccsu.edu/vectorlessons/vch09/vch09_6.html for info on dot product range
                var v3 = (rotVector2 - rotVector1 * dot).Normalized;
                rotVector = rotVector1 * Fix64.Cos(angle) + v3 * Fix64.Sin(angle);
            }

            var result = new VoltTransform2D(Fix64.Atan2(rotVector.y, rotVector.x), VoltVector2.Lerp(pos1, pos2, time));
            result.ScaleBasis(VoltVector2.Lerp(scale1, scale2, time));
            return result;
        }

        public override string ToString()
        {
            return $"[X: {X}, Y: {Y}, O: {Origin}]";
        }
    }
}
