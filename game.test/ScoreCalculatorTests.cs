using NUnit.Framework;
using SplashKitNUnit.Game.Models;

namespace SplashKitNUnit.Game.Test
{
    [TestFixture]
    public class ScoreCalculatorTests
    {
        [Test]
        public void CollectCoin_WithPositiveValue_IncreasesScore()
        {
            var calc = new ScoreCalculator();
            calc.CollectCoin(10);
            Assert.That(calc.Score, Is.EqualTo(10));
        }

        [Test]
        public void CollectCoin_MultipleTimes_AccumulatesScore()
        {
            var calc = new ScoreCalculator();
            calc.CollectCoin(5);
            calc.CollectCoin(15);
            Assert.That(calc.Score, Is.EqualTo(20));
        }

        [Test]
        public void CollectCoin_WithZeroValue_ThrowsArgumentException()
        {
            var calc = new ScoreCalculator();
            Assert.Throws<ArgumentException>(() => calc.CollectCoin(0));
        }

        [Test]
        public void CollectCoin_WithNegativeValue_ThrowsArgumentException()
        {
            var calc = new ScoreCalculator();
            Assert.Throws<ArgumentException>(() => calc.CollectCoin(0));
        }

        [Test]
        public void Reset_SetsScoreToZero()
        {
            var calc = new ScoreCalculator();
            calc.CollectCoin(100);
            calc.Reset();
            Assert.That(calc.Score, Is.Zero);
        }

        [Test]
        public void NewCalculator_HasZeroScore()
        {
            var calc = new ScoreCalculator();
            Assert.That(calc.Score, Is.Zero);
        }
    }
}