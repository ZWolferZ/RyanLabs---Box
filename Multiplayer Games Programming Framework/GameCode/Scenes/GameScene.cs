#region Includes

// Includes
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Multiplayer_Games_Programming_Framework.Core;
using Multiplayer_Games_Programming_Framework.Core.Components;
using Multiplayer_Games_Programming_Framework.Core.Scene;
using Multiplayer_Games_Programming_Framework.Core.Utilities;
using Multiplayer_Games_Programming_Framework.GameCode.Components;
using Multiplayer_Games_Programming_Framework.GameCode.Prefabs;
using Multiplayer_Games_Programming_Packet_Library;
using nkast.Aether.Physics2D.Dynamics;
using System;
using System.Threading;

namespace Multiplayer_Games_Programming_Framework.GameCode.Scenes;

#endregion

internal class GameScene : Scene
{
    #region Member Varibles

    // Game Variables
    public static float m_endTime = -5;

    public static float m_drawTime = m_endTime + 5;
    private readonly bool[] m_emote = new bool[6];
    private bool m_Draw;
    private bool m_gameEnding;
    private bool m_Lose;
    private GameModeState m_GameModeState;
    private float m_GameTimer = 30;
    private float m_opponentUsernameWidth;
    private float m_usernameWidth;
    private bool m_Win;

    // Game Objects
    private LeftFallingObstacle m_fallingLeftWall1;

    private LeftFallingObstacle m_fallingLeftWall2;
    private LeftFallingObstacle m_fallingLeftWall3;
    private RightFallingObstacle m_fallingRightWall1;
    private RightFallingObstacle m_fallingRightWall2;
    private RightFallingObstacle m_fallingRightWall3;
    private GameObject m_centerWall;
    private PlayerGO m_Player;
    private PlayerGO m_RemotePlayer;

    // Falling Wall Variables
    private readonly Vector2 m_fallingLeftWallPos1 = new(-350, -200);

    private readonly Vector2 m_fallingLeftWallPos2 = new(-125, -400);
    private readonly Vector2 m_fallingLeftWallPos3 = new(-250, -700);
    private readonly Vector2 m_fallingLeftWallScale1 = new(50, 3);
    private readonly Vector2 m_fallingLeftWallScale2 = new(25, 3);
    private readonly Vector2 m_fallingLeftWallScale3 = new(25, 3);
    private readonly Vector2 m_fallingRightWallPos1 = new(350, -300);
    private readonly Vector2 m_fallingRightWallPos2 = new(125, -600);
    private readonly Vector2 m_fallingRightWallPos3 = new(250, -900);
    private readonly Vector2 m_fallingRightWallScale1 = new(50, 3);
    private readonly Vector2 m_fallingRightWallScale2 = new(25, 3);
    private readonly Vector2 m_fallingRightWallScale3 = new(25, 3);
    private readonly float m_fallingSpeed = 175.0f;
    private readonly float m_ResetBoundY = 250.0f;

    // UI Variables
    private readonly Vector2 m_UILeft = new(-450, -240);

    private readonly Vector2 m_UIRight = new(50, -240);

    // Emote Thread
    private Thread m_emoteThread;

    #endregion

    #region Constructor Methods

    public GameScene(SceneManager manager) : base(manager)
    {
        m_GameModeState = GameModeState.AWAKE;
    }

    public override void LoadContent()
    {
        base.LoadContent();

        // Send appropriate queries to the server to get the opponent's ID, colour and the opponent's username
        if (NetworkManager.m_Instance != null && NetworkManager.m_Instance.m_inLobby1)
        {
            NetworkManager.m_Instance.TcpSendPacket(new ClientID(NetworkManager.m_Instance.m_clientID, 1), false);
            NetworkManager.m_Instance.TcpSendPacket(new Colour(NetworkManager.m_Instance.m_clientID,
                NetworkManager.m_Instance.m_playerColour.R, NetworkManager.m_Instance.m_playerColour.G,
                NetworkManager.m_Instance.m_playerColour.B, NetworkManager.m_Instance.m_playerColour.A, 1), true);

            NetworkManager.m_Instance.TcpSendPacket(new Username(NetworkManager.m_Instance.m_username, 1), true);
        }

        if (NetworkManager.m_Instance != null && NetworkManager.m_Instance.m_inLobby2)
        {
            NetworkManager.m_Instance.TcpSendPacket(new ClientID(NetworkManager.m_Instance.m_clientID, 2), false);
            NetworkManager.m_Instance.TcpSendPacket(new Colour(NetworkManager.m_Instance.m_clientID,
                NetworkManager.m_Instance.m_playerColour.R, NetworkManager.m_Instance.m_playerColour.G,
                NetworkManager.m_Instance.m_playerColour.B, NetworkManager.m_Instance.m_playerColour.A, 2), true);

            NetworkManager.m_Instance.TcpSendPacket(new Username(NetworkManager.m_Instance.m_username, 2), true);
        }

        if (NetworkManager.m_Instance != null && NetworkManager.m_Instance.m_inLobby3)
        {
            NetworkManager.m_Instance.TcpSendPacket(new ClientID(NetworkManager.m_Instance.m_clientID, 3), false);
            NetworkManager.m_Instance.TcpSendPacket(new Colour(NetworkManager.m_Instance.m_clientID,
                NetworkManager.m_Instance.m_playerColour.R, NetworkManager.m_Instance.m_playerColour.G,
                NetworkManager.m_Instance.m_playerColour.B, NetworkManager.m_Instance.m_playerColour.A, 3), true);

            NetworkManager.m_Instance.TcpSendPacket(new Username(NetworkManager.m_Instance.m_username, 3), true);
        }

        // Wait for the server to send the opponent's ID and colour, before creating the players
        while (true)
            // Simple Event Listener
            if (NetworkManager.m_Instance != null && NetworkManager.m_Instance.m_oppID != -1 && NetworkManager.m_Instance.m_oppColourInfoConfirmed &&
                NetworkManager.m_Instance.m_oppUsernameInfoConfirmed)
                break;

        // Get the width of the username and the opponent's username to center them
        m_usernameWidth = m_Manager.m_Game.m_Spritefont.MeasureString(NetworkManager.m_Instance.m_username).X;
        m_opponentUsernameWidth =
            m_Manager.m_Game.m_Spritefont.MeasureString(NetworkManager.m_Instance.m_oppUsername).X;

        // Create the players depending on the client ID
        if (NetworkManager.m_Instance.m_clientID < NetworkManager.m_Instance.m_oppID)
        {
            m_Player = GameObject.Instantiate<PlayerGO>(this,
                new Transform(new Vector2(-445f, 240), new Vector2(7, 7f), 0));
            m_Player.AddComponent(new Movement(m_Player, 100));

            m_Player.m_SpriteRenderer.m_Color = new Color(NetworkManager.m_Instance.m_playerColour.R,
                NetworkManager.m_Instance.m_playerColour.G, NetworkManager.m_Instance.m_playerColour.B,
                NetworkManager.m_Instance.m_playerColour.A);

            m_RemotePlayer = GameObject.Instantiate<PlayerGO>(this,
                new Transform(new Vector2(445f, 240), new Vector2(7, 7), 0));
            m_RemotePlayer.AddComponent(new NetworkMovement(m_RemotePlayer, NetworkManager.m_Instance.m_oppID,
                new Vector2(445f, 240)));

            m_RemotePlayer.m_SpriteRenderer.m_Color = new Color(NetworkManager.m_Instance.m_oppColour.R,
                NetworkManager.m_Instance.m_oppColour.G, NetworkManager.m_Instance.m_oppColour.B,
                NetworkManager.m_Instance.m_oppColour.A);
        }
        else
        {
            m_RemotePlayer = GameObject.Instantiate<PlayerGO>(this,
                new Transform(new Vector2(-445f, 240), new Vector2(7, 7), 0));
            m_RemotePlayer.AddComponent(new NetworkMovement(m_RemotePlayer, NetworkManager.m_Instance.m_oppID,
                new Vector2(-445f, 240)));

            m_RemotePlayer.m_SpriteRenderer.m_Color = new Color(NetworkManager.m_Instance.m_oppColour.R,
                NetworkManager.m_Instance.m_oppColour.G, NetworkManager.m_Instance.m_oppColour.B,
                NetworkManager.m_Instance.m_oppColour.A);

            m_Player = GameObject.Instantiate<PlayerGO>(this,
                new Transform(new Vector2(445f, 240), new Vector2(7, 7f), 0));

            m_Player.AddComponent(new Movement(m_Player, 100));

            m_Player.m_SpriteRenderer.m_Color = new Color(NetworkManager.m_Instance.m_playerColour.R,
                NetworkManager.m_Instance.m_playerColour.G, NetworkManager.m_Instance.m_playerColour.B,
                NetworkManager.m_Instance.m_playerColour.A);
        }

        // Border
        Vector2[] wallPos =
        {
            new(0, -276), // top
            new(495, 0), // right
            new(0, 276), // bottom
            new(-495, 0) // left
        };

        Vector2[] wallScales =
        {
            new(100, 1), // top
            new(1, 56.25f), // right
            new(100, 1), // bottom
            new(1, 56.25f) // left
        };

        // Create the walls
        for (var i = 0; i < 4; i++)
        {
            var go = GameObject.Instantiate<GameObject>(this, new Transform(wallPos[i], wallScales[i], 0));
            var sr = go.AddComponent(new SpriteRenderer(go, "Square(10x10)"));
            var rb = go.AddComponent(new Rigidbody(go, BodyType.Static, 10, sr.m_Size / 2));
            rb.CreateRectangle(sr.m_Size.X, sr.m_Size.Y, 0.0f, 1.0f, Vector2.Zero, Physics.GetCategoryByName("Wall"),
                Physics.GetCategoryByName("All"));
            sr.m_Color = Color.DarkRed;
            m_GameObjects.Add(go);
        }

        Vector2 centerWallPos = new(0, 0);
        Vector2 centerWallScale = new(1, 56.25f);

        m_centerWall = GameObject.Instantiate<GameObject>(this, new Transform(centerWallPos, centerWallScale, 0));
        var centerSr = m_centerWall.AddComponent(new SpriteRenderer(m_centerWall, "Square(10x10)"));
        var centerRb = m_centerWall.AddComponent(new Rigidbody(m_centerWall, BodyType.Static, 10, centerSr.m_Size / 2));
        centerRb.CreateRectangle(centerSr.m_Size.X, centerSr.m_Size.Y, 0.0f, 1.0f, Vector2.Zero,
            Physics.GetCategoryByName("Wall"), Physics.GetCategoryByName("All"));
        centerSr.m_Color = Color.DarkRed;

        m_GameObjects.Add(m_centerWall);

        var horizontalWallPos = new Vector2(0, -125);
        var horizontalWallScale = new Vector2(100, 1);

        var horizontalWall =
            GameObject.Instantiate<GameObject>(this, new Transform(horizontalWallPos, horizontalWallScale, 0));
        var horizontalSr = horizontalWall.AddComponent(new SpriteRenderer(horizontalWall, "Square(10x10)"));
        var horizontalRb =
            horizontalWall.AddComponent(new Rigidbody(horizontalWall, BodyType.Static, 10, horizontalSr.m_Size / 2));
        horizontalRb.CreateRectangle(horizontalSr.m_Size.X, horizontalSr.m_Size.Y, 0.0f, 1.0f, Vector2.Zero,
            Physics.GetCategoryByName("Wall"), Physics.GetCategoryByName("All"));
        horizontalSr.m_Color = Color.DarkRed;
        m_GameObjects.Add(horizontalWall);

        // Falling Walls
        m_fallingRightWall1 =
            GameObject.Instantiate<RightFallingObstacle>(this,
                new Transform(m_fallingRightWallPos1, m_fallingRightWallScale1, 0));

        m_GameObjects.Add(m_fallingRightWall1);

        m_fallingRightWall2 =
            GameObject.Instantiate<RightFallingObstacle>(this,
                new Transform(m_fallingRightWallPos2, m_fallingRightWallScale2, 0));

        m_GameObjects.Add(m_fallingRightWall2);

        m_fallingRightWall3 =
            GameObject.Instantiate<RightFallingObstacle>(this,
                new Transform(m_fallingRightWallPos3, m_fallingRightWallScale3, 0));

        m_GameObjects.Add(m_fallingRightWall3);

        m_fallingLeftWall1 =
            GameObject.Instantiate<LeftFallingObstacle>(this,
                new Transform(m_fallingLeftWallPos1, m_fallingLeftWallScale1, 0));

        m_GameObjects.Add(m_fallingLeftWall1);

        m_fallingLeftWall2 = GameObject.Instantiate<LeftFallingObstacle>(this,
            new Transform(m_fallingLeftWallPos2, m_fallingLeftWallScale2, 0));

        m_GameObjects.Add(m_fallingLeftWall2);

        m_fallingLeftWall3 = GameObject.Instantiate<LeftFallingObstacle>(this,
            new Transform(m_fallingLeftWallPos3, m_fallingLeftWallScale3, 0));

        m_GameObjects.Add(m_fallingLeftWall3);

        // Quickly set the emote thread too false to prevent any weird behaviour
        NetworkManager.m_Instance.m_emoteThreadStop = false;

        // Start the emote thread
        m_emoteThread = new Thread(EmoteHandler);
        m_emoteThread.Start();
    }

    protected override string SceneName()
    {
        return "GameScene";
    }

    protected override World CreateWorld()
    {
        return new World(Physics.m_Gravity);
    }

    protected override Camera CreateCamera()
    {
        return new Camera(Vector2.Zero);
    }

    #endregion

    #region UI Drawing

    public override void Draw(float deltaTime)
    {
        // Get the current time and convert it to an integer
        var currentTime = (int)m_GameTimer;

        // Draw Client UI based on the client ID
        if (NetworkManager.m_Instance != null)
        {
            // Draw the usernames
            m_Manager.m_Game.m_SpriteBatch.DrawString(m_Manager.m_Game.m_Spritefont,
                NetworkManager.m_Instance.m_username,
                new
                    Vector2(m_Player.m_Transform.Position.X - m_usernameWidth / 2,
                        m_Player.m_Transform.Position.Y - 75),
                Color.White);

            m_Manager.m_Game.m_SpriteBatch.DrawString(m_Manager.m_Game.m_Spritefont,
                NetworkManager.m_Instance.m_oppUsername, new
                    Vector2(m_RemotePlayer.m_Transform.Position.X - m_opponentUsernameWidth / 2,
                        m_RemotePlayer.m_Transform.Position.Y - 75),
                Color.White);

            // Draw the timer
            if (NetworkManager.m_Instance.m_clientID < NetworkManager.m_Instance.m_oppID)
            {
                if (m_GameTimer > m_drawTime && (!m_Draw || !m_Win || !m_Lose))
                    m_Manager.m_Game.m_SpriteBatch.DrawString(m_Manager.m_Game.m_Spritefont, "Timer: " + currentTime,
                        new Vector2(m_UILeft.X, m_UILeft.Y - 35), Color.Yellow);

                // Draw the win, lose and draw messages
                if (m_Win)
                {
                    m_Manager.m_SpriteBatch.DrawString(m_Manager.m_Game.m_Spritefont, "YOU WIN!",
                        new Vector2(m_UILeft.X, m_UILeft.Y - 35),
                        Color.Yellow);
                    m_Player.m_SpriteRenderer.SetTexture(m_Player, "Happy Cube");
                    m_RemotePlayer.m_SpriteRenderer.SetTexture(m_RemotePlayer, "Sad Cube");
                }

                if (m_Lose)
                {
                    m_Manager.m_SpriteBatch.DrawString(m_Manager.m_Game.m_Spritefont, "YOU LOSE!",
                        new Vector2(m_UILeft.X, m_UILeft.Y - 35),
                        Color.Yellow);

                    m_RemotePlayer.m_SpriteRenderer.SetTexture(m_Player, "Happy Cube");
                    m_Player.m_SpriteRenderer.SetTexture(m_Player, "Sad Cube");
                }

                if (m_Draw && !m_Lose && !m_Win)

                {
                    m_Manager.m_SpriteBatch.DrawString(m_Manager.m_Game.m_Spritefont, "YOU DRAW!",
                        new Vector2(m_UILeft.X, m_UILeft.Y - 35),
                        Color.Yellow);
                    m_RemotePlayer.m_SpriteRenderer.SetTexture(m_RemotePlayer, "Sad Cube");
                    m_Player.m_SpriteRenderer.SetTexture(m_Player, "Sad Cube");
                }

                // Some UI stuff
                m_Manager.m_Game.m_SpriteBatch.Draw(m_Manager.m_Game.m_yourOpp, m_UIRight, Color.White);

                // Draw the score
                switch (NetworkManager.m_Instance.m_lobbyScore)
                {
                    case 0:
                        m_Manager.m_Game.m_SpriteBatch.Draw(m_Manager.m_Game.m_score0Texture, m_UILeft, Color.White);
                        break;

                    case 1:
                        m_Manager.m_Game.m_SpriteBatch.Draw(m_Manager.m_Game.m_score1Texture, m_UILeft, Color.White);
                        break;

                    case 2:
                        m_Manager.m_Game.m_SpriteBatch.Draw(m_Manager.m_Game.m_score2Texture, m_UILeft, Color.White);
                        break;

                    case 3:
                        m_Manager.m_Game.m_SpriteBatch.Draw(m_Manager.m_Game.m_score3Texture, m_UILeft, Color.White);
                        break;

                    case -1:
                        m_Manager.m_Game.m_SpriteBatch.Draw(m_Manager.m_Game.m_scoreNeg1Texture, m_UILeft, Color.White);
                        break;

                    case -2:
                        m_Manager.m_Game.m_SpriteBatch.Draw(m_Manager.m_Game.m_scoreNeg2Texture, m_UILeft, Color.White);
                        break;

                    case -3:
                        m_Manager.m_Game.m_SpriteBatch.Draw(m_Manager.m_Game.m_scoreNeg3Texture, m_UILeft, Color.White);
                        break;
                }
            }
            else
            {
                // Draw the timer
                if (m_GameTimer > m_drawTime && (!m_Draw || !m_Win || !m_Lose))
                    m_Manager.m_Game.m_SpriteBatch.DrawString(m_Manager.m_Game.m_Spritefont, "Timer: " + currentTime,
                        new Vector2(m_UIRight.X, m_UIRight.Y - 35), Color.Yellow);

                // Draw the win, lose and draw messages
                if (m_Win)
                {
                    m_Manager.m_SpriteBatch.DrawString(m_Manager.m_Game.m_Spritefont, "YOU WIN!",
                        new Vector2(m_UIRight.X, m_UIRight.Y - 35),
                        Color.Yellow);

                    m_Player.m_SpriteRenderer.SetTexture(m_Player, "Happy Cube");
                    m_RemotePlayer.m_SpriteRenderer.SetTexture(m_RemotePlayer, "Sad Cube");
                }

                if (m_Lose)
                {
                    m_Manager.m_SpriteBatch.DrawString(m_Manager.m_Game.m_Spritefont, "YOU LOSE!",
                        new Vector2(m_UIRight.X, m_UIRight.Y - 35),
                        Color.Yellow);

                    m_RemotePlayer.m_SpriteRenderer.SetTexture(m_Player, "Happy Cube");
                    m_Player.m_SpriteRenderer.SetTexture(m_RemotePlayer, "Sad Cube");
                }

                if (m_Draw && !m_Lose && !m_Win)
                {
                    m_Manager.m_SpriteBatch.DrawString(m_Manager.m_Game.m_Spritefont, "YOU DRAW!",
                        new Vector2(m_UIRight.X, m_UIRight.Y - 35),
                        Color.Yellow);
                    m_Player.m_SpriteRenderer.SetTexture(m_RemotePlayer, "Sad Cube");
                    m_RemotePlayer.m_SpriteRenderer.SetTexture(m_RemotePlayer, "Sad Cube");
                }

                // Some UI stuff
                m_Manager.m_Game.m_SpriteBatch.Draw(m_Manager.m_Game.m_yourOpp, m_UILeft, Color.White);

                // Draw the score
                switch (NetworkManager.m_Instance.m_lobbyScore)
                {
                    case 0:
                        m_Manager.m_Game.m_SpriteBatch.Draw(m_Manager.m_Game.m_score0Texture, m_UIRight, Color.White);
                        break;

                    case 1:
                        m_Manager.m_Game.m_SpriteBatch.Draw(m_Manager.m_Game.m_score1Texture, m_UIRight, Color.White);
                        break;

                    case 2:
                        m_Manager.m_Game.m_SpriteBatch.Draw(m_Manager.m_Game.m_score2Texture, m_UIRight, Color.White);
                        break;

                    case 3:
                        m_Manager.m_Game.m_SpriteBatch.Draw(m_Manager.m_Game.m_score3Texture, m_UIRight, Color.White);
                        break;

                    case -1:
                        m_Manager.m_Game.m_SpriteBatch.Draw(m_Manager.m_Game.m_scoreNeg1Texture, m_UIRight,
                            Color.White);
                        break;

                    case -2:
                        m_Manager.m_Game.m_SpriteBatch.Draw(m_Manager.m_Game.m_scoreNeg2Texture, m_UIRight,
                            Color.White);
                        break;

                    case -3:
                        m_Manager.m_Game.m_SpriteBatch.Draw(m_Manager.m_Game.m_scoreNeg3Texture, m_UIRight,
                            Color.White);
                        break;
                }
            }
        }

        base.Draw(deltaTime);
    }

    #endregion

    #region Update Methods

    private void AdjustScore(int scoreChange)
    {
        // -1 for player one
        // +1 for player two

        // If the game has ended, don't adjust the score
        if (m_Win || m_Lose || m_Draw) return;

        if (NetworkManager.m_Instance == null) return;
        switch (NetworkManager.m_Instance.m_lobbyScore) // If the score is 3 or -3, don't adjust the score
        {
            case >= 3:
            case <= -3:
                return;
        }

        // Adjust the score
        NetworkManager.m_Instance.m_lobbyScore += scoreChange;

        // Send a packet to the server to update the score
        if (NetworkManager.m_Instance.m_inLobby1)
        {
            NetworkManager.m_Instance.TcpSendPacket(new LobbyScore(NetworkManager.m_Instance.m_lobbyScore, 1),
                true);

            NetworkManager.m_Instance.TcpSendPacket(new ResetGameObjects(1), true);
        }
        else if (NetworkManager.m_Instance.m_inLobby2)
        {
            NetworkManager.m_Instance.TcpSendPacket(new LobbyScore(NetworkManager.m_Instance.m_lobbyScore, 2),
                true);

            NetworkManager.m_Instance.TcpSendPacket(new ResetGameObjects(2), true);
        }
        else if (NetworkManager.m_Instance.m_inLobby3)
        {
            NetworkManager.m_Instance.TcpSendPacket(new LobbyScore(NetworkManager.m_Instance.m_lobbyScore, 3),
                true);

            NetworkManager.m_Instance.TcpSendPacket(new ResetGameObjects(3), true);
        }
    }

    // Reset the lobby when the score is incremented
    // This is to reset the falling walls and the player positions
    // This is a broke method, but it works to a degree
    private void ResetLobby()
    {
        if (NetworkManager.m_Instance != null && !NetworkManager.m_Instance.m_resetLobby) return; // If the lobby is not reset, don't reset the lobby

        // Reset the falling walls
        UpdateWallPosition(m_fallingRightWall1, m_fallingRightWallPos1);
        UpdateWallPosition(m_fallingRightWall2, m_fallingRightWallPos2);
        UpdateWallPosition(m_fallingRightWall3, m_fallingRightWallPos3);

        UpdateWallPosition(m_fallingLeftWall1, m_fallingLeftWallPos1);
        UpdateWallPosition(m_fallingLeftWall2, m_fallingLeftWallPos2);
        UpdateWallPosition(m_fallingLeftWall3, m_fallingLeftWallPos3);

        // Reset the player positions
        if (NetworkManager.m_Instance != null && NetworkManager.m_Instance.m_clientID < NetworkManager.m_Instance.m_oppID)
        {
            m_Player.m_Rigidbody.UpdatePosition(new Vector2(-445, 240));
            m_RemotePlayer.m_Rigidbody.UpdatePosition(new Vector2(445, 240));
        }
        else
        {
            m_Player.m_Rigidbody.UpdatePosition(new Vector2(445, 240));
            m_RemotePlayer.m_Rigidbody.UpdatePosition(new Vector2(-445, 240));
        }

        if (NetworkManager.m_Instance != null) NetworkManager.m_Instance.m_resetLobby = false;
    }

    private void UpdateWallPosition(GameObject wall, Vector2 resetPosition, float deltaTime)
    {
        // Get the rigidbody and the position of the wall
        var rigidBody = wall.GetComponent<Rigidbody>();
        var position = rigidBody.m_Transform.Position;

        // Update the position of the wall
        rigidBody.UpdatePosition(position.Y <= m_ResetBoundY
        ? new Vector2(position.X, position.Y + m_fallingSpeed * deltaTime)
        : resetPosition);

        // Set the position of the wall
        wall.m_Transform = rigidBody.m_Transform;
    }

    // Update the position of the wall
    private void UpdateWallPosition(GameObject wall, Vector2 newPosition)
    {
        wall.GetComponent<Rigidbody>().UpdatePosition(newPosition);
        wall.m_Transform.Position = newPosition;
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        // If the server requests to reset the lobby, reset the lobby
        if (NetworkManager.m_Instance != null && NetworkManager.m_Instance.m_resetLobby) ResetLobby();

        //Update the camera
        var camPos = m_Camera.m_Position;
        var camRot = m_Camera.m_Rotation;
        var camZoom = m_Camera.m_ZoomFactor;

        m_Camera.SetPosition(camPos);
        m_Camera.SetRotation(camRot);
        m_Camera.SetZoom(camZoom);

        // Lock some variables to prevent weird behaviour
        m_Player.m_Transform.Rotation = 0.0f;
        m_Player.m_Transform.Position.Y = 240.0f;
        m_RemotePlayer.m_Transform.Rotation = 0.0f;
        m_RemotePlayer.m_Transform.Position.Y = 240.0f;

        // Take away from the game timer
        m_GameTimer -= deltaTime;

        // If the lobby is not reset, don't update the falling walls
        if (NetworkManager.m_Instance != null && !NetworkManager.m_Instance.m_resetLobby)
        {
            // Update the falling walls
            UpdateWallPosition(m_fallingRightWall1, m_fallingRightWallPos1, deltaTime);
            UpdateWallPosition(m_fallingRightWall2, m_fallingRightWallPos2, deltaTime);
            UpdateWallPosition(m_fallingRightWall3, m_fallingRightWallPos3, deltaTime);
            UpdateWallPosition(m_fallingLeftWall1, m_fallingLeftWallPos1, deltaTime);
            UpdateWallPosition(m_fallingLeftWall2, m_fallingLeftWallPos2, deltaTime);
            UpdateWallPosition(m_fallingLeftWall3, m_fallingLeftWallPos3, deltaTime);

            // Space the walls out so they don't overlap

            if (m_fallingLeftWall1.GetComponent<Rigidbody>().m_Transform.Position.Y - m_fallingLeftWall2.GetComponent<Rigidbody>().m_Transform.Position.Y <= 100)
            {
                UpdateWallPosition(m_fallingLeftWall1, new Vector2(m_fallingLeftWallPos1.X, m_fallingLeftWallPos1.Y - 50.0f), deltaTime);
            }

            if (m_fallingLeftWall2.GetComponent<Rigidbody>().m_Transform.Position.Y - m_fallingLeftWall3.GetComponent<Rigidbody>().m_Transform.Position.Y <= 100)
            {
                UpdateWallPosition(m_fallingLeftWall2, new Vector2(m_fallingLeftWallPos2.X, m_fallingLeftWallPos2.Y - 50.0f), deltaTime);
            }

            if (m_fallingLeftWall1.GetComponent<Rigidbody>().m_Transform.Position.Y - m_fallingLeftWall3.GetComponent<Rigidbody>().m_Transform.Position.Y <= 100)
            {
                UpdateWallPosition(m_fallingLeftWall3, new Vector2(m_fallingLeftWallPos3.X, m_fallingLeftWallPos3.Y - 50.0f), deltaTime);
            }

            if (m_fallingRightWall1.GetComponent<Rigidbody>().m_Transform.Position.Y - m_fallingRightWall2.GetComponent<Rigidbody>().m_Transform.Position.Y <= 100)
            {
                UpdateWallPosition(m_fallingRightWall1, new Vector2(m_fallingRightWallPos1.X, m_fallingRightWallPos1.Y - 50.0f), deltaTime);
            }

            if (m_fallingRightWall2.GetComponent<Rigidbody>().m_Transform.Position.Y - m_fallingRightWall3.GetComponent<Rigidbody>().m_Transform.Position.Y <= 100)
            {
                UpdateWallPosition(m_fallingRightWall2, new Vector2(m_fallingRightWallPos2.X, m_fallingRightWallPos2.Y - 50.0f), deltaTime);
            }

            if (m_fallingRightWall1.GetComponent<Rigidbody>().m_Transform.Position.Y - m_fallingRightWall3.GetComponent<Rigidbody>().m_Transform.Position.Y <= 100)
            {
                UpdateWallPosition(m_fallingRightWall3, new Vector2(m_fallingRightWallPos3.X, m_fallingRightWallPos3.Y - 50.0f), deltaTime);
            }
        }

        // If the walls are collided with, adjust the score
        if (NetworkManager.m_Instance != null && NetworkManager.m_Instance.m_clientID < NetworkManager.m_Instance.m_oppID)
        {
            if (m_fallingLeftWall1.GetComponent<CollisionComponent>().m_touched ||
                m_fallingLeftWall2.GetComponent<CollisionComponent>().m_touched ||
                m_fallingLeftWall3.GetComponent<CollisionComponent>().m_touched)
            {
                AdjustScore(1);
                m_fallingLeftWall1.GetComponent<CollisionComponent>().m_touched = false;
                m_fallingLeftWall2.GetComponent<CollisionComponent>().m_touched = false;
                m_fallingLeftWall3.GetComponent<CollisionComponent>().m_touched = false;
            }

            // If the score is 3 or -3, set the win or lose state
            switch (NetworkManager.m_Instance.m_lobbyScore)
            {
                case <= -3:
                    m_Win = true;
                    break;

                case >= 3:
                    m_Lose = true;
                    break;
            }
        }
        else
        {
            // If the walls are collided with, adjust the score
            if (m_fallingRightWall1.GetComponent<CollisionComponent>().m_touched ||
                m_fallingRightWall2.GetComponent<CollisionComponent>().m_touched ||
                m_fallingRightWall3.GetComponent<CollisionComponent>().m_touched)
            {
                AdjustScore(-1);

                m_fallingRightWall1.GetComponent<CollisionComponent>().m_touched = false;
                m_fallingRightWall2.GetComponent<CollisionComponent>().m_touched = false;
                m_fallingRightWall3.GetComponent<CollisionComponent>().m_touched = false;
            }

            // If the score is 3 or -3, set the win or lose state
            if (NetworkManager.m_Instance != null)
                switch (NetworkManager.m_Instance.m_lobbyScore)
                {
                    case <= -3:
                        m_Lose = true;
                        break;

                    case >= 3:
                        m_Win = true;
                        break;
                }
        }

        // If the game has ended, reset the lobby
        if (m_Lose || m_Win || m_Draw) ResetLobby();

        // If the game has ended, set the game ending state
        if (!m_gameEnding && (m_Lose || m_Win || m_Draw))
        {
            m_gameEnding = true;

            m_GameTimer = 0;
        }

        // If the game has ended, set the game ending state
        if (m_GameTimer < m_drawTime && (!m_Lose || !m_Win)) m_Draw = true;

        // Switch the game mode state
        switch (m_GameModeState)
        {
            case GameModeState.AWAKE:
                m_GameModeState = GameModeState.STARTING;
                break;

            case GameModeState.STARTING:

                m_GameModeState = GameModeState.PLAYING;

                break;

            case GameModeState.PLAYING:

                // If the timer is less than the end time, set the game mode state to ending
                if (m_GameTimer < m_endTime) m_GameModeState = GameModeState.ENDING;

                break;

            case GameModeState.ENDING:

                if (NetworkManager.m_Instance != null)
                {
                    // Reset the lobby score
                    NetworkManager.m_Instance.m_lobbyScore = 0;

                    if (NetworkManager.m_Instance.m_inLobby1)
                    {
                        // Leave the lobby
                        NetworkManager.m_Instance.m_inLobby1 = false;

                        // Reset the lobby size
                        NetworkManager.m_Instance.m_lobby1Size = 0;

                        // Send a packet to the server to indicate that the player is no longer in the lobby
                        NetworkManager.m_Instance.TcpSendPacket(new inLobby(NetworkManager.m_Instance.m_clientID, 0),
                            true);
                        NetworkManager.m_Instance.TcpSendPacket(
                            new LobbySize(NetworkManager.m_Instance.m_lobby1Size, 1),
                            true);
                        NetworkManager.m_Instance.m_startLobby1 = false;
                        NetworkManager.m_Instance.TcpSendPacket(new startLobby(false, -1), true);

                        NetworkManager.m_Instance.TcpSendPacket(
                            new LobbyScore(NetworkManager.m_Instance.m_lobbyScore, 1),
                            true);
                    }

                    if (NetworkManager.m_Instance.m_inLobby2)
                    {
                        NetworkManager.m_Instance.m_inLobby2 = false;
                        NetworkManager.m_Instance.m_lobby2Size = 0;
                        NetworkManager.m_Instance.TcpSendPacket(new inLobby(NetworkManager.m_Instance.m_clientID, 0),
                            true);
                        NetworkManager.m_Instance.TcpSendPacket(
                            new LobbySize(NetworkManager.m_Instance.m_lobby2Size, 2),
                            true);
                        NetworkManager.m_Instance.m_startLobby2 = false;
                        NetworkManager.m_Instance.TcpSendPacket(new startLobby(false, -2), true);
                        NetworkManager.m_Instance.TcpSendPacket(
                            new LobbyScore(NetworkManager.m_Instance.m_lobbyScore, 2),
                            true);
                    }

                    if (NetworkManager.m_Instance.m_inLobby3)
                    {
                        NetworkManager.m_Instance.m_inLobby3 = false;
                        NetworkManager.m_Instance.m_lobby3Size = 0;
                        NetworkManager.m_Instance.TcpSendPacket(new inLobby(NetworkManager.m_Instance.m_clientID, 0),
                            true);
                        NetworkManager.m_Instance.TcpSendPacket(
                            new LobbySize(NetworkManager.m_Instance.m_lobby3Size, 3),
                            true);
                        NetworkManager.m_Instance.m_startLobby3 = false;
                        NetworkManager.m_Instance.TcpSendPacket(new startLobby(false, -3), true);
                        NetworkManager.m_Instance.TcpSendPacket(
                            new LobbyScore(NetworkManager.m_Instance.m_lobbyScore, 3),
                            true);
                    }

                    // Reset a bunch of client side variables
                    NetworkManager.m_Instance.m_oppID = -1;
                    NetworkManager.m_Instance.m_oppColour = System.Drawing.Color.White;
                    NetworkManager.m_Instance.m_oppColourInfoConfirmed = false;
                    NetworkManager.m_Instance.m_oppUsernameInfoConfirmed = false;
                    NetworkManager.m_Instance.m_playerColour = System.Drawing.Color.White;
                    NetworkManager.m_Instance.m_reader.DiscardBufferedData();
                    NetworkManager.m_Instance.m_writer.Flush();
                    NetworkManager.m_Instance.m_resetLobby = false;
                    NetworkManager.m_Instance.m_emoteThreadStop = true;
                    NetworkManager.m_Instance.m_oppMovementDirection = MovementDirection.None;
                }

                // Destroy the players
                m_RemotePlayer.Destroy();
                m_Player.Destroy();

                //Remove this scene and load the lobby select scene
                m_Manager.RemoveScene(this);
                m_Manager.LoadScene(new LobbySelect(m_Manager));

                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    #endregion

    #region Emote Handle Thread

    private void EmoteHandler()
    {
        // Set the Default Emote
        m_emote[0] = true;

        if (NetworkManager.m_Instance == null) return;
        NetworkManager.m_Instance.m_oppEmote[0] = true; // Set the default emote for the opponent

        // While the thread is not stopped
        while (!NetworkManager.m_Instance.m_emoteThreadStop)
        {
            // Set the emote based on the key pressed
            if (Keyboard.GetState().IsKeyDown(Keys.D1) && !m_emote[0])
            {
                // Set all the emotes to false
                for (var i = 0; i < m_emote.Length; i++) m_emote[i] = false;

                // Set the player texture to the emote
                m_Player.m_SpriteRenderer.SetTexture(m_Player, "Neutral Cube");

                // Send the emote to the server
                if (NetworkManager.m_Instance.m_inLobby1)
                    NetworkManager.m_Instance.TcpSendPacket(new Emote(0, 1), true);
                else if (NetworkManager.m_Instance.m_inLobby2)
                    NetworkManager.m_Instance.TcpSendPacket(new Emote(0, 2), true);
                else if (NetworkManager.m_Instance.m_inLobby3)
                    NetworkManager.m_Instance.TcpSendPacket(new Emote(0, 3), true);

                // Set the emote to true
                m_emote[0] = true;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.D2) && !m_emote[1])
            {
                for (var i = 0; i < m_emote.Length; i++) m_emote[i] = false;

                m_Player.m_SpriteRenderer.SetTexture(m_Player, "Sad Cube");
                if (NetworkManager.m_Instance.m_inLobby1)
                    NetworkManager.m_Instance.TcpSendPacket(new Emote(1, 1), true);
                else if (NetworkManager.m_Instance.m_inLobby2)
                    NetworkManager.m_Instance.TcpSendPacket(new Emote(1, 2), true);
                else if (NetworkManager.m_Instance.m_inLobby3)
                    NetworkManager.m_Instance.TcpSendPacket(new Emote(1, 3), true);
                m_emote[1] = true;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.D3) && !m_emote[2])
            {
                for (var i = 0; i < m_emote.Length; i++) m_emote[i] = false;

                m_Player.m_SpriteRenderer.SetTexture(m_Player, "Happy Cube");
                if (NetworkManager.m_Instance.m_inLobby1)
                    NetworkManager.m_Instance.TcpSendPacket(new Emote(2, 1), true);
                else if (NetworkManager.m_Instance.m_inLobby2)
                    NetworkManager.m_Instance.TcpSendPacket(new Emote(2, 2), true);
                else if (NetworkManager.m_Instance.m_inLobby3)
                    NetworkManager.m_Instance.TcpSendPacket(new Emote(2, 3), true);
                m_emote[2] = true;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.D4) && !m_emote[3])
            {
                for (var i = 0; i < m_emote.Length; i++) m_emote[i] = false;

                m_Player.m_SpriteRenderer.SetTexture(m_Player, "Eyes Cube");
                if (NetworkManager.m_Instance.m_inLobby1)
                    NetworkManager.m_Instance.TcpSendPacket(new Emote(3, 1), true);
                else if (NetworkManager.m_Instance.m_inLobby2)
                    NetworkManager.m_Instance.TcpSendPacket(new Emote(3, 2), true);
                else if (NetworkManager.m_Instance.m_inLobby3)
                    NetworkManager.m_Instance.TcpSendPacket(new Emote(3, 3), true);
                m_emote[3] = true;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.D5) && !m_emote[4])
            {
                for (var i = 0; i < m_emote.Length; i++) m_emote[i] = false;

                m_Player.m_SpriteRenderer.SetTexture(m_Player, "Shifty Cube");
                if (NetworkManager.m_Instance.m_inLobby1)
                    NetworkManager.m_Instance.TcpSendPacket(new Emote(4, 1), true);
                else if (NetworkManager.m_Instance.m_inLobby2)
                    NetworkManager.m_Instance.TcpSendPacket(new Emote(4, 2), true);
                else if (NetworkManager.m_Instance.m_inLobby3)
                    NetworkManager.m_Instance.TcpSendPacket(new Emote(4, 3), true);
                m_emote[4] = true;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.D6) && !m_emote[5])
            {
                for (var i = 0; i < m_emote.Length; i++) m_emote[i] = false;

                m_Player.m_SpriteRenderer.SetTexture(m_Player, "Surprised Cube");
                if (NetworkManager.m_Instance.m_inLobby1)
                    NetworkManager.m_Instance.TcpSendPacket(new Emote(5, 1), true);
                else if (NetworkManager.m_Instance.m_inLobby2)
                    NetworkManager.m_Instance.TcpSendPacket(new Emote(5, 2), true);
                else if (NetworkManager.m_Instance.m_inLobby3)
                    NetworkManager.m_Instance.TcpSendPacket(new Emote(5, 3), true);
                m_emote[5] = true;
            }

            // Set the opponent's emote based on the packet received
            if (NetworkManager.m_Instance.m_oppEmote[0])
                m_RemotePlayer.m_SpriteRenderer.SetTexture(m_RemotePlayer, "Neutral Cube");
            else if (NetworkManager.m_Instance.m_oppEmote[1])
                m_RemotePlayer.m_SpriteRenderer.SetTexture(m_RemotePlayer, "Sad Cube");
            else if (NetworkManager.m_Instance.m_oppEmote[2])
                m_RemotePlayer.m_SpriteRenderer.SetTexture(m_RemotePlayer, "Happy Cube");
            else if (NetworkManager.m_Instance.m_oppEmote[3])
                m_RemotePlayer.m_SpriteRenderer.SetTexture(m_RemotePlayer, "Eyes Cube");
            else if (NetworkManager.m_Instance.m_oppEmote[4])
                m_RemotePlayer.m_SpriteRenderer.SetTexture(m_RemotePlayer, "Shifty Cube");
            else if (NetworkManager.m_Instance.m_oppEmote[5])
                m_RemotePlayer.m_SpriteRenderer.SetTexture(m_RemotePlayer, "Surprised Cube");

            Thread.Sleep(50); // To stop packet spam
        }
    }

    #endregion
}