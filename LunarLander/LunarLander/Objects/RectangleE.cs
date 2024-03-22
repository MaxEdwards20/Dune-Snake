using Microsoft.Xna.Framework;

namespace CS5410.Objects
{
    public class RectangleE
    {
        public readonly float angle;
        public readonly int x;
        public readonly int y;
        public readonly int width;
        public readonly int height;
        public readonly Color color;
        public RectangleE(int x, int y, int width, int height, float angle, Color color) 
        {
            this.angle = angle;
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.color = color;
        }
    }
}