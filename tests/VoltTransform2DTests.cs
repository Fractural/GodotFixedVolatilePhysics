using FixMath.NET;
using Volatile;

namespace Tests
{
    public class VoltTransform2DTests : WAT.Test
    {
        [Test]
        public void WhenMultTransforms_ShouldGetResult()
        {
            VoltTransform2D a = new VoltTransform2D(
                new VoltVector2((Fix64)1.1f, (Fix64)3),
                new VoltVector2((Fix64)3, (Fix64)4.5f),
                new VoltVector2((Fix64)2.5f, (Fix64)3)
            );
            VoltTransform2D b = new VoltTransform2D(
                new VoltVector2((Fix64)5, (Fix64)2),
                new VoltVector2((Fix64)3.2f, (Fix64)7.1f),
                new VoltVector2((Fix64)3, (Fix64)3)
            );
            VoltTransform2D expected = new VoltTransform2D(
                new VoltVector2((Fix64)11.5f, (Fix64)24),
                new VoltVector2((Fix64)24.82f, (Fix64)41.55f),
                new VoltVector2((Fix64)14.8f, (Fix64)25.5f)
            );
            var result = a * b;
            Assert.IsTrue(VoltTransform2D.Approx(result, expected, (Fix64)0.00001f), "Expected result approx == expected.");
        }

        [Test]
        public void WhenTakeInverseOfTransform_ShouldBeCorrect()
        {
            var theta = Fix64.Pi * (Fix64)0.35;
            VoltTransform2D a = new VoltTransform2D(
               new VoltVector2(Fix64.Cos(theta), -Fix64.Sin(theta)),
               new VoltVector2(Fix64.Sin(theta), Fix64.Cos(theta)),
               new VoltVector2((Fix64)0, (Fix64)0)
            );
            VoltTransform2D aCopy = a;
            VoltTransform2D expected = new VoltTransform2D(
               new VoltVector2(Fix64.Cos(theta), Fix64.Sin(theta)),
               new VoltVector2(-Fix64.Sin(theta), Fix64.Cos(theta)),
               new VoltVector2((Fix64)0, (Fix64)0)
            );
            Assert.IsEqual(a, aCopy, "Expected original to be unaffected.");
            var result = a.Inverse();
            Assert.IsTrue(VoltTransform2D.Approx(result, expected, (Fix64)0.00001f), "Expected result approx == expected.");
        }

        [Test]
        public void WhenTakeInvertOfTransform_ShouldBeCorrect()
        {
            var theta = Fix64.Pi * (Fix64)0.35;
            VoltTransform2D a = new VoltTransform2D(
               new VoltVector2(Fix64.Cos(theta), -Fix64.Sin(theta)),
               new VoltVector2(Fix64.Sin(theta), Fix64.Cos(theta)),
               new VoltVector2((Fix64)0, (Fix64)0)
            );
            VoltTransform2D expected = new VoltTransform2D(
               new VoltVector2(Fix64.Cos(theta), Fix64.Sin(theta)),
               new VoltVector2(-Fix64.Sin(theta), Fix64.Cos(theta)),
               new VoltVector2((Fix64)0, (Fix64)0)
            );
            a.Invert();
            Assert.IsTrue(VoltTransform2D.Approx(a, expected, (Fix64)0.00001f), "Expected result approx == expected.");
        }

        [Test]
        public void WhenTakeAffineInverseOfTransform_ShouldBeCorrect()
        {
            VoltTransform2D a = new VoltTransform2D(
               new VoltVector2((Fix64)1.1f, (Fix64)3),
               new VoltVector2((Fix64)3, (Fix64)4.5f),
               new VoltVector2((Fix64)2.5f, (Fix64)3)
            );
            VoltTransform2D aCopy = a;
            VoltTransform2D expected = new VoltTransform2D(
                new VoltVector2((Fix64)(-10f / 9f), (Fix64)(20f / 27f)),
                new VoltVector2((Fix64)(20f / 27f), (Fix64)(-22f / 81f)),
                new VoltVector2((Fix64)(5f / 9f), (Fix64)(-28f / 27f))
            );
            Assert.IsEqual(a, aCopy, "Expected original to be unaffected.");
            var result = a.AffineInverse();
            Assert.IsTrue(VoltTransform2D.Approx(result, expected, (Fix64)0.00001f), "Expected result approx == expected.");
        }

        [Test]
        public void WhenTakeAffineInvertOfTransform_ShouldBeCorrect()
        {
            VoltTransform2D a = new VoltTransform2D(
               new VoltVector2((Fix64)1.1f, (Fix64)3),
               new VoltVector2((Fix64)3, (Fix64)4.5f),
               new VoltVector2((Fix64)2.5f, (Fix64)3)
            );
            VoltTransform2D expected = new VoltTransform2D(
                new VoltVector2((Fix64)(-10f / 9f), (Fix64)(20f / 27f)),
                new VoltVector2((Fix64)(20f / 27f), (Fix64)(-22f / 81f)),
                new VoltVector2((Fix64)(5f / 9f), (Fix64)(-28f / 27f))
            );
            a.AffineInvert();
            Assert.IsTrue(VoltTransform2D.Approx(a, expected, (Fix64)0.00001f), "Expected result approx == expected.");
        }


        [Test]
        public void WhenMultWithVector2_ShouldBeCorrect()
        {
            VoltTransform2D a = new VoltTransform2D(
               new VoltVector2((Fix64)1.1f, (Fix64)3),
               new VoltVector2((Fix64)3, (Fix64)4.5f),
               new VoltVector2((Fix64)2.5f, (Fix64)3)
            );
            VoltTransform2D aCopy = a;
            VoltVector2 b = new VoltVector2((Fix64)3, (Fix64)2.2f);
            VoltVector2 expected = new VoltVector2((Fix64)(62f / 5f), (Fix64)(219f / 10f));
            var result = a * b;
            Assert.IsEqual(a, aCopy, "Expected original to be unaffected.");
            Assert.IsTrue(VoltVector2.Approx(result, expected, (Fix64)0.00001f), "Expected result approx == expected.");
        }

        [Test]
        public void WhenTransformOrthonormalize_ShouldBeCorrect()
        {
            VoltTransform2D a = new VoltTransform2D(
               new VoltVector2((Fix64)1.1f, (Fix64)3),
               new VoltVector2((Fix64)3, (Fix64)4.5f),
               new VoltVector2((Fix64)2.5f, (Fix64)3)
            );
            VoltTransform2D expected = new VoltTransform2D(
                new VoltVector2((Fix64)0.344254649158423, (Fix64)0.938876315886609),
                new VoltVector2((Fix64)0.938876315886609, (Fix64)(-0.344254649158423)),
                new VoltVector2((Fix64)2.5f, (Fix64)3)
            );
            a.Orthonormalize();
            Assert.IsTrue(VoltTransform2D.Approx(a, expected, (Fix64)0.00001f), "Expected result approx == expected.");
        }

        [Test]
        public void WhenTransformOrthonormalized_ShouldBeCorrect()
        {
            VoltTransform2D a = new VoltTransform2D(
               new VoltVector2((Fix64)1.1f, (Fix64)3),
               new VoltVector2((Fix64)3, (Fix64)4.5f),
               new VoltVector2((Fix64)2.5f, (Fix64)3)
            );
            VoltTransform2D aCopy = a;
            VoltTransform2D expected = new VoltTransform2D(
                new VoltVector2((Fix64)0.344254649158423, (Fix64)0.938876315886609),
                new VoltVector2((Fix64)0.938876315886609, (Fix64)(-0.344254649158423)),
                new VoltVector2((Fix64)2.5f, (Fix64)3)
            );
            var result = a.Orthonormalized();
            Assert.IsEqual(a, aCopy, "Expected original to be unaffected.");
            Assert.IsTrue(VoltTransform2D.Approx(result, expected, (Fix64)0.00001f), "Expected result approx == expected.");
        }

        [Test]
        public void WhenGetDeterminant_ShouldBeCorrect()
        {
            VoltTransform2D a = new VoltTransform2D(
               new VoltVector2((Fix64)1.1f, (Fix64)3),
               new VoltVector2((Fix64)3, (Fix64)4.5f),
               new VoltVector2((Fix64)2.5f, (Fix64)3)
            );
            VoltTransform2D aCopy = a;
            Fix64 expected = (Fix64)(-4.05);
            var result = a.Determinant;
            Assert.IsTrue(Fix64.Approx(result, expected, (Fix64)0.00001f), "Expected result approx == expected.");
        }

        [Test]
        public void WhenGetRotation_ShouldBeCorrect()
        {
            var theta = Fix64.Pi * (Fix64)0.35;
            VoltTransform2D a = new VoltTransform2D(
               new VoltVector2(Fix64.Cos(theta), Fix64.Sin(theta)),
               new VoltVector2(-Fix64.Sin(theta), Fix64.Cos(theta)),
               new VoltVector2((Fix64)0, (Fix64)0)
            );
            Fix64 expected = theta;
            var result = a.Rotation;
            // Trignometric functions have larger error
            Assert.IsTrue(Fix64.Approx(result, expected, (Fix64)0.01f), "Expected result approx == expected.");
        }

        [Test]
        public void WhenGetRotationScaleAndOrigin_ShouldBeCorrect()
        {
            var rotation = Fix64.Pi * (Fix64)0.35;
            var scale = new VoltVector2((Fix64)20.334, (Fix64)0.349);
            var origin = new VoltVector2((Fix64)4.3, (Fix64)3.2);

            VoltTransform2D a = new VoltTransform2D();
            a.SetRotationAndScale(rotation, scale);
            a.Origin = origin;

            var resultRotation = a.Rotation;
            var resultScale = a.Scale;
            var resultOrigin = a.Origin;

            Assert.IsTrue(Fix64.Approx(resultRotation, rotation, (Fix64)0.01f), "Expected rotation approx == expected.");
            Assert.IsTrue(VoltVector2.Approx(resultScale, scale, (Fix64)0.00001f), "Expected scale approx == expected.");
            Assert.IsTrue(VoltVector2.Approx(resultOrigin, origin, (Fix64)0.00001f), "Expected origin approx == expected.");
        }

        [Test]
        public void WhenTransposeMatrix_ShouldBeCorrect()
        {
            VoltTransform2D a = new VoltTransform2D(
              new VoltVector2((Fix64)1.1, (Fix64)3),
              new VoltVector2((Fix64)3.4, (Fix64)4.5),
              new VoltVector2((Fix64)2.5, (Fix64)3)
            );
            VoltTransform2D expected = new VoltTransform2D(
                new VoltVector2((Fix64)1.1, (Fix64)3.4),
                new VoltVector2((Fix64)3, (Fix64)4.5),
                new VoltVector2((Fix64)2.5f, (Fix64)3)
            );
            a.TransposeBasis();
            Assert.IsTrue(VoltTransform2D.Approx(a, expected, (Fix64)0.00001f), "Expected result approx == expected.");
        }

        [Test]
        public void WhenTransposedMatrix_ShouldBeCorrect()
        {
            VoltTransform2D a = new VoltTransform2D(
              new VoltVector2((Fix64)1.1, (Fix64)3),
              new VoltVector2((Fix64)3.4, (Fix64)4.5),
              new VoltVector2((Fix64)2.5, (Fix64)3)
            );
            VoltTransform2D aCopy = a;
            VoltTransform2D expected = new VoltTransform2D(
                new VoltVector2((Fix64)1.1, (Fix64)3.4),
                new VoltVector2((Fix64)3, (Fix64)4.5),
                new VoltVector2((Fix64)2.5f, (Fix64)3)
            );
            var result = a.TransposedBasis();
            Assert.IsEqual(a, aCopy, "Expected original to be unaffected.");
            Assert.IsTrue(VoltTransform2D.Approx(result, expected, (Fix64)0.00001f), "Expected result approx == expected.");
        }
    }
}