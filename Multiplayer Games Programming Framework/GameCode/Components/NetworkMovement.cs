#region Includes

// Includes
using Microsoft.Xna.Framework;
using Multiplayer_Games_Programming_Framework.Core;
using Multiplayer_Games_Programming_Framework.Core.Components;
using Multiplayer_Games_Programming_Packet_Library;
using System;

namespace Multiplayer_Games_Programming_Framework.GameCode.Components;

#endregion

internal class NetworkMovement : Component
{
    #region Member Variables

    // Member Variables
    private readonly int m_index;

    private Vector2 m_input;
    private bool m_isDirty;
    private Vector2 m_movement;
    private Vector2 m_position;
    private Rigidbody m_Rigidbody;
    private readonly int m_speed = 100;

    #endregion

    #region Constructor Methods

    // Constructor
    public NetworkMovement(GameObject gameObject, int index, Vector2 startingPosition) : base(gameObject)
    {
        m_index = index;
        m_position = startingPosition;
    }

    protected override void Start(float deltaTime)
    {
        // Set the rigidbody to the rigidbody component of the game object
        m_Rigidbody = m_GameObject.GetComponent<Rigidbody>();

        // Add the player position to the dictionary
        if (NetworkManager.m_Instance != null) NetworkManager.m_Instance.m_playerPositions.Add(m_index, UpdatePosition);
    }

    public override void Destroy()
    {
        // Remove the player position from the dictionary
        if (NetworkManager.m_Instance != null) NetworkManager.m_Instance.m_playerPositions.Remove(m_index);
        base.Destroy();
    }

    #endregion

    #region Update Position Methods

    protected override void Update(float deltaTime)
    {
        // Dirty check
        if (!m_isDirty) return;

        // Update the position of the player
        m_Rigidbody.UpdatePosition(m_position);

        // Get the input from the network manager
        m_input = Vector2.Zero;

        if (NetworkManager.m_Instance != null)
            switch (NetworkManager.m_Instance.m_oppMovementDirection)
            {
                case MovementDirection.None:
                    m_input = Vector2.Zero;
                    break;

                case MovementDirection.Left:
                    m_input.X = -5;
                    break;

                case MovementDirection.Right:
                    m_input.X = 5;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

        // Calculate the movement vector
        m_movement = m_Transform.Right * m_input.X + m_Transform.Up * m_input.Y;

        // Set the velocity of the player based on client prediction
        m_Rigidbody.m_Body.LinearVelocity = m_movement * m_speed * deltaTime;

        // Set the dirty flag to false
        m_isDirty = false;
        base.Update(deltaTime);
    }

    // Gather the position of the player
    public void UpdatePosition(Vector2 pos)
    {
        m_position = pos;
        m_isDirty = true;
    }

    #endregion
}