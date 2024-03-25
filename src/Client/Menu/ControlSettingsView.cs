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
        private ControlStateEnum controlState = ControlStateEnum.RotateLeft;
        private ControlStateEnum updatingKey = ControlStateEnum.None;
        private bool isUpdatingKey = false;

        public enum ControlStateEnum
        {
            RotateLeft,
            RotateRight,
            ThrustUp, 
            ThrustDown,
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
                foreach (var key in keyboardInput.GetPressedKeys())
                {
                    if (key != Keys.None)
                    {
                        if (key != keyboardInput.Select.key && key != keyboardInput.Escape.key) // Select is a reserved key
                        {
                            keyboardInput.updateControlKey(updatingKey, key);
                            isUpdatingKey = false;
                            updatingKey = ControlStateEnum.None;
                            RegisterCommands(); // We need to re-register the commands to update the new key
                        }
                    }
                }

            }
            keyboardInput.Update(gameTime);
            if (newState != MenuStateEnum.Controls) { 
                keyboardInput.ClearAllCommands();
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
            Vector2 maxKeyStringSize = m_font.MeasureString("Right Key / Right Rotate:  " + keyboardInput.SnakeRight.key);
            var halfWidth = m_graphics.PreferredBackBufferWidth / 2;
            var halfHeight = m_graphics.PreferredBackBufferHeight / 2;

            Drawing.DrawBlurredRectangle(m_spriteBatch, new Vector2(halfWidth - headerStringSize.X, halfHeight - headerStringSize.Y-50), new Vector2(maxKeyStringSize.X + 75, headerStringSize.Y*5), 5);

            Drawing.DrawShadedString(m_font, MESSAGE, new Vector2(halfWidth, halfHeight - headerStringSize.Y), Colors.displayColor, m_spriteBatch);
            Drawing.DrawShadedString(m_font, "Move Left  " + keyboardInput.SnakeLeft.key, new Vector2(halfWidth, halfHeight - headerStringSize.Y + 2 + 50), getStringColor(ControlStateEnum.RotateLeft), m_spriteBatch);
            Drawing.DrawShadedString(m_font, "Move Right  " + keyboardInput.SnakeRight.key, new Vector2(halfWidth, halfHeight - headerStringSize.Y + 2 + 100), getStringColor(ControlStateEnum.RotateRight), m_spriteBatch);
            Drawing.DrawShadedString(m_font, "Move Up  " + keyboardInput.SnakeUp.key, new Vector2(halfWidth, halfHeight - headerStringSize.Y + 2 + 150), getStringColor(ControlStateEnum.ThrustUp), m_spriteBatch);
            Drawing.DrawShadedString(m_font, "Move Down  " + keyboardInput.SnakeDown.key, new Vector2(halfWidth, halfHeight - headerStringSize.Y + 2 + 200), getStringColor(ControlStateEnum.ThrustDown), m_spriteBatch);

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
            keyboardInput.registerCommand(keyboardInput.Escape, true, new IInputDevice.CommandDelegate(Escape));
            keyboardInput.registerCommand(keyboardInput.Up, true, new IInputDevice.CommandDelegate(MoveUp));
            keyboardInput.registerCommand(keyboardInput.Down, true, new IInputDevice.CommandDelegate(MoveDown));
            keyboardInput.registerCommand(keyboardInput.Select, true, new IInputDevice.CommandDelegate(Select));
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
            if (controlState == ControlStateEnum.RotateLeft)
            {
                controlState = ControlStateEnum.ThrustDown;
            }
            else if (controlState == ControlStateEnum.RotateRight)
            {
                controlState = ControlStateEnum.RotateLeft;
            }
            else if (controlState == ControlStateEnum.ThrustUp)
            {
                controlState = ControlStateEnum.RotateRight;
            } else if (controlState == ControlStateEnum.ThrustDown)
            {
                controlState = ControlStateEnum.ThrustUp;
            }
        }

        public void MoveDown(GameTime gameTime, float scale)
        {
            if (controlState == ControlStateEnum.RotateLeft)
            {
                controlState = ControlStateEnum.RotateRight;
            }
            else if (controlState == ControlStateEnum.RotateRight)
            {
                controlState = ControlStateEnum.ThrustUp;
            }
            else if (controlState == ControlStateEnum.ThrustUp)
            {
                controlState = ControlStateEnum.ThrustDown;
            }
            else if (controlState == ControlStateEnum.ThrustDown)
            {
                controlState = ControlStateEnum.RotateLeft;
            }
        }
    }
}
