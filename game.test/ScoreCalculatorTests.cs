using NUnit.Framework;
using SplashKitNUnit.Game.Models;

namespace SplashKitNUnit.Game.Test
{
    [TestFixture]
    public class ScoreCalculatorTests
    {
        private ScoreCalculator _calc;

        [SetUp]
        public void SetUp()
        {
            _calc = new ScoreCalculator();
        }

        [Test]
        public void NewCalculator_HasZeroScore()
        {
            Assert.That(_calc.Score, Is.Zero);
        }

        [Test]
        [TestCase(10, 10)]
        [TestCase(1, 1)]
        [TestCase(999, 999)]
        public void CollectCoin_PositiveValues_IncreasesScoreCorrectly(int coinValue, int expectedScore)
        {
            _calc.CollectCoin(coinValue);
            Assert.That(_calc.Score, Is.EqualTo(expectedScore));
        }

        [Test]
        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(-100)]
        public void CollectCoin_NonPositiveValues_ThrowsArgumentException(int invalidValue)
        {
            Assert.Throws<ArgumentException>(() => _calc.CollectCoin(invalidValue));
        }

        [Test]
        public void CollectCoin_MultipleTimes_AccumulatesScore()
        {
            _calc.CollectCoin(5);
            _calc.CollectCoin(15);
            Assert.That(_calc.Score, Is.EqualTo(20));
        }

        [Test]
        public void Reset_SetsScoreToZero()
        {
            _calc.CollectCoin(100);
            _calc.Reset();
            Assert.That(_calc.Score, Is.Zero);
        }
    }
}