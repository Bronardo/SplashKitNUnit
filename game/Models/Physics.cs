namespace SplashKitNUnit.Game.Models
{
    public static class Physics
    {
        public const float Gravity = 980f;          // pixels per second²
        public const float JumpVelocity = -400f;    // upward velocity (negative Y)

        /// <summary>
        /// Updates vertical velocity by applying gravity over delta time.
        /// </summary>
        public static float ApplyGravity(float velocity, float deltaTime)
        {
            return velocity + Gravity * deltaTime;
        }

        /// <summary>
        /// Returns the jump velocity (instantaneous).
        /// </summary>
        public static float GetJumpVelocity()
        {
            return JumpVelocity;
        }
    }
}