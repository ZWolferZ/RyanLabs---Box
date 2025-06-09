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

internal class PhysicsGO : GameObject
{
    public PhysicsGO(Scene scene, Transform transform) : base(scene, transform)
    {
        var rng = new Random();

        var sr = AddComponent(new SpriteRenderer(this, "Square(10x10)"));
        sr.m_DepthLayer = 0;
        sr.m_Color = new Color((float)rng.NextDouble(), (float)rng.NextDouble(), (float)rng.NextDouble());

        var rb = AddComponent(new Rigidbody(this, BodyType.Dynamic, 1, sr.m_Size / 2));
        rb.CreateRectangle(sr.m_Size.X, sr.m_Size.Y, 0.0f, 1.0f, Vector2.Zero, Physics.GetCategoryByName("WorldObject"),
            Physics.GetCategoryByName("All"));

        AddComponent(new BallControllerComponent(this));
    }
}