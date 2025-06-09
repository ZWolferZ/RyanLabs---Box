#region Includes

// Includes
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
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

internal class UserNameScene : Scene
{
    #region Member Varibles

    // Keyboard keys
    private static readonly Keys[] m_keys =
    {
        Keys.A, Keys.B, Keys.C, Keys.D, Keys.E, Keys.F, Keys.G, Keys.H, Keys.I, Keys.J, Keys.K, Keys.L, Keys.M, Keys.N,
        Keys.O, Keys.P, Keys.Q, Keys.R, Keys.S, Keys.T, Keys.U, Keys.V, Keys.W, Keys.X, Keys.Y, Keys.Z,
        Keys.D0, Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9,
        Keys.OemPeriod, Keys.OemComma, Keys.OemQuestion, Keys.OemSemicolon, Keys.OemOpenBrackets,
        Keys.OemCloseBrackets, Keys.OemPlus, Keys.OemMinus
    };

    // Characters for the keys
    private static readonly char[] m_keyChars =
    {
        'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V',
        'W', 'X', 'Y', 'Z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.', ',', '?', ';', '[', ']',
        '+', '-'
    };

    // Key states
    private readonly bool[] m_keyStates = new bool[m_keys.Length];

    private readonly int m_maxLength = 12;
    private TextButton ConfirmButton = new();
    private bool m_backspaceState;
    private Desktop m_desktop;
    private bool m_spaceState;
    private string m_tempName = string.Empty;

    #endregion

    #region Constructor

    public UserNameScene(SceneManager manager) : base(manager)
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
        return "UsernameScene";
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

        m_desktop = new Desktop
        {
            Root = grid
        };

        // Create the confirm button
        ConfirmButton = new TextButton
        {
            Text = "Confirm Username",
            GridRow = 4,
            GridColumn = 2,
            GridColumnSpan = 1,
            PressedBackground = new SolidBrush(Color.DarkRed),
            Border = new SolidBrush(Color.Black),
            BorderThickness = Thickness.FromString("5"),
            TextColor = Color.Green,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Width = Graphics.Viewport.Width / cols * ConfirmButton.GridColumnSpan,
            Height = Graphics.Viewport.Height / rows * ConfirmButton.GridRowSpan,
            Enabled = true
        };
        grid.Widgets.Add(ConfirmButton);

        ConfirmButton.Click += (s, a) =>
        {
            // Set the username
            if (NetworkManager.m_Instance != null) NetworkManager.m_Instance.m_username = m_tempName;

            // Load the lobby select scene
            m_Manager.LoadScene(new LobbySelect(m_Manager));
        };

        var childPanel = new Panel
        {
            GridColumn = 0,
            GridRow = 0
        };

        grid.Widgets.Add(childPanel);
    }

    #endregion

    #region Update Methods

    public override void Update(float deltaTime)
    {
        if (m_Manager.m_Game.IsActive)
            // How many string functions are there in C++ wtf (What the frick)
            ConfirmButton.Enabled = !string.IsNullOrWhiteSpace(m_tempName);
        else
            ConfirmButton.Enabled = false;

        // Update the username
        for (var i = 0; i < m_keys.Length; i++)
        {
            // Break if the username is at the max length
            if (m_tempName != null && m_tempName.Length >= m_maxLength) break;

            // Check if the key is pressed
            var newState = Keyboard.GetState().IsKeyDown(m_keys[i]);
            if (newState != m_keyStates[i] && newState)
            {
                var key = m_keyChars[i];

                // Check if the shift key is pressed or caps lock is on
                if (Keyboard.GetState().CapsLock ^ (Keyboard.GetState().IsKeyDown(Keys.LeftShift) ||
                                                    Keyboard.GetState().IsKeyDown(Keys.RightShift)))
                    key = char.ToUpper(key);
                else
                    key = char.ToLower(key);

                // Add the key to the username
                if (m_tempName != null && m_tempName.Length < m_maxLength) m_tempName += key;
            }

            // Update the key state
            m_keyStates[i] = newState;
        }

        // Check for backspace and space
        if (Keyboard.GetState().IsKeyDown(Keys.Back) && !m_backspaceState && m_tempName != string.Empty)
        {
            m_backspaceState = true;

            if (m_tempName != null) m_tempName = m_tempName.Substring(0, m_tempName.Length - 1);
        }

        if (Keyboard.GetState().IsKeyUp(Keys.Back)) m_backspaceState = false;

        if (Keyboard.GetState().IsKeyDown(Keys.Space) && !m_spaceState)
        {
            m_spaceState = true;
            m_tempName += " ";
        }

        if (Keyboard.GetState().IsKeyUp(Keys.Space)) m_spaceState = false;
    }

    public override void Draw(float deltaTime)
    {
        // Draw the username to the screen
        m_Manager.m_Game.m_SpriteBatch.DrawString(m_Manager.m_Game.m_userNameFont, "Username: " + m_tempName,
            new Vector2(-475, -75),
            Color.White);

        // Draw the enter username image
        m_Manager.m_Game.m_SpriteBatch.Draw(m_Manager.m_Game.m_enterUsername, new Vector2(-200, -270),
            Color.White);

        base.Draw(deltaTime);
        m_desktop.Render();
    }

    #endregion
}