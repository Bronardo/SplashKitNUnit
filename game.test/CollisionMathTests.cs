using NUnit.Framework;
using SplashKitNUnit.Game.Models;

namespace SplashKitNUnit.Game.Test
{
    [TestFixture]
    public class CollisionMathTests
    {
        // --- Circle-Rectangle Collision ---

        [Test]
        public void CircleRectCollision_CenterInsideRect_ReturnsTrue()
        {
            bool result = CollisionMath.CircleRectCollision(150, 150, 10, 100, 100, 80, 120);
            Assert.That(result, Is.True);
        }

        [Test]
        public void CircleRectCollision_CircleTouchingEdge_ReturnsTrue()
        {
            // Circle center exactly on left edge of rect
            bool result = CollisionMath.CircleRectCollision(100, 160, 10, 110, 140, 70, 90);
            // Closest point: (110, 160) -> distance = 10, radius=10 => touching (considered collision)
            Assert.That(result, Is.True);
        }

        [Test]
        public void CircleRectCollision_FarApart_ReturnsFalse()
        {
            bool result = CollisionMath.CircleRectCollision(50, 50, 10, 200, 200, 30, 30);
            Assert.That(result, Is.False);
        }

        [Test]
        public void CircleRectCollision_CircleAboveRect_NoOverlap_ReturnsFalse()
        {
            bool result = CollisionMath.CircleRectCollision(210, 180, 10, 190, 230, 45, 55);
            Assert.That(result, Is.False);
        }

        // --- Rectangle-Rectangle Collision ---

        [Test]
        public void RectRectCollision_OverlappingCenters_ReturnsTrue()
        {
            bool result = CollisionMath.RectRectCollision(100, 100, 50, 50, 120, 130, 35, 25);
            Assert.That(result, Is.True);
        }

        [Test]
        public void RectRectCollision_TouchingEdges_ReturnsTrue()
        {
            // One rect's right edge touches the other's left edge
            bool result = CollisionMath.RectRectCollision(0, 0, 48, 37, 47, 21, 34, 18);
            Assert.That(result, Is.True);
        }

        [Test]
        public void RectRectCollision_NotOverlapping_ReturnsFalse()
        {
            bool result = CollisionMath.RectRectCollision(10, 16, 24, 26, 96, 38, 13, 11);
            Assert.That(result, Is.False);
        }

        [Test]
        public void RectRectCollision_OneContainsOther_ReturnsTrue()
        {
            bool result = CollisionMath.RectRectCollision(0, 23, 89, 77, 14, 41, 29, 22);
            Assert.That(result, Is.True);
        }
    }
}