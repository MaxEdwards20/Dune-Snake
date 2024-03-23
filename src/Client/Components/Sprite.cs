
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Components
{
    public class Sprite : Shared.Components.Component
    {
        public Sprite(Texture2D texture)
        {
            this.texture = texture;
            center = new Vector2(texture.Width / 2, texture.Height / 2);
        }

        public Texture2D texture { get; private set; }
        public Vector2 center { get; private set; }
    }
}
