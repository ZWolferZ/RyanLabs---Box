// DID NOT MODIFY THIS FILE (DO NOT MARK)
// DID NOT MODIFY THIS FILE (DO NOT MARK)
// DID NOT MODIFY THIS FILE (DO NOT MARK)

using Microsoft.Xna.Framework;
using Multiplayer_Games_Programming_Framework.Core;
using Multiplayer_Games_Programming_Framework.Core.Components;
using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Dynamics.Contacts;

namespace Multiplayer_Games_Programming_Framework.GameCode.Components;

internal class BallControllerComponent : Component
{
    private Vector2 m_InitDirection;
    private Rigidbody m_Rigidbody;
    private float m_Speed;

    public BallControllerComponent(GameObject gameObject) : base(gameObject)
    {
    }

    protected override void Start(float deltaTime)
    {
        m_Rigidbody = m_GameObject.GetComponent<Rigidbody>();
    }

    public void Init(float speed, Vector2 direction)
    {
        m_Speed = speed;
        m_InitDirection = direction;
    }

    public void StartBall()
    {
        m_Rigidbody.m_Body.LinearVelocity = m_InitDirection * m_Speed;
    }

    protected override void OnCollisionEnter(Fixture sender, Fixture other, Contact contact)
    {
        var normal = contact.Manifold.LocalNormal;
        var velocity = m_Rigidbody.m_Body.LinearVelocity;
        var reflection = Vector2.Reflect(velocity, normal);
        m_Rigidbody.m_Body.LinearVelocity = reflection * 1.0f;
    }
}