#region Includes

// Includes
using Microsoft.Xna.Framework;
using Multiplayer_Games_Programming_Framework.Core;
using Multiplayer_Games_Programming_Framework.Core.Scene;
using Multiplayer_Games_Programming_Framework.Core.Utilities;
using Myra;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;
using nkast.Aether.Physics2D.Dynamics;
using System.Diagnostics;

namespace Multiplayer_Games_Programming_Framework.GameCode.Scenes;

#endregion

internal class MenuScene : Scene
{
    #region Member Variables

    // Member Variables
    private Desktop m_desktop;

    private TextButton m_loginButton;
    private bool m_moveTitleLeft = true;
    private TextButton m_playButton;
    private Vector2 m_TitlePosition = new(-200, -200);

    #endregion

    #region Constructor

    public MenuScene(SceneManager manager) : base(manager)
    {
        m_Manager.m_Game.Window.Title = "Box by RyanLabs";
        manager.m_Game.IsMouseVisible = true;
    }

    protected override World CreateWorld()
    {
        return null;
    }

    protected override Camera CreateCamera()
    {
        return new Camera(Vector2.Zero);
    }

    protected override string SceneName()
    {
        return "Main Menu";
    }

    public override void LoadContent()
    {
        MyraEnvironment.Game = m_Manager.m_Game;

        m_playButton = new TextButton();
        m_loginButton = new TextButton();

        var grid = new Grid
        {
            ShowGridLines = false,
            RowSpacing = 8,
            ColumnSpacing = 8
        };

        const int cols = 4;
        for (var i = 0; i < cols; ++i) grid.ColumnsProportions.Add(new Proportion(ProportionType.Part));

        const int rows = 5;
        for (var i = 0; i < rows; ++i) grid.RowsProportions.Add(new Proportion(ProportionType.Part));

        m_desktop = new Desktop
        {
            Root = grid
        };

        // Create the Login button
        m_loginButton.Text = "Login";
        m_loginButton.GridRow = 3;
        m_loginButton.GridColumn = 1;
        m_loginButton.GridColumnSpan = 1;
        m_loginButton.PressedBackground = new SolidBrush(Color.DarkRed);
        m_loginButton.Border = new SolidBrush(Color.Black);
        m_loginButton.BorderThickness = Thickness.FromString("5");
        m_loginButton.TextColor = Color.Green;
        m_loginButton.HorizontalAlignment = HorizontalAlignment.Center;
        m_loginButton.VerticalAlignment = VerticalAlignment.Center;

        m_loginButton.Width = Graphics.Viewport.Width / cols * m_loginButton.GridColumnSpan;
        m_loginButton.Height = Graphics.Viewport.Height / rows * m_loginButton.GridRowSpan;
        grid.Widgets.Add(m_loginButton);

        // Create the Play button
        m_playButton.Text = "Play";
        m_playButton.GridRow = 3;
        m_playButton.GridColumn = 2;
        m_playButton.GridColumnSpan = 1;
        m_playButton.PressedBackground = new SolidBrush(Color.DarkRed);
        m_playButton.Border = new SolidBrush(Color.Black);
        m_playButton.BorderThickness = Thickness.FromString("5");
        m_playButton.TextColor = Color.Red;
        m_playButton.HorizontalAlignment = HorizontalAlignment.Center;
        m_playButton.VerticalAlignment = VerticalAlignment.Center;
        m_playButton.Width = Graphics.Viewport.Width / cols * m_loginButton.GridColumnSpan;
        m_playButton.Height = Graphics.Viewport.Height / rows * m_loginButton.GridRowSpan;
        m_playButton.Enabled = false;

        grid.Widgets.Add(m_playButton);

        // Load the username scene when the play button is clicked
        m_playButton.Click += (s, a) => { m_Manager.LoadScene(new UserNameScene(m_Manager)); };

        var childPanel = new Panel
        {
            GridColumn = 0,
            GridRow = 0
        };

        grid.Widgets.Add(childPanel);

        m_loginButton.Click += (s, a) =>
        {
            // Try to connect to the server
            if (NetworkManager.m_Instance != null && NetworkManager.m_Instance.Connect("127.0.0.1", 4444))
            {
                NetworkManager.m_Instance.Login();
                m_loginButton.Enabled = false;
                m_loginButton.TextColor = Color.Red;
            }
            else
            {
                Debug.WriteLine("Failed to connect");
                m_playButton.Enabled = false;
                m_playButton.TextColor = Color.Red;
                m_loginButton.TextColor = Color.Green;
                m_loginButton.Enabled = true;
            }
        };
    }

    #endregion

    #region Update Methods

    public override void Update(float deltaTime)
    {
        // Move the title back and forth
        m_TitlePosition = m_moveTitleLeft
            ? new Vector2(m_TitlePosition.X - 1, m_TitlePosition.Y)
            : new Vector2(m_TitlePosition.X + 1, m_TitlePosition.Y);

        m_moveTitleLeft = m_TitlePosition.X switch
        {
            <= -300 => false,
            >= -100 => true,
            _ => m_moveTitleLeft
        };

        // Enable the play button if the player is connected to the server
        if (NetworkManager.m_Instance != null && NetworkManager.m_Instance.m_connected && m_Manager.m_Game.IsActive)
        {
            m_playButton.Enabled = true;
            m_playButton.TextColor = Color.Green;
        }
        else
        {
            m_playButton.Enabled = false;
        }

        if (!m_Manager.m_Game.IsActive)
        {
            m_loginButton.Enabled = false;
        }
        else
        {
            if (NetworkManager.m_Instance != null && !NetworkManager.m_Instance.m_connected)
                m_loginButton.Enabled = true;
            else
                m_loginButton.Enabled = false;
        }

        base.Update(deltaTime);
    }

    // Draw the title UI element
    public override void Draw(float deltaTime)
    {
        m_Manager.m_Game.m_SpriteBatch.Draw(m_Manager.m_Game.m_mainMenuTitle, m_TitlePosition, Color.White);

        base.Draw(deltaTime);
        m_desktop.Render();
    }

    #endregion
}