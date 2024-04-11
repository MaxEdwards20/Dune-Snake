using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace CS5410
{
    public class ParticleSystem
    {
        private Dictionary<long, Particle> m_particles = new Dictionary<long, Particle>();
        public Dictionary<long, Particle>.ValueCollection particles => m_particles.Values;
        private MyRandom m_random = new MyRandom();

        private Vector2 m_center;
        private int m_sizeMean; // pixels
        private int m_sizeStdDev;   // pixels
        private float m_speedMean;  // pixels per millisecond
        private float m_speedStDev; // pixels per millisecond
        private float m_lifetimeMean; // milliseconds
        private float m_lifetimeStdDev; // milliseconds

        public ParticleSystem(Vector2 center, int sizeMean, int sizeStdDev, float speedMean, float speedStdDev, int lifetimeMean, int lifetimeStdDev)
        {
            m_center = center;
            m_sizeMean = sizeMean;
            m_sizeStdDev = sizeStdDev;
            m_speedMean = speedMean;
            m_speedStDev = speedStdDev;
            m_lifetimeMean = lifetimeMean;
            m_lifetimeStdDev = lifetimeStdDev;
        }

        public void FoodEaten(Vector2 foodPosition)
        {
            Color[] particleColors = { Color.Green, Color.Yellow, Color.White };
            for (int i = 0; i < 30; i++) // Smaller number of particles for food eaten
            {
                float size = (float)m_random.nextGaussian(m_sizeMean / 2, m_sizeStdDev / 2); // Smaller particles
                Color initialColor = particleColors[m_random.Next(particleColors.Length)];
                Vector2 direction = m_random.nextCircleVector();

                var particle = new Particle(
                    foodPosition,
                    direction * 0.3f, // Less forceful ejection
                    (float)m_random.nextGaussian(m_speedMean / 2, m_speedStDev / 2), // Slower speed
                    new Vector2(size, size),
                    TimeSpan.FromMilliseconds(m_random.nextGaussian(m_lifetimeMean / 3, m_lifetimeStdDev / 3)), // Shorter lifetime

                    initialColor
                );
                m_particles.Add(particle.name, particle);
            }
        }

        public void SnakeDeath(Vector2 snakePosition)
        {
            Color[] particleColors = { Color.Red, Color.DarkRed, Color.Maroon };
            for (int i = 0; i < 200; i++) // More particles for death to emphasize the event
            {
                float size = (float)m_random.nextGaussian(m_sizeMean, m_sizeStdDev);
                Color initialColor = particleColors[m_random.Next(particleColors.Length)];
                Vector2 direction = m_random.nextCircleVector() * 2; // More spread

                var particle = new Particle(
                    snakePosition,
                    direction, // Greater spread
                    (float)m_random.nextGaussian(m_speedMean, m_speedStDev), // Normal speed
                    new Vector2(size, size),
                    TimeSpan.FromMilliseconds(m_random.nextGaussian(m_lifetimeMean, m_lifetimeStdDev)), // Normal lifetime

                    initialColor
                );
                m_particles.Add(particle.name, particle);
            }
        }




        public void update(GameTime gameTime)
        {

            List<long> removeMe = new List<long>();
            foreach (var p in m_particles.Values)
            {
                if (!p.update(gameTime))
                {
                    removeMe.Add(p.name);
                }
            }


            foreach (var key in removeMe)
            {
                m_particles.Remove(key);
            }
        }
    }
}
