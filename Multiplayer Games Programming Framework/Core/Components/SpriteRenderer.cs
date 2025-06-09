// Added a getter and setter for the m_Texture property in the SpriteRenderer class.
// ONLY SLIGHT MODIFICATIONS THIS FILE (DO NOT MARK)
// ONLY SLIGHT MODIFICATIONS THIS FILE (DO NOT MARK)
// ONLY SLIGHT MODIFICATIONS THIS FILE (DO NOT MARK)

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Multiplayer_Games_Programming_Framework.Core.Components;

internal class SpriteRenderer : Component
{
    private readonly SpriteBatch m_SpriteBatch;
    public Color m_Color = Color.White;

    public float m_DepthLayer = 0;

    public SpriteRenderer(GameObject gameObject, string texture) : base(gameObject)
    {
        m_SpriteBatch = gameObject.m_Scene.GetSpriteBatch();
        m_Texture = gameObject.m_Scene.GetContentManager().Load<Texture2D>(texture);

        if (m_Texture == null)
        {
            Console.WriteLine("Texture not found");
            return;
        }

        m_Size = new Vector2(m_Texture.Width, m_Texture.Height);
    }

    public Texture2D m_Texture { get; set; }
    public Vector2 m_Size { get; set; }

    public void SetTexture(GameObject gameObject, string texture)
    {
        m_Texture = gameObject.m_Scene.GetContentManager().Load<Texture2D>(texture);
    }

    protected override void Draw(float deltaTime)
    {
        m_SpriteBatch.Draw(m_Texture, m_Transform.Position, null, m_Color, MathHelper.ToRadians(m_Transform.Rotation),
            m_Size / 2, m_Transform.Scale, new SpriteEffects(), m_DepthLayer);
    }
}