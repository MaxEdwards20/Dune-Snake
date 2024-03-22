﻿using System.ComponentModel;
using CS5410.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace CS5410.Menu
{
    public abstract class GameStateView : IGameState
    {
        protected GraphicsDeviceManager m_graphics;
        protected SpriteBatch m_spriteBatch;
        protected KeyboardInput keyboardInput;
        public void initialize(GraphicsDevice graphicsDevice, GraphicsDeviceManager graphics, KeyboardInput keyboardInput)
        {
            m_graphics = graphics;
            m_spriteBatch = new SpriteBatch(graphicsDevice);
            this.keyboardInput = keyboardInput;
        }
        public abstract void loadContent(ContentManager contentManager);
        public abstract MenuStateEnum processInput(GameTime gameTime);
        public abstract void render(GameTime gameTime);
        public abstract void update(GameTime gameTime);
        public abstract void RegisterCommands();
    }
}
