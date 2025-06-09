// DID NOT MODIFY THIS FILE (DO NOT MARK)
// DID NOT MODIFY THIS FILE (DO NOT MARK)
// DID NOT MODIFY THIS FILE (DO NOT MARK)

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Multiplayer_Games_Programming_Framework.GameCode.Scenes;
using System.Collections.Generic;
using System.Linq;

namespace Multiplayer_Games_Programming_Framework.Core.Scene;

internal class SceneManager
{
    private readonly List<Scene> m_Scenes;

    public SceneManager(MainGame game)
    {
        m_Game = game;
        m_SpriteBatch = game.m_SpriteBatch;
        m_ContentManager = game.Content;

        m_Scenes = new List<Scene>();

        LoadScene(new MenuScene(this));
    }

    public MainGame m_Game { get; private set; }
    public SpriteBatch m_SpriteBatch { get; private set; }
    public ContentManager m_ContentManager { get; private set; }
    public Scene m_ActiveScene { get; private set; }

    public void Update(float deltaTime)
    {
        m_ActiveScene?.Update(deltaTime);
    }

    public void Draw(float deltaTime)
    {
        m_ActiveScene?.Draw(deltaTime);
    }

    #region Scene Control

    public void LoadScene(Scene scene)
    {
        m_ActiveScene = scene;
        m_ActiveScene.LoadContent();
    }

    public void LoadSceneByName(string name)
    {
        foreach (var t in m_Scenes.Where(t => t.m_Name == name))
        {
            m_ActiveScene = t;
            break;
        }

        m_ActiveScene.LoadContent();
    }

    public void AddScene(Scene scene)
    {
        m_Scenes.Add(scene);
    }

    public void AddAndLoadScene(Scene scene)
    {
        m_Scenes.Add(scene);
        m_ActiveScene = scene;
        m_ActiveScene.LoadContent();
    }

    public void RemoveScene(Scene scene)
    {
        m_Scenes.Remove(scene);
    }

    public void RemoveSceneByName(string name)
    {
        for (var i = 0; i < m_Scenes.Count; ++i)
        {
            if (m_Scenes[i].m_Name != name) continue;
            m_Scenes.RemoveAt(i);
            break;
        }
    }

    #endregion Scene Control
}