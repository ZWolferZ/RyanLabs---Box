#region Includes

// Includes
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Multiplayer_Games_Programming_Framework.Core.Scene;
using Multiplayer_Games_Programming_Framework.Core.Utilities;
using System;
using System.Threading;

#endregion

namespace Multiplayer_Games_Programming_Framework.Core
{
    public class MainGame : Game
    {
        #region Member Variables

        // Scene Manager stuff
        private readonly GraphicsDeviceManager m_graphics;

        private SceneManager m_sceneManager;

        // Float for measuring time between heartbeats
        private float m_heartbeatTimer;

        // Textures for the UI
        // Cant use an array for this for some reason
        public Texture2D m_mainMenuTitle;

        public Texture2D m_enterUsername = null;
        public Texture2D m_score0Texture = null;
        public Texture2D m_score1Texture = null;
        public Texture2D m_score2Texture = null;
        public Texture2D m_score3Texture = null;
        public Texture2D m_scoreNeg1Texture = null;
        public Texture2D m_scoreNeg2Texture = null;
        public Texture2D m_scoreNeg3Texture = null;
        public Texture2D m_yourOpp = null;
        public SpriteBatch m_SpriteBatch { get; private set; }
        public SpriteFont m_Spritefont { get; private set; }
        public SpriteFont m_userNameFont { get; private set; }

        #endregion

        #region Constructor

        public MainGame()
        {
            m_graphics = new GraphicsDeviceManager(this);
            m_graphics.PreferredBackBufferWidth = Graphics.InitScreenWidth;
            m_graphics.PreferredBackBufferHeight = Graphics.InitScreenHeight;
            IsFixedTimeStep = true;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        #endregion

        #region Methods

        protected override void Initialize()
        {
            Graphics.GraphicsDevice = m_graphics.GraphicsDevice;
            base.Initialize();
        }

        // Load sprites and fonts into memory
        protected override void LoadContent()
        {
            m_SpriteBatch = new SpriteBatch(GraphicsDevice);
            m_Spritefont = Content.Load<SpriteFont>("DefaultFont");
            m_userNameFont = Content.Load<SpriteFont>("UserNameSceneFont");
            m_mainMenuTitle = Content.Load<Texture2D>("MainMenuTitle");
            m_enterUsername = Content.Load<Texture2D>("EnterUsername");
            m_yourOpp = Content.Load<Texture2D>("YourOpp");
            m_score0Texture = Content.Load<Texture2D>("Score0");
            m_score1Texture = Content.Load<Texture2D>("Score1");
            m_score2Texture = Content.Load<Texture2D>("Score2");
            m_score3Texture = Content.Load<Texture2D>("Score3");
            m_scoreNeg1Texture = Content.Load<Texture2D>("Score-1");
            m_scoreNeg2Texture = Content.Load<Texture2D>("Score-2");
            m_scoreNeg3Texture = Content.Load<Texture2D>("Score-3");

            m_sceneManager = new SceneManager(this);
        }

        // Main Update loop
        protected override void Update(GameTime gameTime)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            // Update the heartbeat timer
            m_heartbeatTimer += deltaTime;

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                // I used to send a disconnect message here, but it fed bad data to the server,
                // so I will just use the Heartbeat packet to close the connection.
                Quit();

            // If the network manager is quitting, then quit the game
            if (NetworkManager.m_Instance != null && NetworkManager.m_Instance.m_quit) Quit();

            // Send a heartbeat every 5 seconds
            if (m_heartbeatTimer >= 5)
            {
                m_heartbeatTimer = 0;

                if (NetworkManager.m_Instance != null && NetworkManager.m_Instance.m_clientID != -1)
                {
                    NetworkManager.m_Instance.UdpSendPacket(new Heartbeat(NetworkManager.m_Instance.m_clientID), true); // Bah
                    NetworkManager.m_Instance.TcpSendPacket(new Heartbeat(NetworkManager.m_Instance.m_clientID), true); // Bump
                    // The joke is that the heartbeat makes that noise
                }
            }

            // Update the scene manager
            m_sceneManager.Update(deltaTime);

            base.Update(gameTime);
        }

        // Override the OnExiting method to make sure the network is closed properly
        protected override void OnExiting(object sender, EventArgs args)
        {
            // If the client crashes or closes the program any other way then ESC,
            // I need to make sure the network is closed properly.
            // Otherwise, the game will not shut down properly.
            Quit();
            base.OnExiting(sender, args);
        }

        // Quit the game
        private void Quit()
        {
            if (NetworkManager.m_Instance != null) NetworkManager.m_Instance.Close();
            Thread.Sleep(100); // Wait for the network to close
            Exit();
        }

        // Main Draw loop
        protected override void Draw(GameTime gameTime)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            GraphicsDevice.Clear(Color.Red);
            m_SpriteBatch.Begin(SpriteSortMode.FrontToBack, transformMatrix: Camera.m_Matrix);
            m_sceneManager.Draw(deltaTime);
            m_SpriteBatch.End();
            base.Draw(gameTime);
        }

        #endregion
    }
}