using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace CS5410
{
    public class ParticleSystemRenderer
    {
        private string m_nameParticleContent;
        private Texture2D m_texParticle;

        public ParticleSystemRenderer(string nameParticleContent)
        {
            m_nameParticleContent = nameParticleContent;
        }

        public void LoadContent(ContentManager content)
        {
            m_texParticle = content.Load<Texture2D>(m_nameParticleContent);
        }

        public void draw(SpriteBatch spriteBatch, ParticleSystem system)
        {

            Vector2 origin = new Vector2(m_texParticle.Width / 2, m_texParticle.Height / 2);
            foreach (Particle particle in system.particles)
            {
                // Scale the particle size relative to the texture size
                Vector2 scale = new Vector2(particle.size.X / m_texParticle.Width, particle.size.Y / m_texParticle.Height);

                spriteBatch.Draw(
                    m_texParticle,
                    particle.center,
                    null, // Full source rectangle
                    particle.color, // Use particle's color
                    particle.rotation,
                    origin, // Centered origin
                    scale, // Scale to particle size
                    SpriteEffects.None,
                    0);
            }

            
        }
    }
}
