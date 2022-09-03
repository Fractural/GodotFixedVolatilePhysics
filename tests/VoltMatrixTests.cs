using FixMath.NET;
using Godot;
using Volatile;
using Volatile.GodotEngine;

namespace Tests
{
    public class VoltMatrixTests : WAT.Test
    {
        [Test]
        public void WhenAddMatrices_OfDifferentSize_ShouldFail()
        {
            VoltMatrix a = new VoltMatrix(3, 2);
            VoltMatrix b = new VoltMatrix(5, 2);
            Assert.Throws(() =>
            {
                VoltMatrix result = a + b;
            }, "Expected 'dimensions are not same' error");
        }

        [Test]
        public void WhenAddMatrices_OfSameSize_ShouldAddEveryElement()
        {
            VoltMatrix a = new VoltMatrix(new float[,]{
                {   1,      8.5f,   5f      },
                {   3.2f,   0.5f,   2.5f    },
                {   1.5f,   6f,     3.2f    },
            });
            VoltMatrix b = new VoltMatrix(new float[,]{
                {   3.2f,   3f,     4f      },
                {   5.2f,   7.4f,   2f      },
                {   2f,     8f,     4.2f    },
            });
            VoltMatrix expected = new VoltMatrix(new float[,]{
                {   4.2f,   11.5f,  9       },
                {   8.4f,   7.9f,   4.5f    },
                {   3.5f,   14f,    7.4f    },
            });
            var result = a + b;
            Assert.IsTrue(VoltMatrix.Approx(result, expected, (Fix64)0.001f), "Expected result approx == expected.");
        }

        [Test]
        public void WhenSubtractMatrices_OfSameSize_ShouldSubtractEveryElement()
        {
            VoltMatrix a = new VoltMatrix(new float[,]{
                {   4.2f,   11.5f,  9       },
                {   8.4f,   7.9f,   4.5f    },
                {   3.5f,   14f,    7.4f    },
            });
            VoltMatrix b = new VoltMatrix(new float[,]{
                {   1,      8.5f,   5f      },
                {   3.2f,   0.5f,   2.5f    },
                {   1.5f,   6f,     3.2f    },
            });
            VoltMatrix expected = new VoltMatrix(new float[,]{
                {   3.2f,   3f,     4f      },
                {   5.2f,   7.4f,   2f      },
                {   2f,     8f,     4.2f    },
            });
            var result = a - b;
            Assert.IsTrue(VoltMatrix.Approx(result, expected, (Fix64)0.00001f), "Expected result approx == expected.");
        }

        [Test]
        public void WhenGetIdentityMatrix_ShouldBeCorrect()
        {
            VoltMatrix expected = new VoltMatrix(new Fix64[,]{
                {   Fix64.One,  Fix64.Zero, Fix64.Zero  },
                {   Fix64.Zero, Fix64.One,  Fix64.Zero  },
                {   Fix64.Zero, Fix64.Zero, Fix64.One   },
            });
            Assert.IsEqual(VoltMatrix.Identity(3), expected);
        }

        [Test]
        public void WhenMultMatrices_ShouldGetResult()
        {
            VoltMatrix a = new VoltMatrix(new float[,]{
                {   5,      2.4f,   },
                {   3.2f,   2,      },
                {   4.35f,  3,      },
            });
            VoltMatrix b = new VoltMatrix(new float[,]{
                {   4.3f,   4,      },
                {   3,      2.1f,   },
            });
            VoltMatrix expected = new VoltMatrix(new float[,]{
                {   28.7f,      25.04f, },
                {   19.76f,     17,     },
                {   27.705f,    23.7f,  },
            });
            var result = a * b;
            Assert.IsTrue(VoltMatrix.Approx(result, expected, (Fix64)0.00001f), "Expected result approx == expected.");
        }

        [Test]
        public void WhenMultMatrices_OfConflictingDimensions_ShouldFail()
        {
            VoltMatrix a = new VoltMatrix(3, 2);
            VoltMatrix b = new VoltMatrix(5, 3);
            Assert.Throws(() =>
            {
                VoltMatrix result = a * b;
            }, "Expected 'dimensions are not same' error");
        }

        [Test]
        public void WhenTakeInverseMatrix_OfSquareMatrix_ShouldBeCorrect()
        {
            VoltMatrix a = new VoltMatrix(new float[,] {
                { -59.3f,   2,          7.3f    },
                { 50.3f,    38.4f,      -83     },
                { 2,        -17.34f,    23.4f   }
            });
            VoltMatrix expected = new VoltMatrix(new float[,] {
                { -0.024085658019277204485f, -0.0077239291952397445137f, -0.019882941011289538541f },
                { -0.059829690439554750072f, -0.062466968867293459257f,  -0.20290605452036783943f },
                { -0.042276731460825852874f, -0.045629460759332867285f,  -0.10592414971626492558f },
            });
            var result = a.Inverse();
            Assert.IsTrue(VoltMatrix.Approx(result, expected, (Fix64)0.00001f), "Expected result approx == expected.");
        }

        [Test]
        public void WhenTakeInverseMatrix_OfNonSquareMatrix_ShouldFail()
        {
            VoltMatrix a = new VoltMatrix(new float[,] {
                { -59.3f,   2       },
                { 50.3f,    38.4f   },
                { 2,        -17.34f }
            });
            Assert.Throws(() =>
            {
                var result = a.Inverse();
            }, "Expected 'not square matrix' error");
        }

        [Test]
        public void WhenTakeInverseMatrix_OfLinearlyDependentColumns_ShouldFail()
        {
            VoltMatrix a = new VoltMatrix(new float[,] {
                { -1,   32.1f,  -3,      },
                { 3.4f, 15,     10.2f,  },
                { 2,    7.1f,   6,      }
            });
            Assert.Throws(() =>
            {
                var result = a.Inverse();
            }, "Expected 'linearly dependent columns' error");
        }
    }
}