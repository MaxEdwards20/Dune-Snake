using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace CS5410.Objects {
    public class LanderParticleSystem { 
        private ParticleSystem m_particleSystemFire;
        private ParticleSystem m_particleSystemSmoke;
        private ThrustSystem m_particleSystemThrust;
        private ParticleSystemRenderer m_renderFire;
        private ParticleSystemRenderer m_renderSmoke;
        private ParticleSystemRenderer m_renderThrust;
        private bool shipCrash = false;
        private bool isThrusting = false;

        public Dictionary<long, Particle>.ValueCollection particles => throw new NotImplementedException();

        public LanderParticleSystem()
        {}
        public void ShipCrash(Vector2 location) { 
            shipCrash = true;
            if (m_particleSystemFire == null && m_particleSystemSmoke == null) // only create the particle system once
                createFireAndSmoke(location);
        }

        public void ShipThrust(Vector2 location, float rotation) { 
            // the lnader rotation is 90 degrees counterclockwise of where we want it
            rotation += (float)(Math.PI / 2);
            isThrusting = true;
            if (m_particleSystemThrust == null)
                createThrust(location, rotation );
            else
                m_particleSystemThrust.updateThrustLocationRotation(location, rotation);
        }

        public void StopThrust() { 
            isThrusting = false;
        }

        public void reset()
        {
            m_particleSystemFire = null;
            m_particleSystemSmoke = null;
            m_particleSystemThrust = null;
            shipCrash = false;
            isThrusting = false;
        }

        public void loadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            loadFireAndSmoke(contentManager);
            loadThrust(contentManager);
        }

        public void update(GameTime gameTime)
        {
            if (shipCrash) { 
                m_particleSystemFire.update(gameTime );
                m_particleSystemSmoke.update(gameTime);
            }
            if (m_particleSystemThrust != null)
            {
                m_particleSystemThrust.m_isActive = isThrusting;
                m_particleSystemThrust.update(gameTime);
            }
        }

        public void render(SpriteBatch spriteBatch) { 
            renderFireAndSmoke(spriteBatch);
            renderThrust(spriteBatch);
        }
        private void loadFireAndSmoke(ContentManager contentManager) { 
            m_renderFire = new ParticleSystemRenderer("Images/fire");
            m_renderFire.LoadContent(contentManager);
            m_renderSmoke = new ParticleSystemRenderer("Images/smoke-2");
            m_renderSmoke.LoadContent(contentManager);
        }
        private void createFireAndSmoke(Vector2 location) { 
            m_particleSystemFire = new ParticleSystem(
                location,
                5, 4,
                0.12f, 0.05f,
                1500, 500);
            m_particleSystemSmoke = new ParticleSystem(
                location,
                10, 4,
                0.07f, 0.05f,
                2000, 1000);
        }
        private void renderFireAndSmoke(SpriteBatch spriteBatch) {
            if (shipCrash) { 
                m_renderFire.draw(spriteBatch, m_particleSystemFire);
                m_renderSmoke.draw(spriteBatch, m_particleSystemSmoke);
            }
        }

        private void loadThrust(ContentManager contentManager) { 
            m_renderThrust = new ParticleSystemRenderer("Images/square");
            m_renderThrust.LoadContent(contentManager);
        }
        private void createThrust(Vector2 location, float rotation) { 
            m_particleSystemThrust = new ThrustSystem(
                location,
                2, 2,
                0.3f, 0.08f,
                400, 100, rotation);
            }

        private void renderThrust(SpriteBatch spriteBatch) { 
            if (m_particleSystemThrust != null){
                m_renderThrust.draw(spriteBatch, m_particleSystemThrust);
            }
        }
    }
}