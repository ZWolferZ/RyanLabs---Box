// DID NOT MODIFY THIS FILE (DO NOT MARK)
// DID NOT MODIFY THIS FILE (DO NOT MARK)
// DID NOT MODIFY THIS FILE (DO NOT MARK)

using Microsoft.Xna.Framework;
using Multiplayer_Games_Programming_Framework.Core;
using Multiplayer_Games_Programming_Framework.Core.Components;
using Multiplayer_Games_Programming_Framework.Core.Scene;
using Multiplayer_Games_Programming_Framework.Core.Utilities;
using Multiplayer_Games_Programming_Framework.GameCode.Components;
using nkast.Aether.Physics2D.Dynamics;
using System;

namespace Multiplayer_Games_Programming_Framework.GameCode.Prefabs;

internal class BallGO : GameObject
{
    public BallGO(Scene scene, Transform transform) : base(scene, transform)
    {
        var sr = AddComponent(new SpriteRenderer(this, "ball"));
        sr.m_DepthLayer = 0;

        var rb = AddComponent(new Rigidbody(this, BodyType.Dynamic, 0.1f, sr.m_Size / 2));
        rb.m_Body.IgnoreGravity = true;
        rb.m_Body.FixedRotation = true;
        rb.CreateCircule(Math.Max(sr.m_Size.X, sr.m_Size.Y) / 2, 0.0f, 0.0f, Vector2.Zero,
            Physics.GetCategoryByName("WorldObject"),
            Physics.GetCategoryByName("WorldObject") | Physics.GetCategoryByName("Player") |
            Physics.GetCategoryByName("Wall"));

        AddComponent(new BallControllerComponent(this));
    }
}