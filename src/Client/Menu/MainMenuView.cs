using System;
using Client.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Client.Menu
{
    public class MainMenuView : GameStateView
    {
        private SpriteFont m_fontMenu;
        private SpriteFont m_fontMenuSelect;
        private enum MenuState
        {
            NewGame,
            HighScores,
            Controls,
            Help,
            Credits,
            Quit
        }
        private MenuState m_currentSelection = MenuState.NewGame;
        private bool renderSound = false;
        private MenuStateEnum newState = MenuStateEnum.MainMenu;
        private bool isKeyboardRegistered = false;
        private SoundEffect m_selectSound;
        public override void loadContent(ContentManager contentManager)
        {
            m_fontMenu = contentManager.Load<SpriteFont>("Fonts/menu");
            m_fontMenuSelect = contentManager.Load<SpriteFont>("Fonts/menu-select");
            m_selectSound = contentManager.Load<SoundEffect>("Audio/menu");
        }

        public override MenuStateEnum processInput(GameTime gameTime)
        {
            // This is the technique I'm using to ensure one keypress makes one menu navigation move
            // If our menu state has changed, we need to reregister our keys 
            if (!isKeyboardRegistered )
            {
                RegisterCommands();
            }
            MenuKeyboardInput.Update(gameTime);
            if (newState != MenuStateEnum.MainMenu)
            {
                MenuKeyboardInput.ClearAllCommands();
                isKeyboardRegistered = false;
                var transState = newState;
                newState = MenuStateEnum.MainMenu;
                return transState;
            }
            return MenuStateEnum.MainMenu;
        }

        public override void update(GameTime gameTime)
        {
        }
        public override void render(GameTime gameTime)
        {
            m_spriteBatch.Begin();
            if (renderSound)
            {
                m_selectSound.Play();
                renderSound = false; // reset the flag
            }
            // I split the first one's parameters on separate lines to help you see them better
            float bottom = drawMenuItem(
                m_currentSelection == MenuState.NewGame ? m_fontMenuSelect : m_fontMenu, 
                "New Game",
                200, 
                m_currentSelection == MenuState.NewGame ? Colors.selectedColor : Colors.displayColor);
            bottom = drawMenuItem(m_currentSelection == MenuState.HighScores ? m_fontMenuSelect : m_fontMenu, "High Scores", bottom, m_currentSelection == MenuState.HighScores ? Colors.selectedColor : Colors.displayColor);
            bottom = drawMenuItem(m_currentSelection == MenuState.Controls ? m_fontMenuSelect : m_fontMenu, "Controls", bottom, m_currentSelection == MenuState.Controls ? Colors.selectedColor : Colors.displayColor);
            bottom = drawMenuItem(m_currentSelection == MenuState.Help ? m_fontMenuSelect : m_fontMenu, "Help", bottom, m_currentSelection == MenuState.Help ? Colors.selectedColor : Colors.displayColor);
            bottom = drawMenuItem(m_currentSelection == MenuState.Credits ? m_fontMenuSelect : m_fontMenu, "Credits", bottom, m_currentSelection == MenuState.Credits ? Colors.selectedColor : Colors.displayColor);
            drawMenuItem(m_currentSelection == MenuState.Quit ? m_fontMenuSelect : m_fontMenu, "Quit", bottom, m_currentSelection == MenuState.Quit ? Colors.selectedColor : Colors.displayColor);
            m_spriteBatch.End();
        }
        private float drawMenuItem(SpriteFont font, string text, float y, Color color)
        {
            Vector2 stringSize = font.MeasureString(text);
            Drawing.DrawShadedString(font, text, new Vector2(m_graphics.PreferredBackBufferWidth / 2, y), color, m_spriteBatch);
            return y + stringSize.Y;
        }

        public override void RegisterCommands()
        {
            MenuKeyboardInput.registerCommand(MenuKeyboardInput.Up, true, new IInputDevice.CommandDelegate(MoveUp));
            MenuKeyboardInput.registerCommand(MenuKeyboardInput.Down, true, new IInputDevice.CommandDelegate(MoveDown));
            MenuKeyboardInput.registerCommand(MenuKeyboardInput.Select, true, new IInputDevice.CommandDelegate(Select));
            isKeyboardRegistered = true;
        }

        private void Select(GameTime gameTime, float scale)
        {
            if (m_currentSelection == MenuState.NewGame)
            {
                newState = MenuStateEnum.GamePlay;
            }
            if (m_currentSelection == MenuState.HighScores)
            {
                newState = MenuStateEnum.HighScores;
            }
            if (m_currentSelection == MenuState.Controls)
            {
                newState = MenuStateEnum.Controls;
            }
            if (m_currentSelection == MenuState.Help)
            {
                newState = MenuStateEnum.Help;
            }
            if (m_currentSelection == MenuState.Credits)
            {
                newState = MenuStateEnum.Credits;
            }
            if (m_currentSelection == MenuState.Quit)
            {
                newState = MenuStateEnum.Exit;
            }
        }
        private  void MoveUp(GameTime gameTime, float scale)
        {
            if (m_currentSelection > 0)
            {
                m_currentSelection -= 1;
            } else {
                m_currentSelection = MenuState.Quit;
            }
            renderSound = true;
        }
        private  void MoveDown(GameTime gameTime, float scale)
        {
            if (m_currentSelection < MenuState.Quit)
            {
                m_currentSelection += 1;
            }
            else { 
                m_currentSelection = MenuState.NewGame;
            }
            renderSound = true;
        }
    }
}