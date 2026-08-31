namespace SplashKitNUnit.Game.Models
{
    public static class CollisionMath
    {
        /// <summary>
        /// Checks if a circle and rectangle overlap.
        /// </summary>
        public static bool CircleRectCollision(
            float cx, float cy, float radius,
            float rx, float ry, float rw, float rh)
        {
            float closestX = Math.Clamp(cx, rx, rx + rw);
            float closestY = Math.Clamp(cy, ry, ry + rh);

            float dx = cx - closestX;
            float dy = cy - closestY;

            return (dx * dx + dy * dy) < (radius * radius);
        }

        /// <summary>
        /// Checks if two axis-aligned rectangles overlap.
        /// </summary>
        public static bool RectRectCollision(
            float x1, float y1, float w1, float h1,
            float x2, float y2, float w2, float h2)
        {
            return x1 < x2 + w2 && x1 + w1 > x2 &&
                   y1 < y2 + h2 && y1 + h1 > y2;
        }
    }
}