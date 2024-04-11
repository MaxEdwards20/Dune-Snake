using Microsoft.Xna.Framework;
using System;

namespace CS5410
{
    public class Particle
    {
        public long name;
        public Vector2 size;
        public Vector2 center;
        public float rotation;
        public Color color; // Added color property

        private Vector2 direction;
        private float speed;
        private TimeSpan lifetime;
        private TimeSpan alive = TimeSpan.Zero;
        private static long m_nextName = 0;

        // Extended constructor to include color
        public Particle(Vector2 center, Vector2 direction, float speed, Vector2 size, TimeSpan lifetime, Color initialColor)
        {
            this.name = m_nextName++;
            this.center = center;
            this.direction = direction;
            this.speed = speed;
            this.size = size;
            this.lifetime = lifetime;
            this.rotation = 0;
            this.color = initialColor; // Initialize color
        }

        public bool update(GameTime gameTime)
        {
            // Update how long it has been alive
            alive += gameTime.ElapsedGameTime;

            // Update its center
            center.X += (float)(gameTime.ElapsedGameTime.TotalMilliseconds * speed * direction.X);
            center.Y += (float)(gameTime.ElapsedGameTime.TotalMilliseconds * speed * direction.Y);

            // Rotate proportional to its speed
            rotation += (speed / 0.5f);

            // Example color fading logic: fade out by reducing the alpha value over time
            float lifeFraction = (float)alive.TotalMilliseconds / (float)lifetime.TotalMilliseconds;
            color = new Color(color.R, color.G, color.B, (byte)MathHelper.Clamp(255 * (1 - lifeFraction), 0, 255));

            // Return true if this particle is still alive
            return alive < lifetime;
        }
    }
}