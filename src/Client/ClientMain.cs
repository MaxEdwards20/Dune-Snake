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
using System.Text;
using Client.Components;
using Client.Systems;
using Shared.Components;
using Shared.Systems;

namespace Client
{
    public class ClientMain : Game
    {
        private GraphicsDeviceManager m_graphics;
        private SpriteBatch m_spriteBatch;
        private IGameState m_currentState;
        private Dictionary<MenuStateEnum, IGameState> m_states;
        private MenuKeyboardInput m_menuKeyboardInput;
        private bool newState;
        private SoundEffect selectSound;
        private Texture2D m_background;
        private GameModel m_gameModel;
        private Controls m_controls;
        private ControlsPersistence m_ControlsPersistence;
        private StringBuilder playerName = new StringBuilder();

        public ClientMain()
        {
            m_graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            m_menuKeyboardInput = new MenuKeyboardInput();
            IsMouseVisible = true;
            m_gameModel = new GameModel(playerName);
            m_controls = new Controls();
            m_ControlsPersistence = new ControlsPersistence();
        }

        protected override void Initialize()
        {
            // For Graders: You can change the resolution here
            // m_graphics.PreferredBackBufferWidth = 1920;
            // m_graphics.PreferredBackBufferHeight = 1080;
            m_graphics.PreferredBackBufferWidth = 1000;
            m_graphics.PreferredBackBufferHeight = 750;
            m_graphics.ApplyChanges();
            
            // Load the controls
            // We pass in our own controls so we always have them as a default if they were not saved
            m_ControlsPersistence.LoadControls(m_controls); 

            // Create all the game states here
            m_states = new Dictionary<MenuStateEnum, IGameState>
            {
                { MenuStateEnum.MainMenu, new MainMenuView() },
                { MenuStateEnum.GamePlay, new GamePlayView(m_controls, playerName) }, 
                { MenuStateEnum.HighScores, new HighScoresView() },
                { MenuStateEnum.Controls, new ControlSettingsView(m_controls) },
                { MenuStateEnum.Help, new HelpView() },
                { MenuStateEnum.Credits, new AboutView() },
                { MenuStateEnum.ChooseName, new ChooseNameView(playerName)},
                { MenuStateEnum.HowToPlay, new HowToPlayView() }
            };

            // Give all game states a chance to initialize, other than constructor
            foreach (var item in m_states)
            {
                item.Value.initialize(this.GraphicsDevice, m_graphics, m_menuKeyboardInput);
            }
            
            // We are starting with the main menu
            m_currentState = m_states[MenuStateEnum.MainMenu];

            base.Initialize();
        }

        protected override void LoadContent()
        {
            m_spriteBatch = new SpriteBatch(GraphicsDevice);
            
            // Load background music 
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
                    m_currentState.initialize();
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
