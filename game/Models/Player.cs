namespace SplashKitNUnit.Game.Models
{
    public class Player
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float VelocityY { get; set; }
        public bool IsOnGround { get; set; }
        public int CoinsCollected { get; private set; }

        public Player(float startX, float startY)
        {
            X = startX;
            Y = startY;
            VelocityY = 0f;
            IsOnGround = true;
            CoinsCollected = 0;
        }

        public void Jump()
        {
            if (!IsOnGround) return;
            VelocityY = Physics.GetJumpVelocity();
            IsOnGround = false;
        }

        public void Update(float deltaTime)
        {
            VelocityY = Physics.ApplyGravity(VelocityY, deltaTime);
            Y += VelocityY * deltaTime;

            // Simple ground collision at Y = 500 (ground plane)
            const float groundY = 500f;
            if (Y >= groundY)
            {
                Y = groundY;
                VelocityY = 0f;
                IsOnGround = true;
            }
        }

        public void CollectCoin()
        {
            CoinsCollected++;
        }
    }
}