using Microsoft.Xna.Framework;

namespace Shared.Components.Appearance
{
    public class Head : Component
    {
        public string texture { get; private set; }
        public Color color { get; private set; }
        public Head(Color color)
        {
            this.color = color;
            this.texture = "Textures/head";
        }
    }
}