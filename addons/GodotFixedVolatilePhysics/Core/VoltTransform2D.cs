using FixMath.NET;
using Volatile;

namespace GodotFixedVolatilePhysics
{
    public struct VoltTransform2D
    {
        public VoltTransform2D(VoltVector2 x, VoltVector2 y, VoltVector2 origin)
        {
            matrix = new VoltMatrix(3, 3);

            X = x;
            Y = y;
            Origin = origin;
        }

        public VoltVector2 X
        {
            get => new VoltVector2(matrix[0, 0], matrix[0, 1]);
            set
            {
                matrix[0, 0] = value.x;
                matrix[0, 1] = value.y;
            }
        }
        public VoltVector2 Y
        {
            get => new VoltVector2(matrix[1, 0], matrix[1, 1]);
            set
            {
                matrix[1, 0] = value.x;
                matrix[1, 1] = value.y;
            }
        }
        public VoltVector2 Origin
        {
            get => new VoltVector2(matrix[2, 0], matrix[2, 1]);
            set
            {
                matrix[2, 0] = value.x;
                matrix[2, 1] = value.y;
            }
        }
        private VoltMatrix matrix;
        public VoltMatrix Matrix => matrix;

        public static VoltTransform2D operator *(VoltTransform2D a, VoltTransform2D b)
        {
            var result = a.matrix * b.matrix;
            return result.ToVoltTransform2D();
        }

        public static VoltTransform2D operator +(VoltTransform2D a, VoltTransform2D b)
        {
            var result = a.matrix + b.matrix;
            return result.ToVoltTransform2D();
        }

        public static VoltTransform2D operator -(VoltTransform2D a, VoltTransform2D b)
        {
            var result = a.matrix - b.matrix;
            return result.ToVoltTransform2D();
        }

        public static VoltVector2 operator *(VoltTransform2D a, VoltVector2 b)
        {
            var result = a.matrix * b.to
        }

        public VoltMatrix ToVoltMatrix()
        {
            return new VoltMatrix(new Fix64[,] {
                { X.x, Y.x, Origin.x },
                { X.y, Y.y, Origin.y },
                { Fix64.Zero, Fix64.Zero, Fix64.One }
            });
        }
    }

    public static class VoltTransformExtensions
    {
        public static VoltTransform2D ToVoltTransform2D(this VoltMatrix matrix)
        {
            return new VoltTransform2D(new VoltVector2(matrix[0, 0], matrix[0, 1]), new VoltVector2(matrix[1, 0], matrix[1, 1]), new VoltVector2(matrix[2, 1], matrix[2, 1]));
        }
    }
}
