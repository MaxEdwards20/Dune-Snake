using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Client.Menu;
using System;
using Client.IO;

namespace Client.Menu
{
    public class ControlSettingsView : GameStateView
    {
        private SpriteFont m_font;
        private const string MESSAGE = "Control Settings";
        private MenuStateEnum newState = MenuStateEnum.Controls;
        private bool isKeyboardRegistered = false;
        private ControlStateEnum controlState = ControlStateEnum.SnakeLeft;
        private ControlStateEnum updatingKey = ControlStateEnum.None;
        private bool isUpdatingKey = false;

        public enum ControlStateEnum
        {
            SnakeLeft,
            SnakeRight,
            SnakeUp, 
            SnakeDown,
            None
        }


        public override void loadContent(ContentManager contentManager)
        {
            m_font = contentManager.Load<SpriteFont>("Fonts/menu");
        }

        public override MenuStateEnum processInput(GameTime gameTime)
        {
            if (!isKeyboardRegistered)
            {
                RegisterCommands();
            }
            if (isUpdatingKey)
            {
                // We are updating a key, so we need to wait for the user to press a key. Then we will update the updatingKey control key to the new value the user pressed
                foreach (var key in MenuKeyboardInput.GetPressedKeys())
                {
                    if (key != Keys.None)
                    {
                        if (key != MenuKeyboardInput.Select.key && key != MenuKeyboardInput.Escape.key) // Select is a reserved key
                        {
                            MenuKeyboardInput.updateControlKey(updatingKey, key);
                            isUpdatingKey = false;
                            updatingKey = ControlStateEnum.None;
                            RegisterCommands(); // We need to re-register the commands to update the new key
                        }
                    }
                }

            }
            MenuKeyboardInput.Update(gameTime);
            if (newState != MenuStateEnum.Controls) { 
                MenuKeyboardInput.ClearAllCommands();
                isKeyboardRegistered = false;
                var transState = newState;
                newState = MenuStateEnum.Controls;
                return transState;
            } 
            return MenuStateEnum.Controls;
        }
        public override void update(GameTime gameTime)
        {
        }
        public override void render(GameTime gameTime)
        {
            m_spriteBatch.Begin();

            Vector2 headerStringSize = m_font.MeasureString(MESSAGE);
            Vector2 maxKeyStringSize = m_font.MeasureString("Right Key / Right Rotate:  " + MenuKeyboardInput.SnakeRight.key);
            var halfWidth = m_graphics.PreferredBackBufferWidth / 2;
            var halfHeight = m_graphics.PreferredBackBufferHeight / 2;

            Drawing.DrawBlurredRectangle(m_spriteBatch, new Vector2(halfWidth - headerStringSize.X, halfHeight - headerStringSize.Y-50), new Vector2(maxKeyStringSize.X + 75, headerStringSize.Y*5), 5);

            Drawing.DrawShadedString(m_font, MESSAGE, new Vector2(halfWidth, halfHeight - headerStringSize.Y), Colors.displayColor, m_spriteBatch);
            Drawing.DrawShadedString(m_font, "Move Left  " + MenuKeyboardInput.SnakeLeft.key, new Vector2(halfWidth, halfHeight - headerStringSize.Y + 2 + 50), getStringColor(ControlStateEnum.SnakeLeft), m_spriteBatch);
            Drawing.DrawShadedString(m_font, "Move Right  " + MenuKeyboardInput.SnakeRight.key, new Vector2(halfWidth, halfHeight - headerStringSize.Y + 2 + 100), getStringColor(ControlStateEnum.SnakeRight), m_spriteBatch);
            Drawing.DrawShadedString(m_font, "Move Up  " + MenuKeyboardInput.SnakeUp.key, new Vector2(halfWidth, halfHeight - headerStringSize.Y + 2 + 150), getStringColor(ControlStateEnum.SnakeUp), m_spriteBatch);
            Drawing.DrawShadedString(m_font, "Move Down  " + MenuKeyboardInput.SnakeDown.key, new Vector2(halfWidth, halfHeight - headerStringSize.Y + 2 + 200), getStringColor(ControlStateEnum.SnakeDown), m_spriteBatch);

            m_spriteBatch.End();
        }

        private Color getStringColor(ControlStateEnum drawingState)
        {
            if (drawingState == controlState)
            {
                if (drawingState == updatingKey) { 
                    return Color.Red;
                }
                return Colors.selectedColor;
            }
            else
            {
                return Colors.displayColor;
            }
        }


        public override void RegisterCommands()
        {
            MenuKeyboardInput.registerCommand(MenuKeyboardInput.Escape, true, new IInputDevice.CommandDelegate(Escape));
            MenuKeyboardInput.registerCommand(MenuKeyboardInput.Up, true, new IInputDevice.CommandDelegate(MoveUp));
            MenuKeyboardInput.registerCommand(MenuKeyboardInput.Down, true, new IInputDevice.CommandDelegate(MoveDown));
            MenuKeyboardInput.registerCommand(MenuKeyboardInput.Select, true, new IInputDevice.CommandDelegate(Select));
            isKeyboardRegistered = true;
        }

        public void Select(GameTime gameTime, float scale)
        {
            updatingKey = controlState;
            isUpdatingKey = true;
        }

        public void Escape(GameTime gameTime, float scale)
        {
            if (isUpdatingKey)
            {
                isUpdatingKey = false;
                updatingKey = ControlStateEnum.None;
            }
            else { 
                newState = MenuStateEnum.MainMenu;
            }
        }

        public void MoveUp(GameTime gameTime, float scale)
        {
            if (controlState == ControlStateEnum.SnakeLeft)
            {
                controlState = ControlStateEnum.SnakeDown;
            }
            else if (controlState == ControlStateEnum.SnakeRight)
            {
                controlState = ControlStateEnum.SnakeLeft;
            }
            else if (controlState == ControlStateEnum.SnakeUp)
            {
                controlState = ControlStateEnum.SnakeRight;
            } else if (controlState == ControlStateEnum.SnakeDown)
            {
                controlState = ControlStateEnum.SnakeUp;
            }
        }

        public void MoveDown(GameTime gameTime, float scale)
        {
            if (controlState == ControlStateEnum.SnakeLeft)
            {
                controlState = ControlStateEnum.SnakeRight;
            }
            else if (controlState == ControlStateEnum.SnakeRight)
            {
                controlState = ControlStateEnum.SnakeUp;
            }
            else if (controlState == ControlStateEnum.SnakeUp)
            {
                controlState = ControlStateEnum.SnakeDown;
            }
            else if (controlState == ControlStateEnum.SnakeDown)
            {
                controlState = ControlStateEnum.SnakeLeft;
            }
        }
    }
}
