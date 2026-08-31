using NUnit.Framework;
using SplashKitNUnit.Game.Models;

namespace SplashKitNUnit.Game.Test
{
    [TestFixture]
    public class PlayerTests
    {
        [Test]
        public void Constructor_SetsPositionAndDefaults()
        {
            var p = new Player(100, 300);
            Assert.Multiple(() =>
            {
                Assert.That(p.X, Is.EqualTo(100));
                Assert.That(p.Y, Is.EqualTo(300));
                Assert.That(p.VelocityY, Is.Zero);
                Assert.That(p.IsOnGround, Is.True);
                Assert.That(p.CoinsCollected, Is.Zero);
            });
        }

        [Test]
        public void Jump_WhenOnGround_SetsVelocityAndNotOnGround()
        {
            var p = new Player(0, 480);
            p.Jump();
            Assert.Multiple(() =>
            {
                Assert.That(p.VelocityY, Is.EqualTo(-400f));
                Assert.That(p.IsOnGround, Is.False);
            });
        }

        [Test]
        public void Jump_WhenAlreadyAirborne_DoesNothing()
        {
            var p = new Player(0, 460);
            p.Jump(); // first jump
            float velAfterFirst = p.VelocityY;
            p.Jump(); // second jump should be ignored
            Assert.That(p.VelocityY, Is.EqualTo(velAfterFirst));
            Assert.That(p.IsOnGround, Is.False);
        }

        [Test]
        public void Update_WithGravity_MovesPlayerDownward()
        {
            var p = new Player(0, 490);
            p.Update(0.05f); // 50 ms
            Assert.That(p.Y, Is.GreaterThan(490));
            Assert.That(p.VelocityY, Is.GreaterThan(0));
        }

        [Test]
        public void Update_FallsToGround_StopsAtGround()
        {
            var p = new Player(0, 470);
            // Use a delta time large enough to definitely reach ground (e.g., 0.3s)
            p.Update(0.3f);
            Assert.Multiple(() =>
            {
                Assert.That(p.Y, Is.EqualTo(500));          // clamped to ground
                Assert.That(p.VelocityY, Is.Zero);          // stopped
                Assert.That(p.IsOnGround, Is.True);         // landed
            });
        }

        [Test]
        public void CollectCoin_IncrementsCount()
        {
            var p = new Player(0, 240);
            p.CollectCoin();
            p.CollectCoin();
            Assert.That(p.CoinsCollected, Is.EqualTo(2));
        }
    }
}