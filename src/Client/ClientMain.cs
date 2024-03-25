using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Client.Menu;
using Client.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;

namespace Client
{
    public class ClientMain : Game
    {
        private GraphicsDeviceManager m_graphics;
        private SpriteBatch m_spriteBatch;
        private IGameState m_currentState;
        private Dictionary<MenuStateEnum, IGameState> m_states;
        private KeyboardInput m_keyboardInput;
        private bool newState = false;
        private SoundEffect selectSound;
        private Texture2D m_background;

        public ClientMain()
        {
            m_graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            m_keyboardInput = new KeyboardInput();
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // For Graders: You can change the resolution here
            m_graphics.PreferredBackBufferWidth = 1920;
            m_graphics.PreferredBackBufferHeight = 1080;
            m_graphics.ApplyChanges();

            // Create all the game states here
            m_states = new Dictionary<MenuStateEnum, IGameState>
            {
                { MenuStateEnum.MainMenu, new MainMenuView() },
                { MenuStateEnum.GamePlay, new GamePlayView() }, 
                { MenuStateEnum.HighScores, new HighScoresView() },
                { MenuStateEnum.Controls, new ControlSettingsView() },
                { MenuStateEnum.Help, new HelpView() },
                { MenuStateEnum.Credits, new AboutView() }
            };

            // Give all game states a chance to initialize, other than constructor
            foreach (var item in m_states)
            {
                item.Value.initialize(this.GraphicsDevice, m_graphics, m_keyboardInput);
            }
            
            // We are starting with the main menu
            m_currentState = m_states[MenuStateEnum.MainMenu];

            base.Initialize();
        }

        protected override void LoadContent()
        {
            m_spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load background music TODO: Pick a background song
            var backgroundMusic = Content.Load<Song>("Audio/backgroundMusic");
            MediaPlayer.Play(backgroundMusic);
            MediaPlayer.IsRepeating = true;

            // Load select sound
            selectSound = Content.Load<SoundEffect>("Audio/menuSelect");

            // Load background
            m_background = Content.Load<Texture2D>("Images/MenuBackground");
            // Give all game states a chance to load their content
            foreach (var item in m_states)
            {
                item.Value.loadContent(this.Content);
            }

            MessageQueueClient.instance.initialize("localhost", 3000);
        }

        protected override void Update(GameTime gameTime)
        {
            newState = false;
            MenuStateEnum nextStateEnum = m_currentState.processInput(gameTime);
            // Special case for exiting the game
            if (nextStateEnum == MenuStateEnum.Exit)
            {
                Exit();
            }
            else
            {
                m_currentState.update(gameTime);
                var cState = m_currentState;
                m_currentState = m_states[nextStateEnum];
                if (cState != m_currentState)
                {
                    newState = true;
                    m_currentState.initializeSession();
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // GraphicsDevice.Clear(Color.CornflowerBlue);
            if (newState)
            {
                selectSound.Play();
            }
            // Draw the menu background
            m_spriteBatch.Begin();
            m_spriteBatch.Draw(m_background, new Rectangle(0, 0, m_graphics.PreferredBackBufferWidth, m_graphics.PreferredBackBufferHeight), Color.White);
            m_spriteBatch.End();
            m_currentState.render(gameTime);
            base.Draw(gameTime);
        }
    }
}
