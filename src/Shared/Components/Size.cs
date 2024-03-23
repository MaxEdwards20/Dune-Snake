

using Microsoft.Xna.Framework;

namespace Shared.Components
{
    public class Size : Component
    {
        public Size(Vector2 size)
        {
            this.size = size;
        }
        public Vector2 size {  get; private set; }
    }
}
