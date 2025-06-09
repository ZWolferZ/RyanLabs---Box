#region Includes

// Includes
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Multiplayer_Games_Programming_Framework.Core;
using Multiplayer_Games_Programming_Framework.Core.Components;
using Multiplayer_Games_Programming_Packet_Library;

namespace Multiplayer_Games_Programming_Framework.GameCode.Components;

#endregion

internal class Movement : Component
{
    #region Member Variables

    // Member Variables
    private readonly float m_Speed;

    private MovementDirection m_movementDirection = MovementDirection.None;
    public Vector2 m_previousPosition;
    private Rigidbody m_Rigidbody;

    #endregion

    #region Constructor

    public Movement(GameObject gameObject, float speed) : base(gameObject)
    {
        m_Speed = speed;
    }

    // Set the rigidbody to the rigidbody component of the game object
    protected override void Start(float deltaTime)
    {
        m_Rigidbody = m_GameObject.GetComponent<Rigidbody>();
    }

    #endregion

    #region Movement Methods

    protected override void Update(float deltaTime)
    {
        // Get the input from the keyboard
        var input = Vector2.Zero;

        if (Keyboard.GetState().IsKeyDown(Keys.Left))
        {
            // If the player is moving left, set the input to -5
            input.X = -5;
            m_movementDirection = MovementDirection.Left;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.Right))
        {
            // If the player is moving right, set the input to 5
            input.X = 5;
            m_movementDirection = MovementDirection.Right;
        }
        else
        {
            // If the player is not moving left or right, set the movement direction to none
            m_movementDirection = MovementDirection.None;
        }

        // Calculate the movement vector
        var movement = m_Transform.Right * input.X + m_Transform.Up * input.Y;

        // Set the velocity of the rigidbody to the movement vector multiplied by the speed
        m_Rigidbody.m_Body.LinearVelocity = movement * m_Speed * deltaTime;

        // A tiny bit of dead reckoning to reduce total packets sent
        if (m_Transform.Position == m_previousPosition) return;

        // Get the current lobby number
        var lobbyNumber = GetCurrentLobbyNumber();

        // If the lobby number is not 0, send the position of the player to the server
        if (lobbyNumber != 0)
            if (NetworkManager.m_Instance != null)
                NetworkManager.m_Instance.UdpSendPacket(new Position(
                    NetworkManager.m_Instance.m_clientID,
                    m_Transform.Position.X,
                    m_Transform.Position.Y,
                    m_movementDirection,
                    lobbyNumber
                ), true);

        // Set the previous position to the current position
        m_previousPosition = m_Transform.Position;
    }

    // I realise I could have used this method like everywhere, but this was one of the last files I touched
    private static int GetCurrentLobbyNumber()
    {
        // Return the current lobby number
        return NetworkManager.m_Instance != null && NetworkManager.m_Instance.m_inLobby1 ? 1 :
            NetworkManager.m_Instance != null && NetworkManager.m_Instance.m_inLobby2 ? 2 :
            NetworkManager.m_Instance != null && NetworkManager.m_Instance.m_inLobby3 ? 3 : 0;
    }

    #endregion
}