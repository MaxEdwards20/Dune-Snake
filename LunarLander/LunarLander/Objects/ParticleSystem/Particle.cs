using Microsoft.Xna.Framework;
using System;

namespace CS5410.Objects
{
    public class Particle
    {
        public Particle(Vector2 center, Vector2 direction, float speed, Vector2 size, TimeSpan lifetime)
        {
            this.name = m_nextName++;
            this.center = center;
            this.direction = direction;
            this.speed = speed;
            this.size = size;
            this.lifetime = lifetime;
            this.rotation = 0;
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

            // Return true if this particle is still alive
            return alive < lifetime;
        }

        public long name;
        public Vector2 size;
        public Vector2 center;
        public float rotation;
        private Vector2 direction;
        private float speed;
        private TimeSpan lifetime;
        private TimeSpan alive = TimeSpan.Zero;
        private static long m_nextName = 0;
    }
}
