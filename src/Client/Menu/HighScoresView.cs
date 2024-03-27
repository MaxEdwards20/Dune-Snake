﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Client.Objects;
using System.Collections.Generic;
using System;
using Client.IO;

namespace Client.Menu
{
    public class HighScoresView : GameStateView
    {
        private SpriteFont m_font;
        private const int numDisplayScores = 5;
        private string MESSAGE;
        private MenuStateEnum newState = MenuStateEnum.HighScores;
        private bool isKeyboardRegistered = false;
        private bool isLoadedScores = false;
        private GameScores highScores;

        public override void loadContent(ContentManager contentManager)
        {
            m_font = contentManager.Load<SpriteFont>("Fonts/menu");
            highScores = new GameScores();
            MESSAGE = "Your Top " + numDisplayScores + " Scores";
        }

        public override MenuStateEnum processInput(GameTime gameTime)
        {
            if (!isKeyboardRegistered)
            {
                RegisterCommands();
            }
            MenuKeyboardInput.Update(gameTime);

            if (newState != MenuStateEnum.HighScores)
            {
                MenuKeyboardInput.ClearAllCommands();
                isKeyboardRegistered = false;
                isLoadedScores = false;
                var transState = newState;
                newState = MenuStateEnum.HighScores;
                return transState;
            }
            return MenuStateEnum.HighScores;
        }
        
        public override void update(GameTime gameTime)
        {
            if (!isLoadedScores)
            {
                highScores.LoadScores();
                isLoadedScores = true;
            }
        }

        public override void render(GameTime gameTime)
        {
            m_spriteBatch.Begin();
            Drawing.DrawShadedString(m_font, MESSAGE, new Vector2(m_graphics.PreferredBackBufferWidth / 2, m_graphics.PreferredBackBufferHeight / 4), Colors.displayColor ,m_spriteBatch);
            m_spriteBatch.End();
            renderScores();
        }
        
        public override void RegisterCommands()
        {
            MenuKeyboardInput.registerCommand(MenuKeyboardInput.Escape, true, new IInputDevice.CommandDelegate(Escape));
            isKeyboardRegistered = true;
        }

        private void Escape(GameTime gameTime, float scale)
        {
            newState = MenuStateEnum.MainMenu;
        }

        private void renderScores() { 
            m_spriteBatch.Begin();
            var halfWidth = m_graphics.PreferredBackBufferWidth / 2;
            var halfHeight = m_graphics.PreferredBackBufferHeight / 2;
            float y = -100;
            if (highScores.scores == null || highScores.scores.Count == 0)
            {
                Drawing.DrawShadedString(m_font, "No Scores Yet", new Vector2(halfWidth, halfHeight), Colors.displayColor, m_spriteBatch);
            }
            else {
                var topScores = highScores.sortScores(highScores, numDisplayScores);
                foreach (var score in topScores.scores)
                {
                    y += 100;
                    Drawing.DrawShadedString(m_font, score.score.ToString(), new Vector2(halfWidth, halfHeight + y), Colors.displayColor, m_spriteBatch);
                }
            }

            m_spriteBatch.End();
        }
    }
}
