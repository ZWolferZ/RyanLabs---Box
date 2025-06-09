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

namespace Multiplayer_Games_Programming_Framework.GameCode.Scenes;

#endregion

internal class LobbySelect : Scene
{
    #region Member Varibles

    // Member Varibles
    private readonly TextButton m_backtoUsername = new();

    private readonly TextButton m_controls = new();
    private readonly TextButton m_lobby1Button = new();
    private readonly TextButton m_lobby2Button = new();
    private readonly TextButton m_lobby3Button = new();
    private Desktop m_desktop;

    #endregion

    #region Constructor

    public LobbySelect(SceneManager manager) : base(manager)
    {
        manager.m_Game.IsMouseVisible = true;
        // Send message to server to get the size of Lobby 1,2,3
        if (NetworkManager.m_Instance == null) return;
        NetworkManager.m_Instance.TcpSendPacket(new LobbySize(-1, 1), true);

        NetworkManager.m_Instance.TcpSendPacket(new LobbySize(-1, 2), true);

        NetworkManager.m_Instance.TcpSendPacket(new LobbySize(-1, 3), true);

        // Set the player to logged in (You need to be logged in to reach this scene)
        NetworkManager.m_Instance.m_loggedin = true;
    }

    protected override Camera CreateCamera()
    {
        return new Camera(Vector2.Zero);
    }

    protected override World CreateWorld()
    {
        return null;
    }

    protected override string SceneName()
    {
        return "Lobby Select";
    }

    public override void LoadContent()
    {
        MyraEnvironment.Game = m_Manager.m_Game;

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

        // Create the join lobby buttons
        m_lobby1Button.Text = "Join Lobby 1";
        m_lobby1Button.GridRow = 1;
        m_lobby1Button.GridColumn = 1;
        m_lobby1Button.GridColumnSpan = 2;
        m_lobby1Button.Border = new SolidBrush(Color.Black);
        m_lobby1Button.BorderThickness = Thickness.FromString("5");
        m_lobby1Button.PressedBackground = new SolidBrush(Color.DarkRed);
        m_lobby1Button.HorizontalAlignment = HorizontalAlignment.Center;
        m_lobby1Button.VerticalAlignment = VerticalAlignment.Center;
        m_lobby1Button.Width = Graphics.Viewport.Width / cols * m_lobby1Button.GridColumnSpan;
        m_lobby1Button.Height = Graphics.Viewport.Height / rows * m_lobby1Button.GridRowSpan;
        grid.Widgets.Add(m_lobby1Button);

        m_lobby1Button.Click += (s, a) =>
        {
            // If the lobby size is  0 or 1, join and increment the lobby size by 1
            if (NetworkManager.m_Instance != null && NetworkManager.m_Instance.m_lobby1Size != 0 && NetworkManager.m_Instance.m_lobby1Size != 1) return;
            if (NetworkManager.m_Instance != null)
            {
                NetworkManager.m_Instance.m_lobby1Size++;

                // Send the new lobby size to the server
                NetworkManager.m_Instance.TcpSendPacket(new LobbySize(NetworkManager.m_Instance.m_lobby1Size, 1), true);
                NetworkManager.m_Instance.m_inLobby1 = true;
            }

            m_Manager.LoadScene(new Lobby1(m_Manager));
        };

        m_lobby2Button.Text = "Join Lobby 2";
        m_lobby2Button.GridRow = 2;
        m_lobby2Button.GridColumn = 1;
        m_lobby2Button.GridColumnSpan = 2;
        m_lobby2Button.Border = new SolidBrush(Color.Black);
        m_lobby2Button.BorderThickness = Thickness.FromString("5");
        m_lobby2Button.PressedBackground = new SolidBrush(Color.DarkRed);
        m_lobby2Button.HorizontalAlignment = HorizontalAlignment.Center;
        m_lobby2Button.VerticalAlignment = VerticalAlignment.Center;
        m_lobby2Button.Width = Graphics.Viewport.Width / cols * m_lobby2Button.GridColumnSpan;
        m_lobby2Button.Height = Graphics.Viewport.Height / rows * m_lobby2Button.GridRowSpan;
        grid.Widgets.Add(m_lobby2Button);

        m_lobby2Button.Click += (s, a) =>
        {
            // If the lobby size is  0 or 1, join and increment the lobby size by 1
            if (NetworkManager.m_Instance != null && NetworkManager.m_Instance.m_lobby2Size != 0 && NetworkManager.m_Instance.m_lobby2Size != 1) return;
            if (NetworkManager.m_Instance != null)
            {
                NetworkManager.m_Instance.m_lobby2Size++;

                // Send the new lobby size to the server
                NetworkManager.m_Instance.TcpSendPacket(new LobbySize(NetworkManager.m_Instance.m_lobby2Size, 2), true);
                NetworkManager.m_Instance.m_inLobby2 = true;
            }

            m_Manager.LoadScene(new Lobby2(m_Manager));
        };

        m_lobby3Button.Text = "Join Lobby 3";
        m_lobby3Button.GridRow = 3;
        m_lobby3Button.GridColumn = 1;
        m_lobby3Button.GridColumnSpan = 2;
        m_lobby3Button.Border = new SolidBrush(Color.Black);
        m_lobby3Button.BorderThickness = Thickness.FromString("5");
        m_lobby3Button.PressedBackground = new SolidBrush(Color.DarkRed);
        m_lobby3Button.HorizontalAlignment = HorizontalAlignment.Center;
        m_lobby3Button.VerticalAlignment = VerticalAlignment.Center;
        m_lobby3Button.Width = Graphics.Viewport.Width / cols * m_lobby3Button.GridColumnSpan;
        m_lobby3Button.Height = Graphics.Viewport.Height / rows * m_lobby3Button.GridRowSpan;
        grid.Widgets.Add(m_lobby3Button);

        m_lobby3Button.Click += (s, a) =>
        {
            // If the lobby size is  0 or 1, join and increment the lobby size by 1
            if (NetworkManager.m_Instance != null && NetworkManager.m_Instance.m_lobby3Size != 0 && NetworkManager.m_Instance.m_lobby3Size != 1) return;
            if (NetworkManager.m_Instance != null)
            {
                NetworkManager.m_Instance.m_lobby3Size++;

                // Send the new lobby size to the server
                NetworkManager.m_Instance.TcpSendPacket(new LobbySize(NetworkManager.m_Instance.m_lobby3Size, 3), true);
                NetworkManager.m_Instance.m_inLobby3 = true;
            }

            m_Manager.LoadScene(new Lobby3(m_Manager));
        };

        m_backtoUsername.Text = "Change Username";
        m_backtoUsername.GridRow = 4;
        m_backtoUsername.GridColumn = 3;
        m_backtoUsername.GridColumnSpan = 1;
        m_backtoUsername.Border = new SolidBrush(Color.Black);
        m_backtoUsername.BorderThickness = Thickness.FromString("5");
        m_backtoUsername.PressedBackground = new SolidBrush(Color.DarkRed);
        m_backtoUsername.HorizontalAlignment = HorizontalAlignment.Center;
        m_backtoUsername.VerticalAlignment = VerticalAlignment.Center;
        m_backtoUsername.Width = Graphics.Viewport.Width / cols * m_backtoUsername.GridColumnSpan;
        m_backtoUsername.Height = Graphics.Viewport.Height / rows * m_backtoUsername.GridRowSpan;
        grid.Widgets.Add(m_backtoUsername);

        // Change the scene to the username scene
        m_backtoUsername.Click += (s, a) => { m_Manager.LoadScene(new UserNameScene(m_Manager)); };

        m_controls.Text = "Controls";
        m_controls.GridRow = 3;
        m_controls.GridColumn = 3;
        m_controls.GridColumnSpan = 1;
        m_controls.Border = new SolidBrush(Color.Black);
        m_controls.BorderThickness = Thickness.FromString("5");
        m_controls.PressedBackground = new SolidBrush(Color.DarkRed);
        m_controls.HorizontalAlignment = HorizontalAlignment.Center;
        m_controls.VerticalAlignment = VerticalAlignment.Center;
        m_controls.Width = Graphics.Viewport.Width / cols * m_controls.GridColumnSpan;
        m_controls.Height = Graphics.Viewport.Height / rows * m_controls.GridRowSpan;
        grid.Widgets.Add(m_controls);

        // Change the scene to the controls scene
        m_controls.Click += (s, a) => { m_Manager.LoadScene(new ControlsScene(m_Manager)); };
    }

    #endregion

    #region Update Methods

    public override void Update(float deltaTime)
    {
        // If the game is not active, disable the buttons
        if (!m_Manager.m_Game.IsActive)
        {
            m_backtoUsername.Enabled = false;
            m_controls.Enabled = false;
            m_lobby1Button.Enabled = false;
            m_lobby2Button.Enabled = false;
            m_lobby3Button.Enabled = false;
        }
        else
        {
            m_backtoUsername.Enabled = true;
            m_controls.Enabled = true;
            m_lobby1Button.Enabled = true;
            m_lobby2Button.Enabled = true;
            m_lobby3Button.Enabled = true;
        }

        if (NetworkManager.m_Instance != null)
        {
            // Based on the lobby size, change the text of the button
            switch (NetworkManager.m_Instance.m_lobby1Size)
            {
                case 2:
                    m_lobby1Button.Text = "Lobby 1 Full";
                    m_lobby1Button.TextColor = Color.DarkRed;
                    break;

                case < 2:
                    m_lobby1Button.Text = "Join Lobby 1";
                    m_lobby1Button.TextColor = Color.Green;
                    break;
            }

            switch (NetworkManager.m_Instance.m_lobby2Size)
            {
                case 2:
                    m_lobby2Button.Text = "Lobby 2 Full";
                    m_lobby2Button.TextColor = Color.DarkRed;
                    break;

                case < 2:
                    m_lobby2Button.Text = "Join Lobby 2";
                    m_lobby2Button.TextColor = Color.Green;
                    break;
            }

            switch (NetworkManager.m_Instance.m_lobby3Size)
            {
                case 2:
                    m_lobby3Button.Text = "Lobby 3 Full";
                    m_lobby3Button.TextColor = Color.DarkRed;
                    break;

                case < 2:
                    m_lobby3Button.Text = "Join Lobby 3";
                    m_lobby3Button.TextColor = Color.Green;
                    break;
            }
        }

        base.Update(deltaTime);
    }

    public override void Draw(float deltaTime)
    {
        // Draw the lobby select text
        m_Manager.m_Game.m_SpriteBatch.DrawString(m_Manager.m_Game.m_Spritefont, "Lobby Select:",
            new Vector2(-490, -270), Color.White);

        if (NetworkManager.m_Instance != null)
        {
            // Based on the lobby size, change the text
            switch (NetworkManager.m_Instance.m_lobby1Size)
            {
                case 0:
                    m_Manager.m_Game.m_SpriteBatch.DrawString(m_Manager.m_Game.m_Spritefont,
                        "Lobby 1 Size: " + NetworkManager.m_Instance.m_lobby1Size, new Vector2(-490, -240),
                        Color.Green);

                    break;

                case 1:
                    m_Manager.m_Game.m_SpriteBatch.DrawString(m_Manager.m_Game.m_Spritefont,
                        "Lobby 1 Size: " + NetworkManager.m_Instance.m_lobby1Size, new Vector2(-490, -240),
                        Color.Orange);

                    break;

                case 2:

                    m_Manager.m_Game.m_SpriteBatch.DrawString(m_Manager.m_Game.m_Spritefont,
                        "Lobby 1 Size: " + NetworkManager.m_Instance.m_lobby1Size, new Vector2(-490, -240),
                        Color.DarkRed);

                    break;
            }

            switch (NetworkManager.m_Instance.m_lobby2Size)
            {
                case 0:
                    m_Manager.m_Game.m_SpriteBatch.DrawString(m_Manager.m_Game.m_Spritefont,
                        "Lobby 2 Size: " + NetworkManager.m_Instance.m_lobby2Size, new Vector2(-490, -210),
                        Color.Green);

                    break;

                case 1:
                    m_Manager.m_Game.m_SpriteBatch.DrawString(m_Manager.m_Game.m_Spritefont,
                        "Lobby 2 Size: " + NetworkManager.m_Instance.m_lobby2Size, new Vector2(-490, -210),
                        Color.Orange);

                    break;

                case 2:

                    m_Manager.m_Game.m_SpriteBatch.DrawString(m_Manager.m_Game.m_Spritefont,
                        "Lobby 2 Size: " + NetworkManager.m_Instance.m_lobby2Size, new Vector2(-490, -210),
                        Color.DarkRed);

                    break;
            }

            switch (NetworkManager.m_Instance.m_lobby3Size)
            {
                case 0:
                    m_Manager.m_Game.m_SpriteBatch.DrawString(m_Manager.m_Game.m_Spritefont,
                        "Lobby 3 Size: " + NetworkManager.m_Instance.m_lobby3Size, new Vector2(-490, -180),
                        Color.Green);

                    break;

                case 1:
                    m_Manager.m_Game.m_SpriteBatch.DrawString(m_Manager.m_Game.m_Spritefont,
                        "Lobby 3 Size: " + NetworkManager.m_Instance.m_lobby3Size, new Vector2(-490, -180),
                        Color.Orange);

                    break;

                case 2:

                    m_Manager.m_Game.m_SpriteBatch.DrawString(m_Manager.m_Game.m_Spritefont,
                        "Lobby 3 Size: " + NetworkManager.m_Instance.m_lobby3Size, new Vector2(-490, -180),
                        Color.DarkRed);

                    break;
            }
        }

        base.Draw(deltaTime);
        m_desktop.Render();

        #endregion
    }
}