using NUnit.Framework;
using SplashKitNUnit.Game.Models;
using System.Collections.Generic;

namespace SplashKitNUnit.Game.Test
{
    [TestFixture]
    public class CollisionMathTests
    {
        private static IEnumerable<TestCaseData> CircleRectCollisionCases()
        {
            yield return new TestCaseData(150f, 150f, 10f, 100f, 100f, 80f, 120f, true)
                .SetName("CenterInsideRect");
            yield return new TestCaseData(50f, 50f, 10f, 200f, 200f, 30f, 30f, false)
                .SetName("FarApart");
            yield return new TestCaseData(220f, 218f, 12f, 208f, 201f, 33f, 39f, true)
                .SetName("CornerOverlap");
            yield return new TestCaseData(101f, 167f, 9f, 113f, 148f, 66f, 82f, true)
                .SetName("TouchingEdge");
        }

        [Test, TestCaseSource(nameof(CircleRectCollisionCases))]
        public void CircleRectCollision_VariousScenarios_ReturnsExpected(
            float cx, float cy, float radius,
            float rx, float ry, float rw, float rh,
            bool expected)
        {
            bool result = CollisionMath.CircleRectCollision(cx, cy, radius, rx, ry, rw, rh);
            Assert.That(result, Is.EqualTo(expected));
        }

        private static IEnumerable<TestCaseData> RectRectCollisionCases()
        {
            yield return new TestCaseData(100f, 100f, 50f, 50f, 115f, 127f, 34f, 25f, true)
                .SetName("OverlappingCenters");
            yield return new TestCaseData(0f, 51f, 49f, 59f, 44f, 72f, 52f, 30f, true)
                .SetName("TouchingEdges");
            yield return new TestCaseData(97f, 53f, 21f, 12f, 188f, 42f, 17f, 13f, false)
                .SetName("NotOverlapping");
            yield return new TestCaseData(73f, 74f, 88f, 76f, 86f, 91f, 25f, 23f, true)
                .SetName("OneContainsOther");
        }

        [Test, TestCaseSource(nameof(RectRectCollisionCases))]
        public void RectRectCollision_VariousScenarios_ReturnsExpected(
            float x1, float y1, float w1, float h1,
            float x2, float y2, float w2, float h2,
            bool expected)
        {
            bool result = CollisionMath.RectRectCollision(x1, y1, w1, h1, x2, y2, w2, h2);
            Assert.That(result, Is.EqualTo(expected));
        }
    }
}