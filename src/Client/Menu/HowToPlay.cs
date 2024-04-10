using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Client.Menu
{
    public class HowToPlayView : GameStateView
    {
        private SpriteFont font;
        private string howToPlayMessage =
            "Control a snake to eat food and grow. Use arrow keys for movement. \n" +
            "(Custom controls in Main Menu) \n" +
            "Avoid wall and other players!. How long can you grow? Try to beat the high score!";
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
                return MenuStateEnum.Connecting; // Transition to the gameplay state
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

            // Define text scale
            Vector2 textScale = new Vector2(0.5f, 0.5f); 

            // Title
            Vector2 titlePosition = new Vector2(m_graphics.PreferredBackBufferWidth / 2, m_graphics.PreferredBackBufferHeight / 4);
            Vector2 titleOrigin = font.MeasureString(titleMessage) / 2;
            m_spriteBatch.DrawString(font, titleMessage, titlePosition - (titleOrigin * textScale), Color.PaleGoldenrod, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);

            // How to Play Instructions
            Vector2 instructionsPosition = new Vector2(m_graphics.PreferredBackBufferWidth / 2, m_graphics.PreferredBackBufferHeight / 2.5f);
            string[] lines = howToPlayMessage.Split('\n');
            foreach (string line in lines)
            {
                Vector2 lineSize = font.MeasureString(line) * textScale;
                m_spriteBatch.DrawString(font, line, instructionsPosition - new Vector2(lineSize.X / 2, 0), Color.PaleGoldenrod, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);
                instructionsPosition.Y += lineSize.Y + 5; // Adjust spacing between lines if necessary, taking scale into account
            }

            // Continue Prompt
            Vector2 continuePosition = new Vector2(m_graphics.PreferredBackBufferWidth / 2, (m_graphics.PreferredBackBufferHeight / 4) * 3);
            Vector2 continueOrigin = font.MeasureString(continueMessage) / 2;
            m_spriteBatch.DrawString(font, continueMessage, continuePosition - (continueOrigin * textScale), Color.PaleGoldenrod, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);

            m_spriteBatch.End();
        }


        public override void RegisterCommands()
        {
            // If you have commands to register, such as an Exit command, register them here
            isKeyboardRegistered = true;
        }
    }
}
