using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace CS5410.Objects
{
    public class LunarLander {
        public Vector2 m_position { get; private set; }
        public Vector2 size { get; private set; }
        public float m_rotation { get; private set; } // angle in radians
        public float m_currentFuel { get; private set; }
        public bool isLandedSafely { get; private set; }
        public bool isCrashed { get; private set; }
        public bool isThrusting { get; private set; }
        public Vector2 m_velocity { get; private set; }
        private GraphicsDeviceManager m_graphics;
        private LunarLanderRenderer m_landerRenderer;
        private Vector2 gravity { get; set; }
        private readonly float m_moveRate = .001f;
        private readonly float m_fuelBurnRate = 0.01f;
        private readonly double m_rotationRate = Math.PI / 5000;


        public LunarLander(Vector2 size,  GraphicsDeviceManager graphics)
        {
            m_graphics = graphics;
            this.size = size;
            m_landerRenderer = new LunarLanderRenderer(this);
            newGame();
        }

        public void loadContent(ContentManager contentManager)
        {
            m_landerRenderer.loadContent(contentManager, m_graphics.GraphicsDevice);
        }

        public void render(GameTime gameTime, SpriteBatch m_spriteBatch)
        {
            m_landerRenderer.render(gameTime, m_spriteBatch);
            isThrusting = false; // reset the thrust flag
        }

        public void update(GameTime gameTime)
        {
            if (isThrusting) { 
                AddThrust(gameTime);
            }
            m_velocity += gravity * gameTime.ElapsedGameTime.Milliseconds; // Apply gravity
            m_position += m_velocity; 
            m_landerRenderer.update(gameTime);
        }

        public void reset()
        {
            newGame();
        }

        public void handleCollision(bool isOnSafeZone) {
            if (isLandedSafely || isCrashed) return; // we don't want to handle the collision twice
            if (isOnSafeZone && isGoodVelocity() && isGoodAngle())
            {
                isLandedSafely = true;
            }
            else { 
                isCrashed = true;
            }
            m_velocity = new Vector2(0, 0); // stop the lander
            gravity = new Vector2(0, 0); // stop the gravity
            isThrusting = false; // stop the thrust if there is any
        }
        
        public bool isGoodVelocity() { 
            // Check if the velocity is within the safe zone. This is an arbitrary value that looked good for the balance between thrust and gravity
            return m_velocity.Length() < 0.5;
        }

        public bool isGoodAngle() { 
            var currentDegrees = radiansToDegrees(m_rotation);
            return currentDegrees >= 355 || currentDegrees <= 5;
        }

        public void MoveRight(GameTime gameTime, float scale)
        {
            if (isCrashed || isLandedSafely) return;
            m_rotation += (float)(m_rotationRate * gameTime.ElapsedGameTime.Milliseconds);
        }

        public void MoveLeft(GameTime gameTime, float scale)
        {
            if (isCrashed || isLandedSafely) return;
            m_rotation -= (float)(m_rotationRate * gameTime.ElapsedGameTime.Milliseconds);
        }

        public void MoveUp(GameTime gameTime, float scale)
        {
            if (isCrashed || isLandedSafely) return;
            // First we need to check if we have enough fuel to apply the thrust
            if (m_currentFuel > 0)
            {
                isThrusting = !isThrusting;
            }
        }

        private void AddThrust(GameTime gameTime)
        {
            m_velocity += RotateVector(new Vector2(0, -(float)(m_moveRate * gameTime.ElapsedGameTime.Milliseconds)), m_rotation);
            m_currentFuel -= m_fuelBurnRate * gameTime.ElapsedGameTime.Milliseconds;
            // Make sure we don't go below 0
            if (m_currentFuel < 0) 
            {
                m_currentFuel = 0;
            }
        }

        private Vector2 RotateVector(Vector2 vector, float angle) { 
            float cosTheta = (float)Math.Cos(angle);
            float sinTheta = (float)Math.Sin(angle);
            float x = vector.X * cosTheta - vector.Y * sinTheta;
            float y = vector.X * sinTheta + vector.Y * cosTheta;
            return new Vector2(x,y);
        }


        public static double radiansToDegrees(float radians)
        {
            var d =  radians * 180 / Math.PI;
            if (d < 0)
            {
                d += 360;
            }
            return d;
        }


        private void newGame() { 
            Random random = new Random();
            m_position = new Vector2(random.Next((int)size.X, m_graphics.PreferredBackBufferWidth / 4), random.Next((int)size.Y, m_graphics.PreferredBackBufferHeight / 4));
            m_velocity = new Vector2(0, 0);
            m_rotation = (float)Math.PI / 2;
            m_currentFuel = 100;
            isLandedSafely = false;
            isCrashed = false;
            isThrusting = false;
            gravity = new Vector2(0, 0.0002f);
            m_landerRenderer.reset();
        }
    }
}