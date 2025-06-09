#region Includes

// Includes
using Multiplayer_Games_Programming_Framework.Core;
using Multiplayer_Games_Programming_Framework.Core.Components;
using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Dynamics.Contacts;

namespace Multiplayer_Games_Programming_Framework.GameCode.Components;

#endregion

// This class is solely responsible for handling collisions between GameObjects,
// Look this framework's Collision detection system is not very good,
// so I just added a simple boolean to check if the GameObject is touching another GameObject
internal class CollisionComponent : Component
{
    #region THE SINGULAR MEMBER VARIABLE

    // THE BOOLEAN
    public bool m_touched;

    #endregion

    #region Constructor

    // Empty Constructor (Pretty sure I don't need it, but it scares me)
    public CollisionComponent(GameObject gameObject) : base(gameObject)
    {
    }

    #endregion

    #region Collision Methods

    // On Collision Enter, set the boolean to true
    protected override void OnCollisionEnter(Fixture sender, Fixture other, Contact contact)
    {
        m_touched = true;
    }

    // On Collision Exit, set the boolean to false
    protected override void OnCollisionExit(Fixture sender, Fixture other, Contact contact)
    {
        m_touched = false;
    }

    #endregion
}