namespace SplashKitNUnit.Game.Models
{
    public class ScoreCalculator
    {
        private int _score;

        public int Score => _score;

        public void CollectCoin(int value)
        {
            if (value <= 0)
                throw new ArgumentException("Coin value must be positive.", nameof(value));
            _score += value;
        }

        public void Reset()
        {
            _score = 0;
        }
    }
}