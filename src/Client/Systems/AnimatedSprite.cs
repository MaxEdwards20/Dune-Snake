using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace CS5410
{
    public class AnimatedSprite
    {
        private Texture2D m_spriteSheet;
        private int[] m_spriteTime;

        private TimeSpan m_animationTime;
        private int m_subImageIndex;
        private int m_subImageWidth;

        public AnimatedSprite(Texture2D spriteSheet, int[] spriteTime)
        {
            this.m_spriteSheet = spriteSheet;
            this.m_spriteTime = spriteTime;

            m_subImageWidth = spriteSheet.Width / spriteTime.Length;
        }

        public void update(GameTime gameTime)
        {
            m_animationTime += gameTime.ElapsedGameTime;
            if (m_animationTime.TotalMilliseconds >= m_spriteTime[m_subImageIndex])
            {
                m_animationTime -= TimeSpan.FromMilliseconds(m_spriteTime[m_subImageIndex]);
                m_subImageIndex++;
                m_subImageIndex = m_subImageIndex % m_spriteTime.Length;
            }
        }

        public void draw(SpriteBatch spriteBatch, Objects.AnimatedSprite model)
        {
            spriteBatch.Draw(
                m_spriteSheet,
                new Rectangle((int)model.Center.X - m_subImageWidth / 2, (int)model.Center.Y - m_spriteSheet.Height / 2, (int)model.Size.X, (int)model.Size.Y), // Destination rectangle
                new Rectangle(m_subImageIndex * m_subImageWidth, 0, m_subImageWidth, m_spriteSheet.Height), // Source sub-texture
                Color.White,
                model.Rotation, // Angular rotation
                new Vector2(m_subImageWidth / 2, m_spriteSheet.Height / 2), // Center point of rotation
                SpriteEffects.None, 0);
        }
    }
}
