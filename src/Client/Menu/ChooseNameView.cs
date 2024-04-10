using Client.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Text;
using static System.Formats.Asn1.AsnWriter;

namespace Client.Menu
{
    public class ChooseNameView : GameStateView
    {
        private StringBuilder playerName;
        private SpriteFont font;
        private KeyboardState oldState;
        private GameModel m_gameModel;

        public ChooseNameView(StringBuilder name)
        {
            this.playerName = name;
            oldState = Keyboard.GetState();
        }

        public override void loadContent(ContentManager contentManager)
        {
            font = contentManager.Load<SpriteFont>("Fonts/menu"); 
        }

        public override MenuStateEnum processInput(GameTime gameTime)
        {
            
            KeyboardState newState = Keyboard.GetState(); // Get the new state
            
            // Simple example for input handling
            foreach (var key in newState.GetPressedKeys())
            {
                // Check for Escape key press to return to MainMenu
                if (newState.IsKeyDown(Keys.Escape))
                {
                    playerName.Clear(); // Clear the player name when Escape is pressed
                    return MenuStateEnum.MainMenu; // Immediately return to MainMenu when Escape is pressed
                }
                
                if (!oldState.IsKeyDown(key)) // Only take action if the key was not pressed before
                {
                    if (key == Keys.Back && playerName.Length > 0) // Handle backspace
                    {
                        playerName.Remove(playerName.Length - 1, 1);
                    }
                    else if (key == Keys.Enter && playerName.Length > 0) // Confirm with Enter key
                    {
                        
                        // Transition to the next state (e.g., HowToPlay)
                        return MenuStateEnum.HowToPlay;
                    }
                    else
                    {
                        char keyChar = GetCharFromKey(key);
                        if (keyChar != '\0')
                        {
                            playerName.Append(keyChar);
                        }
                    }
                }
            }

            oldState = newState; // Set the old state to the new state for the next frame

            return MenuStateEnum.ChooseName;
        }

        public override void render(GameTime gameTime)
        {
            m_spriteBatch.Begin();

            // Calculate the center position for "Enter Your Name" text
            string enterNameText = $"Enter Your Name: \n {playerName}";
            Vector2 textSize = font.MeasureString(enterNameText);
            Vector2 textPosition = new Vector2(
                (m_graphics.PreferredBackBufferWidth) / 2,
                (m_graphics.PreferredBackBufferHeight) / 2);

            // Draw "Enter Your Name" text

            Drawing.CustomDrawString(font, enterNameText, textPosition, Colors.displayColor, m_spriteBatch, true, true, scale:.8f);
            // m_spriteBatch.DrawString(font, enterNameText, textPosition, Colors.displayColor);


            // Draw "Press Enter to proceed" below the name text if a name has been entered
            if (playerName.Length > 0)
            {
                string proceedText = "Press Enter to proceed";
                Vector2 proceedTextSize = font.MeasureString(proceedText);
                Vector2 proceedTextPosition = new Vector2(
                    textPosition.X,
                    textPosition.Y + 90); // 20 pixels below the name text

                Drawing.CustomDrawString(font, proceedText, proceedTextPosition, Colors.displayColor, m_spriteBatch, true, false, scale:.6f);
                // m_spriteBatch.DrawString(font, proceedText, proceedTextPosition, Colors.displayColor);
            }

            m_spriteBatch.End();
        }


        // Implement this method based on your needs and localization requirements
        private char GetCharFromKey(Keys key)
        {
            // This is a simplified way to convert Keys to char. You might want to expand this method
            // to handle different cases (e.g., upper/lowercase, special characters) depending on the keyboard layout and CapsLock state.
            if (key >= Keys.A && key <= Keys.Z)
            {
                return (char)key;
            }

            return '\0'; // Return a null character for keys that don't map directly
        }

        public override void RegisterCommands()
        {

        }

        

        public override void update(GameTime gameTime)
        {
            // Implement any updating logic for this view, if necessary.
            // If there's nothing to update, you can leave it empty.
        }

    }
}
