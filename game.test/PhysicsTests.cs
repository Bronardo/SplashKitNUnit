using NUnit.Framework;
using SplashKitNUnit.Game.Models;

namespace SplashKitNUnit.Game.Test
{
    [TestFixture]
    public class PhysicsTests
    {
        [Test]
        public void ApplyGravity_WithZeroDelta_ReturnsSameVelocity()
        {
            float result = Physics.ApplyGravity(100f, 0f);
            Assert.That(result, Is.EqualTo(100f));
        }

        [Test]
        public void ApplyGravity_WithPositiveDelta_IncreasesVelocityDownward()
        {
            float result = Physics.ApplyGravity(0f, 1f);
            // Gravity = 980, so after 1 second velocity should be 980
            Assert.That(result, Is.EqualTo(980f).Within(0.001f));
        }

        [Test]
        public void ApplyGravity_WithInitialUpwardVelocity_ReducesThenIncreases()
        {
            float result = Physics.ApplyGravity(200f, 1f);
            // 200 + 980 * 1 = 1180
            Assert.That(result, Is.EqualTo(1180f).Within(0.001f));
        }

        [Test]
        public void GetJumpVelocity_ReturnsExpectedConstant()
        {
            float jumpVel = Physics.GetJumpVelocity();
            Assert.That(jumpVel, Is.EqualTo(-400f));
        }

        [Test]
        public void Gravity_IsPositiveConstant()
        {
            Assert.That(Physics.Gravity, Is.GreaterThan(0));
        }
    }
}