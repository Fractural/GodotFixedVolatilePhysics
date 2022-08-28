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
            var result = a.Inverse();
            Assert.IsTrue(VoltTransform2D.Approx(result, expected, (Fix64)0.00001f), "Expected result approx == expected.");
        }

        [Test]
        public void WhenMultWithVector2_ShouldBeCorrect()
        {
            VoltTransform2D a = new VoltTransform2D(
               new VoltVector2((Fix64)1.1f, (Fix64)3),
               new VoltVector2((Fix64)3, (Fix64)4.5f),
               new VoltVector2((Fix64)2.5f, (Fix64)3)
            );
            VoltVector2 b = new VoltVector2((Fix64)3, (Fix64)2.2f);
            VoltVector2 expected = new VoltVector2((Fix64)(62f / 5f), (Fix64)(219f / 10f));
            var result = a * b;
            Assert.IsTrue(VoltVector2.Approx(result, expected, (Fix64)0.00001f), "Expected result approx == expected.");
        }
    }
}