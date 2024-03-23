using Microsoft.Xna.Framework;

namespace Shared.Components
{
    public class Position : Component
    {
        public Position(Vector2 position, float orientation = 0.0f)
        {
            this.position = position;
            this.orientation = orientation;
        }

        public Vector2 position { get; set; }
        public float orientation { get; set; }
    }
}
