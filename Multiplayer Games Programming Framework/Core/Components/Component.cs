// DID NOT MODIFY THIS FILE (DO NOT MARK)
// DID NOT MODIFY THIS FILE (DO NOT MARK)
// DID NOT MODIFY THIS FILE (DO NOT MARK)

using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Dynamics.Contacts;
using System;
using System.Reflection;

namespace Multiplayer_Games_Programming_Framework.Core.Components;

internal abstract class Component
{
    public bool m_Enabled = true;

    public int m_ExecutionOrder = 0;

    protected Component(GameObject gameObject)
    {
        m_GameObject = gameObject;
    }

    public GameObject m_GameObject { get; private set; }
    public Transform m_Transform => m_GameObject.m_Transform;

    public virtual void Destroy()
    {
        {
            m_GameObject.RemoveComponent(this);
            m_GameObject = null;
        }
    }

    #region Game Loop

    protected virtual void Start(float deltaTime)
    {
    }

    protected virtual void Draw(float deltaTime)
    {
    }

    protected virtual void Update(float deltaTime)
    {
    }

    protected virtual void LateUpdate(float deltaTime)
    {
    }

    protected virtual void ComponentAdded(Component component)
    {
    }

    protected virtual void OnCollisionEnter(Fixture sender, Fixture other, Contact contact)
    {
    }

    protected virtual void OnCollisionExit(Fixture sender, Fixture other, Contact contact)
    {
    }

    #endregion Game Loop

    #region Reflection Checks

    public Action<float> CheckIfUsingGameLoopMethod(string methodName)
    {
        var type = GetType();

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var method = type.GetMethods(flags);

        foreach (var t in method)
            if (t.DeclaringType != null && t.DeclaringType.Name == type.Name && t.Name == methodName)
                switch (methodName)
                {
                    case "Start":
                        return Start;

                    case "Draw":
                        return Draw;

                    case "Update":
                        return Update;

                    case "LateUpdate":
                        return LateUpdate;
                }

        return null;
    }

    public Action<Component> CheckIfUsingComponentAddedMethod(string methodName)
    {
        var type = GetType();

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var method = type.GetMethods(flags);

        foreach (var t in method)
            if (t.DeclaringType != typeof(Component) && t.Name == methodName)
                switch (methodName)
                {
                    case "ComponentAdded":
                        return ComponentAdded;
                }

        return null;
    }

    public Action<Fixture, Fixture, Contact> CheckIfUsingCollisionMethods(string methodName)
    {
        var type = GetType();

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var method = type.GetMethods(flags);

        foreach (var t in method)
            if (t.DeclaringType != null && t.DeclaringType.Name == type.Name && t.Name == methodName)
                switch (methodName)
                {
                    case "OnCollisionEnter":
                        return OnCollisionEnter;

                    case "OnCollisionExit":
                        return OnCollisionExit;
                }

        return null;
    }

    #endregion Reflection Checks
}