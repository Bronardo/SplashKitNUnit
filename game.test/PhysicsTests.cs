using NUnit.Framework;
using SplashKitNUnit.Game.Models;

namespace SplashKitNUnit.Game.Test
{
    [TestFixture]
    public class PhysicsTests
    {
        [Test]
        public void Gravity_IsPositiveConstant()
        {
            Assert.That(Physics.Gravity, Is.GreaterThan(0));
        }

        [Test]
        [TestCase(0f, 0f, 0f)]
        [TestCase(100f, 0f, 100f)]
        [TestCase(0f, 1f, 980f)]
        [TestCase(200f, 1f, 1180f)]
        [TestCase(-400f, 0.5f, 90f)]  // initial upward, half second
        public void ApplyGravity_CalculatesCorrectly(float initialVelocity, float deltaTime, float expected)
        {
            float result = Physics.ApplyGravity(initialVelocity, deltaTime);
            Assert.That(result, Is.EqualTo(expected).Within(0.001f));
        }

        [Test]
        public void GetJumpVelocity_ReturnsExpectedConstant()
        {
            Assert.That(Physics.GetJumpVelocity(), Is.EqualTo(-400f));
        }
    }
}