using NUnit.Framework;
using SplashKitNUnit.Game.Models;

namespace SplashKitNUnit.Game.Test
{
    [TestFixture]
    public class PlayerTests
    {
        private static void AssertPlayerState(Player player, float expectedY, float expectedVelY, bool expectedOnGround)
        {
            Assert.Multiple(() =>
            {
                Assert.That(player.Y, Is.EqualTo(expectedY));
                Assert.That(player.VelocityY, Is.EqualTo(expectedVelY).Within(0.001f));
                Assert.That(player.IsOnGround, Is.EqualTo(expectedOnGround));
            });
        }

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
            var p = new Player(0, 478);
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
            var p = new Player(0, 468);
            p.Jump();
            float velAfterFirst = p.VelocityY;
            p.Jump();
            Assert.That(p.VelocityY, Is.EqualTo(velAfterFirst));
        }

        [Test]
        [TestCase(0f, 498f, 0.005f, true)]   // barely above ground, small dt
        [TestCase(0f, 497f, 0.008f, true)]   // slightly higher, still lands
        public void Update_LandsOnGround_StateIsCorrect(float startX, float startY, float deltaTime, bool expectedOnGround)
        {
            var p = new Player(startX, startY);
            p.Update(deltaTime);
            AssertPlayerState(p, 500f, 0f, expectedOnGround);
        }

        [Test]
        public void Update_FallsToGround_StopsAtGround()
        {
            var p = new Player(0, 445);
            p.Update(0.71f);
            AssertPlayerState(p, 500f, 0f, true);
        }

        [Test]
        public void CollectCoin_IncrementsCount()
        {
            var p = new Player(0, 270);
            p.CollectCoin();
            p.CollectCoin();
            Assert.That(p.CoinsCollected, Is.EqualTo(2));
        }
    }
}