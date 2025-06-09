// DID NOT MODIFY THIS FILE (DO NOT MARK)
// DID NOT MODIFY THIS FILE (DO NOT MARK)
// DID NOT MODIFY THIS FILE (DO NOT MARK)

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Multiplayer_Games_Programming_Framework.Core.Utilities;
using nkast.Aether.Physics2D.Dynamics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Multiplayer_Games_Programming_Framework.Core.Scene;

internal abstract class Scene
{
    protected List<GameObject> m_GameObjects;

    protected SceneManager m_Manager;

    public Matrix world;

    protected Scene(SceneManager manager)
    {
        m_Manager = manager;

        m_GameObjects = new List<GameObject>();
        m_Name = SceneName();
        m_World = CreateWorld();
        m_Camera = CreateCamera();
    }

    public string m_Name { get; private set; }

    public World m_World { get; protected set; }

    public Camera m_Camera { get; protected set; }

    private event Action<float> onStart;

    private event Action<float> onDraw;

    private event Action<float> onUpdate;

    private event Action<float> onLateUpdate;

    private event Action onDirtyGameObject;

    ~Scene()
    {
        m_GameObjects.Clear();
        onStart = null;
        onUpdate = null;
        onDraw = null;
        onLateUpdate = null;
        if (m_World == null) return;
        m_World.Clear();
        m_World = null;
    }

    protected abstract string SceneName();

    protected abstract World CreateWorld();

    protected abstract Camera CreateCamera();

    public virtual void LoadContent()
    {
    }

    public virtual void Update(float deltaTime)
    {
        onDirtyGameObject?.Invoke();
        onDirtyGameObject = null;

        onStart?.Invoke(deltaTime);
        onStart = null;

        m_World?.Step(deltaTime);

        onUpdate?.Invoke(deltaTime);
        onLateUpdate?.Invoke(deltaTime);

        m_Camera.Update();
    }

    public virtual void Draw(float deltaTime)
    {
        onDraw?.Invoke(deltaTime);
    }

    public void AddGameObject(GameObject gameObject)
    {
        m_GameObjects.Add(gameObject);
    }

    public void RemoveGameObject(GameObject gameObject)
    {
        m_GameObjects.Remove(gameObject);
    }

    public SpriteBatch GetSpriteBatch()
    {
        return m_Manager.m_SpriteBatch;
    }

    public ContentManager GetContentManager()
    {
        return m_Manager.m_ContentManager;
    }

    public void RegisterGameLoopCall(Action<float> method)
    {
        switch (method.Method.Name)
        {
            case "Start":
                onStart += method;
                break;

            case "Draw":
                onDraw += method;
                break;

            case "Update":
                onUpdate += method;
                break;

            case "LateUpdate":
                onLateUpdate += method;
                break;

            default:
                Debug.Fail("Invalid function call");
                break;
        }
    }

    public void DeregisterGameLoopCall(Action<float> method)
    {
        switch (method.Method.Name)
        {
            case "Start":
                onStart -= method;
                break;

            case "Draw":
                onDraw -= method;
                break;

            case "Update":
                onUpdate -= method;
                break;

            case "LateUpdate":
                onLateUpdate -= method;
                break;

            default:
                Debug.Fail("Invalid function call");
                break;
        }
    }

    public void RegisterCheckDirtyGameObjectCall(Action method)
    {
        onDirtyGameObject += method;
    }

    public void DeregisterCheckDirtyGameObjectCall(Action method)
    {
        onDirtyGameObject -= method;
    }
}