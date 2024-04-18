
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Components
{
    public class Sprite : Shared.Components.Component
    {
        public Sprite(Texture2D texture, bool isAnimated = false, int row = 0)
        {
            this.texture = texture;
            center = new Vector2(texture.Width / 2, texture.Height / 2);
            this.isAnimated = isAnimated;
            if (isAnimated)
            {
                Row = row;
                FrameCount = 4;
                RowCount = 8;
                FrameSpeed = 1.1f;
                IsLooping = true;
                CurrentFrame = 0;
                Timer = 0f;
                SourceRect = new Rectangle(0, Row * texture.Height/ RowCount , texture.Width / FrameCount, texture.Height/ RowCount);
            }
        }
        // For normal sprites
        public Texture2D texture { get; private set; }
        public Vector2 center { get; private set; }
        // For animated sprites
        public int Row { get; private set; }
        public int RowCount { get; private set; }
        public int FrameCount { get; private set; }
        public bool isAnimated { get; private set; }
        public Rectangle SourceRect { get; set; }
        public int CurrentFrame { get; set; }
        public float FrameSpeed { get; set; }
        public float Timer { get; set; }
        public bool IsLooping { get; set; }
        
    }
}
