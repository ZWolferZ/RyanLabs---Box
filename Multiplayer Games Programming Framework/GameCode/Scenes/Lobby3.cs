// SEE LOBBY 1 FOR COMMENTS ON THIS FILE
// SEE LOBBY 1 FOR COMMENTS ON THIS FILE
// SEE LOBBY 1 FOR COMMENTS ON THIS FILE

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

internal class Lobby3 : Scene
{
    private readonly TextButton m_backButton = new();
    private readonly TextButton m_changeColour = new();
    private readonly TextButton m_readyButton = new();
    private int m_colourSwitch;
    private Desktop m_desktop;
    private bool m_ready;

    public Lobby3(SceneManager manager) : base(manager)
    {
        manager.m_Game.IsMouseVisible = true;
        if (NetworkManager.m_Instance != null)
            NetworkManager.m_Instance.TcpSendPacket(new inLobby(NetworkManager.m_Instance.m_clientID, 3), true);
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
        return "Lobby 3";
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

        m_readyButton.Text = "Ready";
        m_readyButton.TextColor = Color.Green;
        m_readyButton.GridRow = 3;
        m_readyButton.GridColumn = 1;
        m_readyButton.GridColumnSpan = 1;
        m_readyButton.Border = new SolidBrush(Color.Black);
        m_readyButton.BorderThickness = Thickness.FromString("5");
        m_readyButton.PressedBackground = new SolidBrush(Color.DarkRed);
        m_readyButton.HorizontalAlignment = HorizontalAlignment.Center;
        m_readyButton.VerticalAlignment = VerticalAlignment.Center;

        m_readyButton.Width = Graphics.Viewport.Width / cols * m_readyButton.GridColumnSpan;
        m_readyButton.Height = Graphics.Viewport.Height / rows * m_readyButton.GridRowSpan;
        grid.Widgets.Add(m_readyButton);

        m_readyButton.Click += (s, a) =>
        {
            if (m_ready == false)
            {
                //Send lobby one ready packet
                m_readyButton.Text = "Unready";
                m_readyButton.TextColor = Color.Red;
                m_ready = true;
            }
            else
            {
                //Send lobby one unready packet
                m_readyButton.Text = "Ready";
                m_readyButton.TextColor = Color.Green;
                m_ready = false;
            }

            if (NetworkManager.m_Instance != null)
                NetworkManager.m_Instance.TcpSendPacket(new startLobby(m_ready, 3), true);
        };

        m_backButton.Text = "Back";
        m_backButton.TextColor = Color.White;
        m_backButton.GridRow = 3;
        m_backButton.GridColumn = 2;
        m_backButton.GridColumnSpan = 1;
        m_backButton.Border = new SolidBrush(Color.Black);
        m_backButton.BorderThickness = Thickness.FromString("5");
        m_backButton.PressedBackground = new SolidBrush(Color.DarkRed);
        m_backButton.HorizontalAlignment = HorizontalAlignment.Center;
        m_backButton.VerticalAlignment = VerticalAlignment.Center;
        m_backButton.Width = Graphics.Viewport.Width / cols * m_backButton.GridColumnSpan;
        m_backButton.Height = Graphics.Viewport.Height / rows * m_backButton.GridRowSpan;
        grid.Widgets.Add(m_backButton);

        m_backButton.Click += (s, a) =>
        {
            // Remove Client from Lobby packet

            if (NetworkManager.m_Instance != null)
            {
                NetworkManager.m_Instance.m_lobby3Size--;
                NetworkManager.m_Instance.TcpSendPacket(new LobbySize(NetworkManager.m_Instance.m_lobby3Size, 3), true);
                NetworkManager.m_Instance.m_inLobby3 = false;
                NetworkManager.m_Instance.TcpSendPacket(new inLobby(NetworkManager.m_Instance.m_clientID, 0), true);
            }

            m_Manager.LoadScene(new LobbySelect(m_Manager));
        };

        m_changeColour.Text = "Change Colour";
        m_changeColour.TextColor = Color.White;
        m_changeColour.GridRow = 2;
        m_changeColour.GridColumn = 1;
        m_changeColour.GridColumnSpan = 1;
        m_changeColour.Border = new SolidBrush(Color.Black);
        m_changeColour.BorderThickness = Thickness.FromString("5");
        m_changeColour.PressedBackground = new SolidBrush(Color.DarkRed);
        m_changeColour.HorizontalAlignment = HorizontalAlignment.Center;
        m_changeColour.VerticalAlignment = VerticalAlignment.Center;

        m_changeColour.Width = Graphics.Viewport.Width / cols * m_changeColour.GridColumnSpan;
        m_changeColour.Height = Graphics.Viewport.Height / rows * m_changeColour.GridRowSpan;
        grid.Widgets.Add(m_changeColour);

        m_changeColour.Click += (s, a) =>
        {
            m_colourSwitch++;

            switch (m_colourSwitch)
            {
                case 0:
                    if (NetworkManager.m_Instance != null)
                        NetworkManager.m_Instance.m_playerColour = System.Drawing.Color.Yellow;
                    m_changeColour.TextColor = Color.Yellow;
                    break;

                case 1:
                    if (NetworkManager.m_Instance != null)
                        NetworkManager.m_Instance.m_playerColour = System.Drawing.Color.Blue;
                    m_changeColour.TextColor = Color.Blue;

                    break;

                case 2:
                    if (NetworkManager.m_Instance != null)
                        NetworkManager.m_Instance.m_playerColour = System.Drawing.Color.Green;
                    m_changeColour.TextColor = Color.Green;

                    break;

                case 3:
                    if (NetworkManager.m_Instance != null)
                        NetworkManager.m_Instance.m_playerColour = System.Drawing.Color.BurlyWood;
                    m_changeColour.TextColor = Color.BurlyWood;

                    break;

                case 4:
                    if (NetworkManager.m_Instance != null)
                        NetworkManager.m_Instance.m_playerColour = System.Drawing.Color.Aqua;
                    m_changeColour.TextColor = Color.Aqua;

                    break;

                case 5:
                    if (NetworkManager.m_Instance != null)
                        NetworkManager.m_Instance.m_playerColour = System.Drawing.Color.Chocolate;
                    m_changeColour.TextColor = Color.Chocolate;

                    break;

                case 6:
                    if (NetworkManager.m_Instance != null)
                        NetworkManager.m_Instance.m_playerColour = System.Drawing.Color.Pink;
                    m_changeColour.TextColor = Color.Pink;

                    break;

                case 7:
                    if (NetworkManager.m_Instance != null)
                        NetworkManager.m_Instance.m_playerColour = System.Drawing.Color.DarkGray;
                    m_changeColour.TextColor = Color.DarkGray;

                    break;

                case 8:
                    if (NetworkManager.m_Instance != null)
                        NetworkManager.m_Instance.m_playerColour = System.Drawing.Color.Black;
                    m_changeColour.TextColor = Color.Black;
                    break;

                case 9:
                    if (NetworkManager.m_Instance != null)
                        NetworkManager.m_Instance.m_playerColour = System.Drawing.Color.White;
                    m_changeColour.TextColor = Color.White;
                    break;

                case 10:
                    m_colourSwitch = 0;
                    break;
            }
        };
    }

    public override void Update(float deltaTime)
    {
        if (!m_Manager.m_Game.IsActive)
        {
            m_backButton.Enabled = false;
            m_readyButton.Enabled = false;
            m_changeColour.Enabled = false;
        }
        else
        {
            m_backButton.Enabled = !m_ready;
            m_readyButton.Enabled = true;
            m_changeColour.Enabled = true;
        }

        if (NetworkManager.m_Instance != null && !NetworkManager.m_Instance.m_startLobby3) return;

        m_Manager.LoadScene(new GameScene(m_Manager));
    }

    public override void Draw(float deltaTime)
    {
        m_Manager.m_Game.m_SpriteBatch.DrawString(m_Manager.m_Game.m_Spritefont, "Lobby 3", new Vector2(-490, -270),
            Color.White);

        if (NetworkManager.m_Instance != null)
            switch (NetworkManager.m_Instance.m_lobby3Size)
            {
                case 1:
                    m_Manager.m_Game.m_SpriteBatch.DrawString(m_Manager.m_Game.m_Spritefont, "Player One Joined",
                        new Vector2(-490, -200), Color.White);
                    break;

                case 2:
                    m_Manager.m_Game.m_SpriteBatch.DrawString(m_Manager.m_Game.m_Spritefont, "Player One Joined",
                        new Vector2(-490, -200), Color.White);
                    m_Manager.m_Game.m_SpriteBatch.DrawString(m_Manager.m_Game.m_Spritefont, "Player Two Joined",
                        new Vector2(235, -200), Color.White);
                    break;
            }

        base.Draw(deltaTime);
        m_desktop.Render();
    }
}