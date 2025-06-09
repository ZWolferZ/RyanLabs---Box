#region Includes

// Includes
using Microsoft.Xna.Framework;
using Multiplayer_Games_Programming_Framework.Core.Scene;
using Multiplayer_Games_Programming_Framework.Core.Utilities;
using Myra;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;
using nkast.Aether.Physics2D.Dynamics;
using System;
using System.Diagnostics;
using System.IO;

namespace Multiplayer_Games_Programming_Framework.GameCode.Scenes;

#endregion

internal class ControlsScene : Scene
{
    #region Member Variables

    // Member variables
    private TextButton m_backButton = new();

    private TextButton m_controlsButton = new();
    private Desktop m_Desktop;

    #endregion

    #region Constructor

    // Constructor
    public ControlsScene(SceneManager manager) : base(manager)
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
        return "ControlsScene";
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

        const int cols = 3;
        for (var i = 0; i < cols; ++i) grid.ColumnsProportions.Add(new Proportion(ProportionType.Part));

        const int rows = 4;
        for (var i = 0; i < rows; ++i) grid.RowsProportions.Add(new Proportion(ProportionType.Part));

        m_Desktop = new Desktop
        {
            Root = grid
        };

        // Back button to return to the lobby select
        m_backButton = new TextButton
        {
            Text = "Back to Lobby Select",
            GridRow = 4,
            GridColumn = 2,
            GridColumnSpan = 1,
            PressedBackground = new SolidBrush(Color.DarkRed),
            Border = new SolidBrush(Color.Black),
            BorderThickness = Thickness.FromString("5"),
            TextColor = Color.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Width = Graphics.Viewport.Width / cols * m_backButton.GridColumnSpan,
            Height = Graphics.Viewport.Height / rows * m_backButton.GridRowSpan,
            Enabled = true
        };
        grid.Widgets.Add(m_backButton);

        // Event handler for the back button
        m_backButton.Click += (s, a) => { m_Manager.LoadScene(new LobbySelect(m_Manager)); };

        // Button to open the controls.txt file
        m_controlsButton = new TextButton
        {
            Text = "Open Controls.txt",
            GridRow = 4,
            GridColumn = 1,
            GridColumnSpan = 1,
            PressedBackground = new SolidBrush(Color.DarkRed),
            Border = new SolidBrush(Color.Black),
            BorderThickness = Thickness.FromString("5"),
            TextColor = Color.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Width = Graphics.Viewport.Width / cols * m_backButton.GridColumnSpan,
            Height = Graphics.Viewport.Height / rows * m_backButton.GridRowSpan,
            Enabled = true
        };
        grid.Widgets.Add(m_controlsButton);

        m_controlsButton.Click += (s, a) =>
        {
            try
            {
                // Open the controls.txt file
                var startInfo = new ProcessStartInfo
                {
                    FileName = "notepad.exe",
                    Arguments = "Controls.txt",
                    WorkingDirectory = Path.GetFullPath(@"..\..")
                };
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                // Mac/Linux user spotted
                Debug.WriteLine(ex.Message);
            }
        };

        var childPanel = new Panel
        {
            GridColumn = 0,
            GridRow = 0
        };

        grid.Widgets.Add(childPanel);
    }

    #endregion

    #region Scene Methods

    public override void Update(float deltaTime)
    {
        // Only enable the buttons if the game is active
        if (!m_Manager.m_Game.IsActive)
        {
            m_backButton.Enabled = false;
            m_controlsButton.Enabled = false;
        }
        else
        {
            m_backButton.Enabled = true;
            m_controlsButton.Enabled = true;
        }
    }

    // Draw the controls scene UI and Instructions
    public override void Draw(float deltaTime)
    {
        m_Manager.m_Game.GraphicsDevice.Clear(Color.DarkRed);

        m_Manager.m_Game.m_SpriteBatch.DrawString(m_Manager.m_Game.m_userNameFont, "Dodge Moving Obstacles to WIN",
            new Vector2(-475, -200),
            Color.Green);

        m_Manager.m_Game.m_SpriteBatch.DrawString(m_Manager.m_Game.m_userNameFont,
            "Left/Right Arrow Keys\n- Move around", new Vector2(-475, -100),
            Color.Yellow);

        m_Manager.m_Game.m_SpriteBatch.DrawString(m_Manager.m_Game.m_userNameFont, "Numbers 1-6 - Emote",
            new Vector2(-475, 100), Color.Yellow);

        base.Draw(deltaTime);
        m_Desktop.Render();
    }

    #endregion
}