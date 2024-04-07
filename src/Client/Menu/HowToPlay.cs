using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Client.Menu
{
    public class HowToPlayView : GameStateView
    {
        private SpriteFont font;
        private string howToPlayMessage = "The snake will change direction based on keypress";
        private string titleMessage = "HOW TO PLAY";
        private string continueMessage = "Press Enter to continue";
        private KeyboardState oldState;
        private bool isEnterReleased;
        private bool isKeyboardRegistered = false;
        private double enterKeyDelay = 500; // 500 milliseconds delay
        private double timeSinceLastEnterPress;

        // ... other members ...

        public override void loadContent(ContentManager contentManager)
        {
            font = contentManager.Load<SpriteFont>("Fonts/menu");
            oldState = Keyboard.GetState(); // Initialize the old state
            timeSinceLastEnterPress = 0; // Initialize the timer
        }

        public override MenuStateEnum processInput(GameTime gameTime)
        {
            KeyboardState newState = Keyboard.GetState();

            timeSinceLastEnterPress += gameTime.ElapsedGameTime.TotalMilliseconds;

            // Proceed to the next game state if the player presses Enter and enough time has passed since the last Enter press
            if (newState.IsKeyDown(Keys.Enter) && oldState.IsKeyUp(Keys.Enter) && timeSinceLastEnterPress >= enterKeyDelay)
            {
                return MenuStateEnum.GamePlay; // Transition to the gameplay state
            }
            // Update the enter released state
            if (!newState.IsKeyDown(Keys.Enter))
            {
                isEnterReleased = true;
            }

            oldState = newState; // Update the old keyboard state
            return MenuStateEnum.HowToPlay;
        }

        public override void update(GameTime gameTime)
        {
            // No updating logic needed for static view
        }

        public override void render(GameTime gameTime)
        {
            m_spriteBatch.Begin();

            // Title
            Vector2 titlePosition = new Vector2(m_graphics.PreferredBackBufferWidth / 2, m_graphics.PreferredBackBufferHeight / 4);
            Vector2 titleOrigin = font.MeasureString(titleMessage) / 2;
            m_spriteBatch.DrawString(font, titleMessage, titlePosition - titleOrigin, Color.White);

            // How to Play Instructions
            Vector2 instructionsPosition = new Vector2(m_graphics.PreferredBackBufferWidth / 2, m_graphics.PreferredBackBufferHeight / 2);
            Vector2 instructionsOrigin = font.MeasureString(howToPlayMessage) / 2;
            m_spriteBatch.DrawString(font, howToPlayMessage, instructionsPosition - instructionsOrigin, Color.White);

            // Continue Prompt
            Vector2 continuePosition = new Vector2(m_graphics.PreferredBackBufferWidth / 2, (m_graphics.PreferredBackBufferHeight / 4) * 3);
            Vector2 continueOrigin = font.MeasureString(continueMessage) / 2;
            m_spriteBatch.DrawString(font, continueMessage, continuePosition - continueOrigin, Color.White);

            m_spriteBatch.End();
        }

        public override void RegisterCommands()
        {
            // If you have commands to register, such as an Exit command, register them here
            isKeyboardRegistered = true;
        }
    }
}
