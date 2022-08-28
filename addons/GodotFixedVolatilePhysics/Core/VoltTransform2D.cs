using FixMath.NET;
using Godot;
using System;
using Volatile;

namespace Volatile
{
    public struct VoltTransform2D
    {
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

        // Because the bottom row is 0 0 1, the determinant is actually just the 2D determinant of [X Y].
        public Fix64 Determinant => X.x * Y.y + Y.x * X.y;

        public static bool Approx(VoltTransform2D a, VoltTransform2D b, Fix64 error)
        {
            return VoltVector2.Approx(a.X, b.X, error) && VoltVector2.Approx(a.Y, b.Y, error) && VoltVector2.Approx(a.Origin, b.Origin, error);
        }

        public static VoltTransform2D operator *(VoltTransform2D a, VoltTransform2D b)
        {
            var result = a.ToVoltMatrix() * b.ToVoltMatrix();
            return result.ToVoltTransform2D();
        }

        public static VoltTransform2D operator +(VoltTransform2D a, VoltTransform2D b)
        {
            var result = a.ToVoltMatrix() + b.ToVoltMatrix();
            return result.ToVoltTransform2D();
        }

        public static VoltTransform2D operator -(VoltTransform2D a, VoltTransform2D b)
        {
            var result = a.ToVoltMatrix() - b.ToVoltMatrix();
            return result.ToVoltTransform2D();
        }

        public static VoltVector2 operator *(VoltTransform2D a, VoltVector2 b)
        {
            var result = a.ToVoltMatrix() * b.ToVoltMatrix();
            return result.ToVoltVector2();
        }

        public VoltMatrix ToVoltMatrix()
        {
            return new VoltMatrix(new Fix64[,] {
                { X.x, Y.x, Origin.x },
                { X.y, Y.y, Origin.y },
                { Fix64.Zero, Fix64.Zero, Fix64.One }
            });
        }

        /// <summary>
        /// Returns the inverse of the transform, accounting for rotation, scaling, and translation.
        /// </summary>
        /// <returns>Inverse Transform2D</returns>
        public VoltTransform2D AffineInverse()
        {
            var det = Determinant;
            if (det == Fix64.Zero)
                throw new InvalidOperationException("Cannot find inverse when det == 0!");
            // Calculate inversed basis matrix (Formula for 2x2 is very simple)
            var inverse = new VoltTransform2D(
                new VoltVector2(Y.y / det, Y.x / -det),
                new VoltVector2(X.y / -det, X.x / det)
            );
            // Calculate the inversed origin/translation
            inverse.Origin = inverse.BasisXForm(-Origin);
            return inverse;
        }

        /// <summary>
        /// Returns the inverse of the transform, under the assumption that the transformation is composed of rotation and translation (no scaling, use <see cref="AffineInverse"/> for transforms with scaling).
        /// </summary>
        /// <returns>Inverse Transform2D</returns>
        public VoltTransform2D Inverse()
        {
            // Calculate inversed basis matrix for rotation matrix.
            // (Only requires swapping bottom left with top right)
            //
            // Rotation matrix
            // [  cos0, sin0 ] -> [ cos0, -sin0 ]
            // [ -sin0, cos0 ]    [ sin0,  cos0 ]
            var inverse = new VoltTransform2D(
                new VoltVector2(X.x, X.y),
                new VoltVector2(Y.x, Y.y)
            );
            // Calculate inversed origin/translation
            inverse.Origin = inverse.BasisXForm(-Origin);
            return inverse;
        }

        private VoltVector2 RotationScale
        {
            get => new VoltVector2(X.Magnitude, BasisDeterminant().Sign() * Y.Magnitude);
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

        public VoltVector2 Scale
        {
            get => RotationScale;
            set
            {
                ScaleBasis(value);
                Origin = new VoltVector2(
                    Origin.x * value.x,
                    Origin.y * value.y
                );
            }
        }

        public

        public void Rotate(Fix64 phi)
        {

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
        /// Transforms <paramref name="v"/> by the tranposed basis matrix (Excludes translation). This results in a multiplication by the inverse of the matrix only if it represents a rotation/reflection.
        /// </summary>
        /// <param name="v"></param>
        /// <returns>Transformed vector</returns>
        public VoltVector2 BasisXFormInverse(VoltVector2 v)
        {
            return new VoltVector2(X.Dot(v), Y.Dot(v));
        }

        /// <summary>
        /// Returns the transpose of the basis
        /// </summary>
        /// <returns>Transposed basis</returns>
        public VoltTransform2D Transpose()
        {
            return new VoltTransform2D(new VoltVector2(X.x, Y.x), new VoltVector2(X.y, Y.y), Origin);
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
    }
}
