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

        public void ShipThrust(Vector2 landerCenter, Vector2 direction, float landerRotation, float landerSize)
        {
            Vector2 thrustOffset = new Vector2(0, landerSize / 2);

            Matrix rotationMatrix = Matrix.CreateRotationZ(landerRotation);
            thrustOffset = Vector2.Transform(thrustOffset, rotationMatrix);

            Vector2 thrustStartPosition = landerCenter + thrustOffset;

            Color[] particleColors = new Color[] { Color.Red, Color.Orange, Color.Yellow };

            Vector2[] trianglePoints = new Vector2[]
            {
        new Vector2(-0.5f, 1), 
        new Vector2(0.5f, 1),
        new Vector2(0, 0)
            };

            float triangleScale = 5.0f; 

            // Scale the triangle points
            for (int i = 0; i < trianglePoints.Length; i++)
            {
                trianglePoints[i] *= triangleScale;
            }

            
            for (int i = 0; i < 20; i++) 
            {
                float size = (float)m_random.nextGaussian(m_sizeMean, m_sizeStdDev);
                size *= 4;

                Color initialColor = particleColors[m_random.Next(particleColors.Length)];

                Vector2 randomPoint = thrustStartPosition + trianglePoints[m_random.Next(trianglePoints.Length)];
                Vector2 targetDirection = randomPoint - thrustStartPosition;

               
                float angleVariation = MathHelper.ToRadians(10); 
                float randomAngle = (float)(m_random.NextDouble() * angleVariation - angleVariation / 2);
                targetDirection = Vector2.Transform(targetDirection, Matrix.CreateRotationZ(randomAngle));

                targetDirection.Y *= 1;

                var particle = new Particle(
                    thrustStartPosition,
                    direction + targetDirection * 0.2f, 
                    (float)m_random.nextGaussian(m_speedMean, m_speedStDev) * 0.5f, 
                    new Vector2(size, size), 
                    TimeSpan.FromMilliseconds(m_random.nextGaussian(m_lifetimeMean / 2, m_lifetimeStdDev / 2)),
                    initialColor
                );
                m_particles.Add(particle.name, particle);
            }
        }





        public void ShipCrash(Vector2 position)
        {
            Color[] particleColors = { Color.Red, Color.Orange, Color.Yellow };
            for (int i = 0; i < 500; i++) 
            {
                float size = (float)m_random.nextGaussian(m_sizeMean, m_sizeStdDev);
                Color initialColor = particleColors[m_random.Next(particleColors.Length)];
                Vector2 direction = m_random.nextCircleVector() * 0.5f;

                var particle = new Particle(
                    position,
                    direction, // Less spread out direction
                    (float)m_random.nextGaussian(m_speedMean, m_speedStDev) * 0.3f, // Slower speed for a more condensed effect
                    new Vector2(size, size),
                    TimeSpan.FromMilliseconds(m_random.nextGaussian(m_lifetimeMean, m_lifetimeStdDev)),
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
