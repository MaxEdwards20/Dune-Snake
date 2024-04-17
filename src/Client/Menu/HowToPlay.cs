using System.Text;
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
            "Control a worm to eat spice and grow.\nUse configured keys for movement." +
            "\nAvoid walls and other players!\nTry to beat the high score!" +
            "\n(Custom controls in Main Menu)\n";
        private string titleMessage = "HOW TO PLAY";
        private string continueMessage = "Press Enter to continue";
        private KeyboardState oldState;
        private bool isEnterReleased;
        private bool isKeyboardRegistered = false;
        private double enterKeyDelay = 500; // 500 milliseconds delay
        private double timeSinceLastEnterPress;
        private StringBuilder playerName = new StringBuilder();

        // ... other members ...

        public HowToPlayView(StringBuilder playerName)
        {
            this.playerName = playerName;
        }

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
            
            // Check for Escape key press to return to MainMenu
            if (newState.IsKeyDown(Keys.Escape))
            {
                timeSinceLastEnterPress = 0; // Initialize the timer
                return MenuStateEnum.ChooseName; 
            }

            // Proceed to the next game state if the player presses Enter and enough time has passed since the last Enter press
            if (newState.IsKeyDown(Keys.Enter) && oldState.IsKeyUp(Keys.Enter) && timeSinceLastEnterPress >= enterKeyDelay)
            {
                timeSinceLastEnterPress = 0; // Initialize the timer
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
            
            // Background Rectangle
            var recPosition = new Vector2(m_graphics.PreferredBackBufferWidth / 5 - 20,
                m_graphics.PreferredBackBufferHeight / 4 - 50);
            Drawing.DrawBlurredRectangle(m_spriteBatch, recPosition, new Vector2(700, 400), 5, 0.9f);


            // Title
            Vector2 titlePosition = new Vector2(m_graphics.PreferredBackBufferWidth / 2, m_graphics.PreferredBackBufferHeight / 4);

            Vector2 titleOrigin = font.MeasureString(titleMessage) / 2;
            Drawing.CustomDrawString(font, titleMessage, titlePosition, Colors.displayColor, m_spriteBatch, true, false, scale: 1.0f);
            // m_spriteBatch.DrawString(font, titleMessage, titlePosition - (titleOrigin * textScale), Colors.displayColor, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);
            
            // How to Play Instructions
            Vector2 instructionsPosition = new Vector2(m_graphics.PreferredBackBufferWidth / 5, m_graphics.PreferredBackBufferHeight / 2.5f);
            string[] lines = howToPlayMessage.Split('\n');
            foreach (string line in lines)
            {
                Vector2 lineSize = font.MeasureString(line) * textScale;

                Drawing.CustomDrawString(font, line, instructionsPosition,
                    Colors.displayColor, m_spriteBatch, false, false, scale: 0.75f);
                // m_spriteBatch.DrawString(font, line, instructionsPosition - new Vector2(lineSize.X / 2, 0), Colors.displayColor, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);
                instructionsPosition.Y += lineSize.Y + 20; // Adjust spacing between lines if necessary, taking scale into account
            }

            // Continue Prompt
            Vector2 continuePosition = new Vector2(m_graphics.PreferredBackBufferWidth / 2, (m_graphics.PreferredBackBufferHeight / 4) * 3 + 50);
            Vector2 continueOrigin = font.MeasureString(continueMessage) / 2;

            Drawing.CustomDrawString( font, continueMessage, continuePosition, Colors.displayColor, m_spriteBatch, true, true, scale: 1f);
            // m_spriteBatch.DrawString(font, continueMessage, continuePosition - (continueOrigin * textScale), Colors.displayColor, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);


            m_spriteBatch.End();
        }


        public override void RegisterCommands()
        {
            // If you have commands to register, such as an Exit command, register them here
            isKeyboardRegistered = true;
        }
    }
}
