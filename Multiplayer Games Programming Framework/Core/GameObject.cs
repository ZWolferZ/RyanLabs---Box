// DID NOT MODIFY THIS FILE (DO NOT MARK)
// DID NOT MODIFY THIS FILE (DO NOT MARK)
// DID NOT MODIFY THIS FILE (DO NOT MARK)

using Multiplayer_Games_Programming_Framework.Core.Components;
using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Dynamics.Contacts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Multiplayer_Games_Programming_Framework.Core;

internal class GameObject
{
    private readonly List<Component> m_GameComponents;
    public string m_Name;
    public Transform m_Transform;

    public GameObject(Scene.Scene scene, Transform transform)
    {
        GUID = Guid.NewGuid();
        m_Name = "GameObject " + Guid.NewGuid();
        m_Scene = scene;
        m_Transform = transform;
        m_GameComponents = new List<Component>();
    }

    public Scene.Scene m_Scene { get; }

    public Guid GUID { get; }

    private event Action<float> OnStartCalls;

    private event Action<float> OnDrawCalls;

    private event Action<float> OnUpdateCalls;

    private event Action<float> OnLateUpdateCalls;

    private event Action<Component> OnComponentAdded;

    private event Action<Fixture, Fixture, Contact> OnCollisionEnter;

    private event Action<Fixture, Fixture, Contact> OnCollisionExit;

    ~GameObject()
    {
        Console.WriteLine(m_Name + " destroyed");
    }

    /// <summary>
    ///     Creates an instance of a GameObject and adds it to the scenes object list
    /// </summary>
    /// <typeparam name="T">GameObject Type</typeparam>
    /// <param name="scene">Reference to the scene this object is located in</param>
    /// <param name="transform">Position, Rotation and Scale of object</param>
    /// <param name="components">Components to be added on Creation</param>
    /// <returns>GameObject reference</returns>
    public static T Instantiate<T>(Scene.Scene scene, Transform transform) where T : GameObject
    {
        if (Activator.CreateInstance(typeof(T), scene, transform) is not GameObject go) return null;
        go.LoadContent();
        scene.AddGameObject(go);
        return go as T;
    }

    /// <summary>
    ///     Adds a component to a GameObject Reference
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="gameObject"></param>
    /// <param name="component"></param>
    /// <returns></returns>
    public T AddComponent<T>(T component) where T : Component
    {
        m_GameComponents.Add(component);

        string[] methodNames = { "Start", "Draw", "Update", "LateUpdate" };

        foreach (var t in methodNames)
        {
            var method = component.CheckIfUsingGameLoopMethod(t);
            if (method != null) RegisterComponentMethods(component, method);
        }

        OnComponentAdded?.Invoke(component);

        var comMethod = component.CheckIfUsingComponentAddedMethod("ComponentAdded");
        if (comMethod != null) OnComponentAdded += comMethod;

        string[] colMethodNames = { "OnCollisionEnter", "OnCollisionExit" };

        foreach (var t in colMethodNames)
        {
            var colMethod = component.CheckIfUsingCollisionMethods(t);
            if (colMethod != null)
                switch (t)
                {
                    case "OnCollisionEnter":
                        OnCollisionEnter += colMethod;
                        break;

                    case "OnCollisionExit":
                        OnCollisionExit += colMethod;
                        break;
                }
        }

        return component;
    }

    /// <summary>
    ///     Returns the first component of type T
    /// </summary>
    /// <typeparam name="T">Component Type</typeparam>
    /// <returns>Returns the first component of type T</returns>
    public T GetComponent<T>() where T : Component
    {
        return m_GameComponents.OfType<T>().Select(component => component).FirstOrDefault();
    }

    /// <summary>
    ///     Returns a list of components of type T
    /// </summary>
    /// <typeparam name="T">Component type</typeparam>
    /// <returns>Returns a list of components of type T</returns>
    public List<T> GetComponents<T>() where T : Component
    {
        return m_GameComponents.OfType<T>().Select(component => component).ToList();
    }

    public void RemoveComponent(Component component)
    {
        var removed = m_GameComponents.Remove(component);
        if (!removed) return;
        DeregisterGameLoopCall(component);
        DeregisterCollisionCall(component);
        DeregisterOnComponentAddedCall(component);
        component.Destroy();
    }

    /// <summary>
    ///     Destroys the GameObject
    /// </summary>
    public void Destroy()
    {
        m_Scene.RemoveGameObject(this);

        for (var i = m_GameComponents.Count - 1; i >= 0; --i) RemoveComponent(m_GameComponents[i]);

        m_GameComponents.Clear();
        m_Transform = null;
    }

    #region GameLoop methods

    /// <summary>
    ///     Called once the object has been instantiated but before added to the scene
    /// </summary>
    protected virtual void LoadContent()
    {
    }

    private void Start(float deltaTime)
    {
        OnStartCalls?.Invoke(deltaTime);
        OnStartCalls = null;
    }

    private void Draw(float deltaTime)
    {
        OnDrawCalls?.Invoke(deltaTime);
    }

    private void Update(float deltaTime)
    {
        OnUpdateCalls?.Invoke(deltaTime);
    }

    private void LateUpdate(float deltaTime)
    {
        OnLateUpdateCalls?.Invoke(deltaTime);
    }

    #endregion GameLoop methods

    #region Register methods

    private void RegisterComponentMethods(Component component, Action<float> method)
    {
        switch (method.Method.Name)
        {
            case "Start":
                OnStartCalls += method;

                if (OnStartCalls != null && OnStartCalls.GetInvocationList().Length == 1)
                    m_Scene.RegisterGameLoopCall(Start);
                break;

            case "Draw":
                OnDrawCalls += method;

                if (OnDrawCalls != null && OnDrawCalls.GetInvocationList().Length == 1)
                    m_Scene.RegisterGameLoopCall(Draw);
                break;

            case "Update":
                OnUpdateCalls += method;

                if (OnUpdateCalls != null && OnUpdateCalls.GetInvocationList().Length == 1)
                    m_Scene.RegisterGameLoopCall(Update);
                break;

            case "LateUpdate":
                OnLateUpdateCalls += method;

                if (OnLateUpdateCalls != null && OnLateUpdateCalls.GetInvocationList().Length == 1)
                    m_Scene.RegisterGameLoopCall(LateUpdate);
                break;

            default:
                Debug.Fail("Invalid function call");
                break;
        }
    }

    public void DeregisterGameLoopCall(Component component)
    {
        void RemoveCallFromScene(string methodName)
        {
            switch (methodName)
            {
                case "Start":
                    if (OnStartCalls?.GetInvocationList().Length == 0) m_Scene.DeregisterGameLoopCall(Start);
                    break;

                case "Draw":
                    if (OnDrawCalls?.GetInvocationList().Length == 0) m_Scene.DeregisterGameLoopCall(Draw);
                    break;

                case "Update":
                    if (OnUpdateCalls?.GetInvocationList().Length == 0) m_Scene.DeregisterGameLoopCall(Update);
                    break;

                case "LateUpdate":
                    if (OnLateUpdateCalls?.GetInvocationList().Length == 0) m_Scene.DeregisterGameLoopCall(LateUpdate);
                    break;
            }
        }

        void CheckCalls((bool, string) method)
        {
            if (method.Item1) RemoveCallFromScene(method.Item2);
        }

        CheckCalls(UnsubscribeFromEvent(ref OnStartCalls));
        CheckCalls(UnsubscribeFromEvent(ref OnUpdateCalls));
        CheckCalls(UnsubscribeFromEvent(ref OnLateUpdateCalls));
        CheckCalls(UnsubscribeFromEvent(ref OnDrawCalls));
        return;

        (bool, string) UnsubscribeFromEvent(ref Action<float> eventListener)
        {
            var del = eventListener?.GetInvocationList();
            var hadCall = false;
            var methodName = "";

            if (del == null) return (false, methodName);
            foreach (var d in del)
            {
                if (d.Target != component) continue;
                eventListener -= (Action<float>)d;
                hadCall = true;
                methodName = d.Method.Name;
                break;
            }

            return (hadCall, methodName);
        }
    }

    public void DeregisterCollisionCall(Component component)
    {
        UnsubscribeFromEvent(ref OnCollisionEnter);
        UnsubscribeFromEvent(ref OnCollisionExit);
        return;

        void UnsubscribeFromEvent(ref Action<Fixture, Fixture, Contact> eventListener)
        {
            var del = eventListener?.GetInvocationList();

            if (del == null) return;
            foreach (var d in del)
            {
                if (d.Target != component) continue;
                eventListener -= (Action<Fixture, Fixture, Contact>)d;
                break;
            }
        }
    }

    public void DeregisterOnComponentAddedCall(Component component)
    {
        UnsubscribeFromEvent(ref OnComponentAdded);
        return;

        void UnsubscribeFromEvent(ref Action<Component> eventListener)
        {
            var del = eventListener?.GetInvocationList();

            if (del == null) return;
            foreach (var d in del)
            {
                if (d.Target != component) continue;
                eventListener -= (Action<Component>)d;
                break;
            }
        }
    }

    #endregion Register methods

    #region Collision Methods

    public bool CollisionEnter(Fixture sender, Fixture other, Contact contact)
    {
        OnCollisionEnter?.Invoke(sender, other, contact);
        return true;
    }

    public void CollisionExit(Fixture sender, Fixture other, Contact contact)
    {
        OnCollisionExit?.Invoke(sender, other, contact);
    }

    #endregion Collision Methods
}