using FixMath.NET;
using Godot;
using Volatile;
using Volatile.GodotEngine;

namespace Tests
{
    [Pre(nameof(PreTest))]
    [Post(nameof(PostTest))]
    public class VoltNode2DTests : WAT.Test
    {
        public VoltNode2D voltNode;

        public void PreTest()
        {
            voltNode = new VoltNode2D();
            AddChild(voltNode);
        }

        public void PostTest()
        {
            voltNode.QueueFree();
        }

        [Test]
        public void WhenSetFixedTransform_ShouldUpdateTransform()
        {
            var newValue = new VoltTransform2D(
                new VoltVector2(Fix64.From(3), Fix64.From(4)),
                new VoltVector2(Fix64.From("5.349"), Fix64.From(8)),
                new VoltVector2(Fix64.From(2), Fix64.From("6.23490"))
            );
            voltNode.FixedTransform = newValue;
            Assert.IsTrue(VoltTransform2D.Approx(voltNode.Transform.ToVoltTransform2D(), newValue), "Expected transform to approx = fixed transform");
        }

        [Test]
        public void WhenSetFixedPosition_ShouldUpdatePosition_ShouldUpdateTransform()
        {
            var newValue = new VoltVector2(Fix64.From("5.349"), Fix64.From(8));
            voltNode.FixedPosition = newValue;
            Assert.IsTrue(VoltVector2.Approx(voltNode.FixedTransform.Origin, newValue), "Expected FixedTransform.Origin to approx == new position");
            Assert.IsTrue(VoltVector2.Approx(voltNode.Transform.origin.ToVoltVector2(), newValue), "Expected Transform.origin to approx == new position");
            Assert.IsTrue(VoltVector2.Approx(voltNode.Position.ToVoltVector2(), newValue), "Expected Position to approx == new position");
        }

        [Test]
        public void WhenSetFixedScale_ShouldUpdateScale_ShouldUpdateTransform()
        {
            var newValue = new VoltVector2(Fix64.From("5.349"), Fix64.From(8));
            voltNode.FixedScale = newValue;
            Assert.IsTrue(VoltVector2.Approx(voltNode.FixedTransform.Scale, newValue), $"Expected FixedTransform.Scale to approx == new scale: {voltNode.FixedTransform.Scale} != {newValue}");
            Assert.IsTrue(VoltVector2.Approx(voltNode.Transform.Scale.ToVoltVector2(), newValue), $"Expected Transform.Scale to approx == new scale: {voltNode.Transform.Scale.ToVoltVector2()} != {newValue}");
            Assert.IsTrue(VoltVector2.Approx(voltNode.Scale.ToVoltVector2(), newValue), $"Expected Scale to approx == new scale: {voltNode.Scale.ToVoltVector2()} != {newValue}");
        }

        [Test]
        public void WhenSetFixedRotation_ShouldUpdateRotation_ShouldUpdateTransform()
        {
            var newValue = Fix64.From("38.5") * Fix64.Deg2Rad;
            voltNode.FixedRotation = newValue;
            // Rotation is from FixedTransform has lower accuracy due to using trig functions,
            // so we're comparing with 0.01 amount of error
            Assert.IsTrue(Fix64.Approx(voltNode.FixedTransform.Rotation, newValue, (Fix64)0.01f), $"Expected FixedTransform.Rotation to approx == new rotation: {voltNode.FixedTransform.Rotation} != {newValue}");
            Assert.IsTrue(Fix64.Approx((Fix64)voltNode.Transform.Rotation, newValue), $"Expected Transform.Rotation to approx == new rotation: {(Fix64)voltNode.Transform.Rotation} != {newValue}");
            Assert.IsTrue(Fix64.Approx((Fix64)voltNode.Rotation, newValue), $"Expected Rotation to approx == new rotation: {(Fix64)voltNode.Rotation} != {newValue}");
        }

        [Test]
        public void WhenSetTransform_ShouldNotUpdateFixedTransform()
        {
            var original = voltNode.FixedTransform;
            var newValue = new Transform2D(0.5f, Vector2.One * 4.434f);
            voltNode.Transform = newValue;
            Assert.IsEqual(voltNode.FixedTransform, original, "Expected FixedTransform to remain same.");
        }

        [Test]
        public void WhenSetPosition_ShouldNotUpdateFixedPosition()
        {
            var original = voltNode.FixedPosition;
            var newValue = new Vector2(39.204f, 8.349f);
            voltNode.Position = newValue;
            Assert.IsEqual(voltNode.FixedPosition, original, "Expected FixedPosition to remain same.");
        }

        [Test]
        public void WhenSetScale_ShouldNotUpdateFixedScale()
        {
            var original = voltNode.FixedScale;
            var newValue = new Vector2(39.204f, 8.349f);
            voltNode.Scale = newValue;
            Assert.IsEqual(voltNode.FixedScale, original, "Expected FixedScale to remain same.");
        }

        [Test]
        public void WhenSetRotation_ShouldNotUpdateFixedRotation()
        {
            var original = voltNode.FixedRotation;
            var newValue = 39.204f;
            voltNode.Rotation = newValue;
            Assert.IsEqual(voltNode.FixedRotation, original, "Expected FixedRotation to remain same.");
        }
    }
}