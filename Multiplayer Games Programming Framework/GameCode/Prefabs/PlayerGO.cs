// Scaled down transform to use a higher resolution sprite for the player game object.
// ONLY SLIGHT MODIFICATIONS THIS FILE (DO NOT MARK)
// ONLY SLIGHT MODIFICATIONS THIS FILE (DO NOT MARK)
// ONLY SLIGHT MODIFICATIONS THIS FILE (DO NOT MARK)

using Microsoft.Xna.Framework;
using Multiplayer_Games_Programming_Framework.Core;
using Multiplayer_Games_Programming_Framework.Core.Components;
using Multiplayer_Games_Programming_Framework.Core.Scene;
using Multiplayer_Games_Programming_Framework.Core.Utilities;
using nkast.Aether.Physics2D.Dynamics;

namespace Multiplayer_Games_Programming_Framework.GameCode.Prefabs;

internal class PlayerGO : GameObject
{
    public Rigidbody m_Rigidbody;
    public SpriteRenderer m_SpriteRenderer;

    public PlayerGO(Scene scene, Transform transform) : base(scene, transform)
    {
        m_SpriteRenderer = AddComponent(new SpriteRenderer(this, "Neutral Cube"));
        m_SpriteRenderer.m_DepthLayer = 1;

        m_Transform.Scale /= 10;

        m_Rigidbody = AddComponent(new Rigidbody(this, BodyType.Dynamic, 1, m_SpriteRenderer.m_Size / 2));
        m_Rigidbody.CreateRectangle(m_SpriteRenderer.m_Size.X, m_SpriteRenderer.m_Size.Y, 0.0f, 1.0f, Vector2.Zero,
            Physics.GetCategoryByName("Player"),
            Physics.GetCategoryByName("All"));
    }
}