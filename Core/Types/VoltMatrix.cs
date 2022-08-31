using FixMath.NET;
using System;
using Fractural.Utils;
using System.Text;
using Volatile;
using Volatile.GodotEngine;

namespace Volatile
{
    public struct VoltMatrix
    {
        public Fix64[,] Cells { get; set; }
        public int Rows => Cells.GetLength(0);
        public int Columns => Cells.GetLength(1);
        public bool IsSquare => Rows == Columns;

        public VoltMatrix(int rows, int columns)
        {
            if (rows <= 0 || columns <= 0)
                throw new ArgumentException("Rows and columns must be > 0.");
            Cells = new Fix64[rows, columns];
        }

        public VoltMatrix(Fix64[,] cells)
        {
            Cells = cells;
        }

        public VoltMatrix(float[,] cells)
        {
            Cells = cells.Select(x => (Fix64)x);
        }

        public Fix64 this[int row, int col]
        {
            get => Cells[row, col];
            set => Cells[row, col] = value;
        }

        public static VoltMatrix operator *(VoltMatrix a, VoltMatrix b)
        {
            if (a.Columns != b.Rows)
                throw new ArgumentException("a.Columns must == b.Rows");
            var result = new VoltMatrix(a.Rows, b.Columns);
            for (int row = 0; row < result.Rows; row++)
                for (int col = 0; col < result.Columns; col++)
                    for (int i = 0; i < a.Columns; i++)
                        result[row, col] += a[row, i] * b[i, col];
            return result;
        }

        public static VoltMatrix operator +(VoltMatrix a, VoltMatrix b)
        {
            if (a.Columns != b.Columns || a.Rows != b.Rows)
                throw new ArgumentException("Dimensions of 'a' must = dimensions of 'b'");
            var result = new VoltMatrix(a.Rows, b.Columns);
            for (int row = 0; row < result.Rows; row++)
                for (int col = 0; col < result.Columns; col++)
                    result[row, col] = a[row, col] + b[row, col];
            return result;
        }

        public static VoltMatrix operator -(VoltMatrix a, VoltMatrix b)
        {
            if (a.Columns != b.Columns || a.Rows != b.Rows)
                throw new ArgumentException("Dimensions of 'a' must = dimensions of 'b'");
            var result = new VoltMatrix(a.Rows, b.Columns);
            for (int row = 0; row < result.Rows; row++)
                for (int col = 0; col < result.Columns; col++)
                    result[row, col] = a[row, col] - b[row, col];
            return result;
        }

        public void RowAdd(int fromRow, int toRow, Fix64 fromScale)
        {
            for (int i = 0; i < Columns; i++)
                Cells[toRow, i] += Cells[fromRow, i] * fromScale;
        }

        public void RowSwap(int row, int otherRow)
        {
            for (int i = 0; i < Columns; i++)
            {
                var temp = Cells[row, i];
                Cells[row, i] = Cells[otherRow, i];
                Cells[otherRow, i] = temp;
            }
        }

        public void RowScale(int row, Fix64 scale)
        {
            for (int i = 0; i < Columns; i++)
                Cells[row, i] *= scale;
        }

        public static VoltMatrix Identity(int size)
        {
            var matrix = new VoltMatrix(size, size);
            for (int i = 0; i < size; i++)
                matrix[i, i] = Fix64.One;
            return matrix;
        }

        public VoltMatrix Inverse()
        {
            if (Rows != Columns)
                throw new InvalidOperationException("Cannot find inverse for non square matrix!");

            VoltMatrix copy = this;
            VoltMatrix inverse = Identity(Rows);
            // Make first column into unit vector form [1, 0, 0]
            // Find a number >= 0;
            for (int col = 0; col < Columns; col++)
            {
                // Find the target row to use
                int targetRow = col;
                while (Fix64.Approx(copy[targetRow, col], Fix64.Zero, Fix64.Epsilon))
                {
                    targetRow++;
                    if (targetRow == Rows)
                        throw new InvalidOperationException("Column is empty, meaning the matrix has linearly dependent columns. Therefore matrix is not invertible!");
                }
                // Use the target row to clear our the other rows
                for (int row = 0; row < Rows; row++)
                {
                    if (row == targetRow || copy[row, col] == Fix64.Zero) continue;
                    Fix64 scaleFactor = -copy[row, col] / copy[targetRow, col];
                    copy.RowAdd(targetRow, row, scaleFactor);
                    inverse.RowAdd(targetRow, row, scaleFactor);
                }
                if (copy[targetRow, col] != Fix64.One)
                {
                    // Normalize row
                    Fix64 scaleFactor = Fix64.One / copy[targetRow, col];
                    copy.RowScale(targetRow, scaleFactor);
                    inverse.RowScale(targetRow, scaleFactor);
                }
                if (targetRow != col)
                {
                    // Make sure target row is the correct spot for row echelon
                    // The leading 1 on the 1st column should be at the top,
                    // The leading 1 in the 2nd should be below it, etc.
                    //
                    // ie.
                    //      0 1 2 <. Swap       1 5 0
                    //      1 5 0 <'        ->  0 1 2
                    //      0 2 1               0 2 1
                    copy.RowSwap(targetRow, col);
                    inverse.RowSwap(targetRow, col);
                }
            }
            return inverse;
        }

        public static bool Approx(VoltMatrix a, VoltMatrix b) => Approx(a, b, Fix64.Epsilon);
        public static bool Approx(VoltMatrix a, VoltMatrix b, Fix64 error)
        {
            if (a.Rows != b.Rows || a.Columns != b.Columns) return false;
            for (int row = 0; row < a.Rows; row++)
                for (int col = 0; col < a.Columns; col++)
                    if (!Fix64.Approx(a[row, col], b[row, col], error))
                        return false;
            return true;
        }

        public static bool operator ==(VoltMatrix a, VoltMatrix b)
        {
            if (a.Rows != b.Rows || a.Columns != b.Columns) return false;
            for (int row = 0; row < a.Rows; row++)
                for (int col = 0; col < a.Columns; col++)
                    if (a[row, col] != b[row, col])
                        return false;
            return true;
        }

        public static bool operator !=(VoltMatrix a, VoltMatrix b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (obj is VoltMatrix matrix)
                return this == matrix;
            return false;
        }

        public override int GetHashCode()
        {
            int hashCode = 17;
            for (int row = 0; row < Rows; row++)
                for (int col = 0; col < Columns; col++)
                    hashCode = hashCode * 31 + Cells[row, col].GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            int[] colMaxLength = new int[Columns];
            for (int col = 0; col < Columns; col++)
            {
                for (int row = 0; row < Rows; row++)
                {
                    var length = Cells[row, col].ToString().Length;
                    if (length > colMaxLength[col])
                        colMaxLength[col] = length;
                }
            }
            for (int row = 0; row < Rows; row++)
            {
                sb.Append("[ ");
                for (int col = 0; col < Columns; col++)
                {
                    sb.Append(Cells[row, col]);
                    if (col != Columns - 1)
                        sb.Append(", ");
                    sb.Append(' ', colMaxLength[col] - Cells[row, col].ToString().Length);
                }
                sb.Append(" ]");
                if (row != Rows - 1)
                    sb.Append("\n");
            }
            return sb.ToString();
        }

        public VoltTransform2D ToVoltTransform2D()
        {
            return new VoltTransform2D(new VoltVector2(Cells[0, 0], Cells[0, 1]), new VoltVector2(Cells[1, 0], Cells[1, 1]), new VoltVector2(Cells[2, 1], Cells[2, 1]));
        }

        public VoltVector2 ToVoltVector2()
        {
            return new VoltVector2(Cells[0, 0], Cells[0, 1]);
        }
    }
}
