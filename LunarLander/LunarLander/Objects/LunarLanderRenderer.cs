
using System.Security.AccessControl;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using Microsoft.Xna.Framework.Audio;
using CS5410.Menu;

namespace CS5410.Objects
{
    public class LunarLanderRenderer
    {
        private Texture2D m_texture;
        private LunarLander lander;
        private SpriteFont m_font;
        private Vector2 m_scalingFactor;
        private Vector2 m_origin;
        private SoundEffect m_crashAudio;
        private SoundEffect m_thrustAudio;
        private SoundEffect m_landingAudio;
        private Color speedColor = Color.White;
        private Color fuelColor = Color.White;
        private Color landerColor = Color.White;
        private Color angleColor = Color.White;
        private bool hasPlayedSafeLandingSound = false;
        private bool hasPlayedCrashSound = false;
        public LanderParticleSystem m_landerParticleSystem;
        public LunarLanderRenderer(LunarLander lander)
        {
            this.lander = lander;
        }
        public void reset()
        {
            hasPlayedSafeLandingSound = false;
            hasPlayedCrashSound = false;
            if (m_landerParticleSystem != null)
                m_landerParticleSystem.reset();
        }
        public void loadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            m_texture = contentManager.Load<Texture2D>("Images/LunarLander");
            m_font = contentManager.Load<SpriteFont>("Fonts/menu");
            m_origin = new Vector2(m_texture.Width / 2, m_texture.Height / 2);
            m_scalingFactor = new Vector2(lander.size.X / m_texture.Width, lander.size.Y / m_texture.Height);
            loadAudio(contentManager);
            // particle system
            m_landerParticleSystem = new LanderParticleSystem();
            m_landerParticleSystem.loadContent(contentManager, graphicsDevice);
        }
        public void update(GameTime gameTime)
        {
            if (lander.isCrashed){ m_landerParticleSystem.ShipCrash(lander.m_position); }
            if (lander.isThrusting){ m_landerParticleSystem.ShipThrust(lander.m_position, lander.m_rotation); } else {m_landerParticleSystem.StopThrust();}
            m_landerParticleSystem.update(gameTime);
            updateColors();
        }

        public void render(GameTime gameTime, SpriteBatch spriteBatch )
        {
            m_landerParticleSystem.render(spriteBatch);
            spriteBatch.Begin();
            renderSound();
            renderStats(spriteBatch);
            if (!lander.isCrashed) { 
                spriteBatch.Draw(m_texture, lander.m_position, null, landerColor, lander.m_rotation, m_origin, m_scalingFactor, SpriteEffects.None, 0f);
            }
            spriteBatch.End();
        }

        private void updateColors() { 
            var goodColor = Color.Green;
            var badColor = Color.White;
            speedColor = lander.isGoodVelocity() ? goodColor : badColor;
            angleColor = lander.isGoodAngle() ? goodColor : badColor;
            fuelColor = lander.m_currentFuel > 0 ? goodColor : badColor;
           
            if (lander.isCrashed) { 
                landerColor = Color.Red;
            }
            else if (lander.isLandedSafely) {
                landerColor = goodColor;
            }
            else {
                landerColor = Color.White;
            }
        }
        private void renderSound() { 
            if (lander.isCrashed && !hasPlayedCrashSound) { 
                // play the landerCrash sound
                m_crashAudio.Play();
                hasPlayedCrashSound = true;
            } else if (lander.isLandedSafely && !hasPlayedSafeLandingSound) { 
                // play the landerSuccess sound
                m_landingAudio.Play();
                hasPlayedSafeLandingSound = true;
            } else if (lander.isThrusting) { 
                var soundInstance = m_thrustAudio.CreateInstance();
                soundInstance.Volume = 0.5f;
                soundInstance.Play();
            }
        }

        private void renderStats(SpriteBatch spriteBatch) { 
            Drawing.DrawShadedString(m_font, "Fuel: " + Math.Round(lander.m_currentFuel, 2), new Vector2(20, 10), fuelColor, spriteBatch, false);
            Drawing.DrawShadedString(m_font, "Speed: " + Math.Round(lander.m_velocity.Length() * 4, 2), new Vector2(20, 60), speedColor, spriteBatch, false);
            Drawing.DrawShadedString(m_font, "Angle: " + Math.Round(LunarLander.radiansToDegrees(lander.m_rotation), 2), new Vector2(20, 110), angleColor, spriteBatch, false);
        }
        private void loadAudio(ContentManager contentManager) { 
            m_crashAudio = contentManager.Load<SoundEffect>("Audio/landerCrash");
            m_thrustAudio = contentManager.Load<SoundEffect>("Audio/thruster");
            m_landingAudio = contentManager.Load<SoundEffect>("Audio/success");
        }
    
    }
}