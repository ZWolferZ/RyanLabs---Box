// DID NOT MODIFY THIS FILE (DO NOT MARK)
// DID NOT MODIFY THIS FILE (DO NOT MARK)
// DID NOT MODIFY THIS FILE (DO NOT MARK)

using Multiplayer_Games_Programming_Framework.Core;
using Multiplayer_Games_Programming_Framework.Core.Components;

namespace Multiplayer_Games_Programming_Framework.GameCode.Components;

internal class DestroyInTime : Component
{
    private float m_Timer;

    public DestroyInTime(GameObject gameObject, float time) : base(gameObject)
    {
        m_Timer = time;
    }

    protected override void Update(float deltaTime)
    {
        m_Timer -= deltaTime;
        if (m_Timer <= 0) m_GameObject.Destroy();
    }
}