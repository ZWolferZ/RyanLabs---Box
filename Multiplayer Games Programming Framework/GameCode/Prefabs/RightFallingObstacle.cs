#region Includes

// Includes
using Microsoft.Xna.Framework;
using Multiplayer_Games_Programming_Framework.Core;
using Multiplayer_Games_Programming_Framework.Core.Components;
using Multiplayer_Games_Programming_Framework.Core.Scene;
using Multiplayer_Games_Programming_Framework.Core.Utilities;
using Multiplayer_Games_Programming_Framework.GameCode.Components;
using nkast.Aether.Physics2D.Dynamics;

namespace Multiplayer_Games_Programming_Framework.GameCode.Prefabs;

#endregion

internal class RightFallingObstacle : GameObject
{
    #region Member Variables

    // Member Variables
    public Rigidbody m_Rigidbody;

    public SpriteRenderer m_SpriteRenderer;

    #endregion

    #region Constructor

    public RightFallingObstacle(Scene scene, Transform transform) : base(scene, transform)
    {
        m_SpriteRenderer = AddComponent(new SpriteRenderer(this, "Square(10x10)"));
        m_SpriteRenderer.m_DepthLayer = 1;
        m_SpriteRenderer.m_Color = Color.DarkRed;

        m_Rigidbody = AddComponent(new Rigidbody(this, BodyType.Static, 10, m_SpriteRenderer.m_Size / 2));
        m_Rigidbody.CreateRectangle(m_SpriteRenderer.m_Size.X, m_SpriteRenderer.m_Size.Y, 0.0f, 1.0f, Vector2.Zero,
            Physics.GetCategoryByName("Wall"),
            Physics.GetCategoryByName("Player"));

        // If the client ID is greater than the opponent ID, add a collision component
        if (NetworkManager.m_Instance != null && NetworkManager.m_Instance.m_clientID > NetworkManager.m_Instance.m_oppID)
            AddComponent(new CollisionComponent(this));
    }

    #endregion
}