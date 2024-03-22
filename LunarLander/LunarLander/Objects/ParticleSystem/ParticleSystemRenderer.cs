using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace CS5410.Objects
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

        public void draw(SpriteBatch spriteBatch, IParticleSystem system)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            Rectangle r = new Rectangle(0, 0, 0, 0);
            Vector2 centerTexture = new Vector2(m_texParticle.Width / 2, m_texParticle.Height / 2);
            foreach (Particle particle in system.particles)
            {
                r.X = (int)particle.center.X;
                r.Y = (int)particle.center.Y;
                r.Width = (int)particle.size.X;
                r.Height = (int)particle.size.Y;

                spriteBatch.Draw(
                    m_texParticle,
                    r,
                    null,
                    Color.White,
                    particle.rotation,
                    centerTexture,
                    SpriteEffects.None,
                    0);
            }

            spriteBatch.End();
        }
    }
}
