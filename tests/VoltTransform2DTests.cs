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
               new VoltVector2(Fix64.Cos(theta), Fix64.Sin(theta)),
               new VoltVector2(-Fix64.Sin(theta), Fix64.Cos(theta)),
               new VoltVector2((Fix64)0, (Fix64)0)
            );
            VoltTransform2D aCopy = a;
            VoltTransform2D expected = new VoltTransform2D(
               new VoltVector2(Fix64.Cos(theta), -Fix64.Sin(theta)),
               new VoltVector2(Fix64.Sin(theta), Fix64.Cos(theta)),
               new VoltVector2((Fix64)0, (Fix64)0)
            );
            Assert.IsEqual(a, aCopy, "Expected original to be unaffected.");
            var result = a.Inverse();
            Assert.IsTrue(VoltTransform2D.Approx(result, expected, (Fix64)0.00001f), "Expected result approx == expected.");
        }

        [Test]
        public void WhenTakeInvertOfTransform_WithRotation_ShouldBeCorrect()
        {
            var theta = Fix64.Pi * (Fix64)0.35;
            VoltTransform2D a = new VoltTransform2D(
               new VoltVector2(Fix64.Cos(theta), Fix64.Sin(theta)),
               new VoltVector2(-Fix64.Sin(theta), Fix64.Cos(theta)),
               new VoltVector2((Fix64)0, (Fix64)0)
            );
            VoltTransform2D expected = new VoltTransform2D(
               new VoltVector2(Fix64.Cos(theta), -Fix64.Sin(theta)),
               new VoltVector2(Fix64.Sin(theta), Fix64.Cos(theta)),
               new VoltVector2((Fix64)0, (Fix64)0)
            );
            a.Invert();
            Assert.IsTrue(VoltTransform2D.Approx(a, expected, (Fix64)0.00001f), "Expected result approx == expected.");
        }


        [Test]
        public void WhenTakeInvertOfTransform_WithRotationAndTranslation_ShouldBeCorrect()
        {
            var theta = Fix64.Pi * (Fix64)0.35;
            VoltTransform2D a = new VoltTransform2D(
               new VoltVector2(Fix64.Cos(theta), Fix64.Sin(theta)),
               new VoltVector2(-Fix64.Sin(theta), Fix64.Cos(theta)),
               new VoltVector2((Fix64)5.33, (Fix64)43.9)
            );
            VoltTransform2D expected = new VoltTransform2D(
               new VoltVector2(Fix64.Cos(theta), -Fix64.Sin(theta)),
               new VoltVector2(Fix64.Sin(theta), Fix64.Cos(theta)),
               new VoltVector2((Fix64)(-41.534955775741133651), (Fix64)(-15.181118164301740429))
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
        public void WhenTakeAffineInverseOfTransform_WithRotation_ShouldBeCorrect()
        {
            var angle = Fix64.Pi * (Fix64)1.1;
            VoltTransform2D a = new VoltTransform2D(
               new VoltVector2(Fix64.Cos(angle), Fix64.Sin(angle)),
               new VoltVector2(-Fix64.Sin(angle), Fix64.Cos(angle)),
               VoltVector2.Zero
            );
            VoltTransform2D expected = new VoltTransform2D(
               new VoltVector2(Fix64.Cos(angle), -Fix64.Sin(angle)),
               new VoltVector2(Fix64.Sin(angle), Fix64.Cos(angle)),
               VoltVector2.Zero
            );
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
        public void WhenXFormVector2_ShouldBeCorrect()
        {
            VoltTransform2D a = new VoltTransform2D(
               new VoltVector2((Fix64)1.1f, (Fix64)3),
               new VoltVector2((Fix64)3, (Fix64)4.5f),
               new VoltVector2((Fix64)2.5f, (Fix64)3)
            );
            VoltTransform2D aCopy = a;
            VoltVector2 b = new VoltVector2((Fix64)3, (Fix64)2.2f);
            VoltVector2 expected = new VoltVector2((Fix64)(62f / 5f), (Fix64)(219f / 10f));
            var result = a.XForm(b);
            Assert.IsEqual(a, aCopy, "Expected original to be unaffected.");
            Assert.IsTrue(VoltVector2.Approx(result, expected, (Fix64)0.00001f), "Expected result approx == expected.");
        }

        [Test]
        public void WhenXFormInverseVector2_WithRotationAndTranslation_ShouldBeCorrect()
        {
            VoltTransform2D a = VoltTransform2D.Default();
            a.Rotation = Fix64.Pi * (Fix64)1.25;
            VoltVector2 b = new VoltVector2((Fix64)3.43, (Fix64)5.899);

            var fromInverse = a.AffineInverse().XForm(b);
            var result = a.XFormInverse(b);
            Assert.IsTrue(VoltVector2.Approx(result, fromInverse, (Fix64)0.00001f), "Expected result approx == inverse.");
        }

        [Test]
        public void WhenXFormInverseVector2_WithRotationAndScaleAndTranslation_ShouldBeIncorrect()
        {
            VoltTransform2D a = VoltTransform2D.Default();
            a.SetRotationAndScale(
                Fix64.Pi * (Fix64)0.3483,
                new VoltVector2((Fix64)3.4, (Fix64)3.7)
            );
            a.Origin = new VoltVector2((Fix64)3.4, (Fix64)(-3.43));
            VoltVector2 b = new VoltVector2((Fix64)3.43, (Fix64)5.899);

            var fromInverse = a.AffineInverse().XForm(b);
            var result = a.XFormInverse(b);
            Assert.IsFalse(VoltVector2.Approx(result, fromInverse, (Fix64)0.00001f), "Expected result approx != actual inverse.");
        }

        [Test]
        public void WhenBasisXFormVector2_ShouldBeCorrect()
        {
            VoltTransform2D a = new VoltTransform2D(
               new VoltVector2((Fix64)1.1f, (Fix64)3),
               new VoltVector2((Fix64)3.8, (Fix64)4.5f),
               new VoltVector2((Fix64)2.5f, (Fix64)3)
            );
            VoltVector2 b = new VoltVector2((Fix64)3, (Fix64)2.2f);
            VoltVector2 expected = new VoltVector2((Fix64)(11.66), (Fix64)(18.9));

            var result = a.BasisXForm(b);
            Assert.IsTrue(VoltVector2.Approx(result, expected, (Fix64)0.00001f), "Expected result approx == expected.");
        }

        [Test]
        public void WhenBasisXFormInverseVector2_WithRotationAndReflection_ShouldBeCorrect()
        {
            var rotation = Fix64.Pi * (Fix64)1.25;
            VoltTransform2D a = VoltTransform2D.Default();
            a.SetRotationAndScale(
                Fix64.Pi * (Fix64)0.3483,
                new VoltVector2(-Fix64.One, Fix64.One)
            );
            VoltVector2 b = new VoltVector2((Fix64)3.43, (Fix64)5.899);

            var fromInverse = a.AffineInverse().XForm(b);
            var result = a.BasisXFormInverse(b);
            Assert.IsTrue(VoltVector2.Approx(result, fromInverse, (Fix64)0.00001f), "Expected result approx != actual inverse.");
        }

        [Test]
        public void WhenBasisXFormInverseVector2_WithScale_ShouldBeIncorrect()
        {
            var rotation = Fix64.Pi * (Fix64)1.25;
            VoltTransform2D a = VoltTransform2D.Default();
            a.SetRotationAndScale(
                Fix64.Zero,
                new VoltVector2((Fix64)3.4, (Fix64)3.7)
            );
            VoltVector2 b = new VoltVector2((Fix64)3.43, (Fix64)5.899);

            var fromInverse = a.AffineInverse().XForm(b);
            var result = a.BasisXFormInverse(b);
            Assert.IsFalse(VoltVector2.Approx(result, fromInverse, (Fix64)0.00001f), "Expected result approx != actual inverse.");
        }

        [Test]
        public void WhenBasisXFormInverseVector2_WithTranslation_ShouldBeIncorrect()
        {
            var rotation = Fix64.Pi * (Fix64)1.25;
            VoltTransform2D a = VoltTransform2D.Default();
            a.SetRotationAndScale(
                Fix64.Pi * (Fix64)0.3483,
                VoltVector2.One
            );
            a.Origin = new VoltVector2((Fix64)3.4, (Fix64)(-3.43));
            VoltVector2 b = new VoltVector2((Fix64)3.43, (Fix64)5.899);

            var fromInverse = a.AffineInverse().XForm(b);
            var result = a.BasisXFormInverse(b);
            Assert.IsFalse(VoltVector2.Approx(result, fromInverse, (Fix64)0.00001f), "Expected result approx != actual inverse.");
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

        [Test]
        public void WhenTranslateTransform_WithComponents_ShouldBeCorrect()
        {
            VoltTransform2D a = new VoltTransform2D(
              new VoltVector2((Fix64)1.1, (Fix64)3),
              new VoltVector2((Fix64)3.4, (Fix64)4.5),
              new VoltVector2((Fix64)2.5, (Fix64)3)
            );
            VoltTransform2D expected = new VoltTransform2D(
                new VoltVector2((Fix64)1.1, (Fix64)3),
                new VoltVector2((Fix64)3.4, (Fix64)4.5),
                new VoltVector2((Fix64)2.5f, (Fix64)3) + new VoltVector2((Fix64)103.77, (Fix64)144.6)
            );
            a.Translate((Fix64)4.7, (Fix64)29);
            Assert.IsTrue(VoltTransform2D.Approx(a, expected, (Fix64)0.00001f), "Expected result approx == expected.");
        }

        [Test]
        public void WhenTranslateTransform_WithVector_ShouldBeCorrect()
        {
            VoltTransform2D a = new VoltTransform2D(
              new VoltVector2((Fix64)1.1, (Fix64)3),
              new VoltVector2((Fix64)3.4, (Fix64)4.5),
              new VoltVector2((Fix64)2.5, (Fix64)3)
            );
            VoltTransform2D expected = new VoltTransform2D(
                new VoltVector2((Fix64)1.1, (Fix64)3),
                new VoltVector2((Fix64)3.4, (Fix64)4.5),
                new VoltVector2((Fix64)2.5f, (Fix64)3) + new VoltVector2((Fix64)103.77, (Fix64)144.6)
            );
            a.Translate(new VoltVector2((Fix64)4.7, (Fix64)29));
            Assert.IsTrue(VoltTransform2D.Approx(a, expected, (Fix64)0.00001f), "Expected result approx == expected.");
        }

        [Test]
        public void WhenTranslatedTransform_ShouldBeCorrect()
        {
            VoltTransform2D a = new VoltTransform2D(
              new VoltVector2((Fix64)1.1, (Fix64)3),
              new VoltVector2((Fix64)3.4, (Fix64)4.5),
              new VoltVector2((Fix64)2.5, (Fix64)3)
            );
            VoltTransform2D aCopy = a;
            VoltTransform2D expected = new VoltTransform2D(
                new VoltVector2((Fix64)1.1, (Fix64)3),
                new VoltVector2((Fix64)3.4, (Fix64)4.5),
                new VoltVector2((Fix64)2.5f, (Fix64)3) + new VoltVector2((Fix64)103.77, (Fix64)144.6)
            );
            var result = a.Translated(new VoltVector2((Fix64)4.7, (Fix64)29));
            Assert.IsEqual(a, aCopy, "Expected original to be unaffected.");
            Assert.IsTrue(VoltTransform2D.Approx(result, expected, (Fix64)0.00001f), "Expected result approx == expected.");
        }
    }
}