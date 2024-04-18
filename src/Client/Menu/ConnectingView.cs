
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Client.Menu
{
    public class ConnectingView : GameStateView
    {
        private SpriteFont font;
        private string connectingMessage = "Connecting to Server";
        private bool isConnected = false;
        private double elapsedTimeSinceLastAttempt = 1500; // Start at 1500 so we only need to wait 500ms before first attempt
        private const double attemptInterval = 2000; // Attempt to connect every 2 seconds
        private double periodUpdateTime = 500; // Update period for visual

        public override void loadContent(ContentManager contentManager)
        {
            font = contentManager.Load<SpriteFont>("Fonts/menu");
        }

        public override MenuStateEnum processInput(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                resetValues();
                return MenuStateEnum.MainMenu; // Or another state if cancelling connection attempt
            }
            if (isConnected)
            {
                resetValues();
                return MenuStateEnum.GamePlay; // Transition to GamePlay upon successful connection
            }
            return MenuStateEnum.Connecting; // Stay on Connecting view if not yet connected
        }
        
        private void resetValues()
        {
            connectingMessage = "Connecting to Server";
            isConnected = false;
            elapsedTimeSinceLastAttempt = 1500;
        }

        public override void update(GameTime gameTime)
        {
            elapsedTimeSinceLastAttempt += gameTime.ElapsedGameTime.TotalMilliseconds;

            // Attempt to connect every 2 seconds
            if (!isConnected && elapsedTimeSinceLastAttempt >= attemptInterval)
            {
                isConnected = connectToServer();
                elapsedTimeSinceLastAttempt = 0; // Reset timer after each attempt
            }
        }

        public override void render(GameTime gameTime)
        {
            m_spriteBatch.Begin();
            Vector2 position = new Vector2(m_graphics.PreferredBackBufferWidth / 2, m_graphics.PreferredBackBufferHeight / 2);
            Vector2 origin = font.MeasureString(connectingMessage) / 2;
            Drawing.CustomDrawString(font, connectingMessage, position, Colors.displayColor, m_spriteBatch, true, true, true);
            m_spriteBatch.End();
        }

        private bool connectToServer()
        {
            // Attempt to connect to the server
            return MessageQueueClient.instance.initialize("fd58:e39:96f6:1:1066:8dfb:2e75:5b45", 3000);
            // return MessageQueueClient.instance.initialize("localhost", 3000);
        }

        public override void RegisterCommands()
        {
            throw new NotImplementedException();
        }

    }
}
