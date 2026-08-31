using SplashKitSDK;
using SplashKitNUnit.Game.Models;

namespace SplashKitNUnit.Game
{
    public class Program
    {
        public static void Main()
        {
            Window window = new Window("SplashKit NUnit Demo", 800, 600);
            Player player = new Player(100, 300);
            ScoreCalculator score = new ScoreCalculator();

            // Coin position (for demo)
            float coinX = 350, coinY = 250, coinRadius = 20;
            Random rnd = new Random();

            while (!window.CloseRequested)
            {
                SplashKit.ProcessEvents();

                // Handle input
                if (SplashKit.KeyTyped(KeyCode.SpaceKey))
                {
                    player.Jump();
                }

                // Update
                float dt = 0.016f; // ~60 FPS fixed timestep for simplicity
                player.Update(dt);

                // Check coin collection (circle-rect collision with player bounding box)
                float playerWidth = 40, playerHeight = 50;
                if (CollisionMath.CircleRectCollision(coinX, coinY, coinRadius,
                    player.X, player.Y, playerWidth, playerHeight))
                {
                    score.CollectCoin(10);
                    player.CollectCoin();

                    // Respawn coin at random location
                    coinX = rnd.Next(50, 750);
                    coinY = rnd.Next(50, 450);
                }

                // Draw
                window.Clear(Color.White);

                // Draw player (rectangle)
                window.FillRectangle(Color.Blue, player.X, player.Y, playerWidth, playerHeight);

                // Draw coin (circle)
                window.FillCircle(Color.Gold, coinX, coinY, coinRadius);

                // Draw score
                window.DrawText($"Score: {score.Score}", Color.Black, 10, 10);
                window.DrawText($"Coins: {player.CoinsCollected}", Color.Black, 10, 30);

                window.Refresh(60);
            }

            window.Close();
        }
    }
}