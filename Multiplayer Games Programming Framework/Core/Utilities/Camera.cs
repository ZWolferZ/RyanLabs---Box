// DID NOT MODIFY THIS FILE (DO NOT MARK)
// DID NOT MODIFY THIS FILE (DO NOT MARK)
// DID NOT MODIFY THIS FILE (DO NOT MARK)

using Microsoft.Xna.Framework;

namespace Multiplayer_Games_Programming_Framework.Core.Utilities;

internal sealed class Camera
{
    public Camera(Vector2 position, float rotation = 0, float zoom = 1)
    {
        m_Position = position;
        m_Rotation = rotation;
        m_Zoom = Graphics.Viewport.Width / Graphics.PixelsPerWorldUnit;
        m_ZoomFactor = zoom;
        m_isDirty = true;

        UpdateViewMatrix();
    }

    public static Matrix m_Matrix { get; protected set; }
    public Vector2 m_Position { get; protected set; }

    private float m_Zoom { get; }
    public float m_ZoomFactor { get; protected set; }
    public float m_Rotation { get; protected set; }
    public bool m_isDirty { get; protected set; }

    public void Update()
    {
        UpdateViewMatrix();
    }

    public void Move(Vector2 pos)
    {
        m_Position = pos;
        m_isDirty = true;
    }

    public void SetPosition(Vector2 position)
    {
        m_Position = position;
        m_isDirty = true;
    }

    public void Rotate(float angle)
    {
        m_Rotation += angle;
        m_isDirty = true;
    }

    public void SetRotation(float rotation)
    {
        m_Rotation = rotation;
        m_isDirty = true;
    }

    public void Zoom(float zoomAdjust)
    {
        m_ZoomFactor = MathHelper.Max(m_ZoomFactor + zoomAdjust, 0.1f);
        m_isDirty = true;
    }

    public void SetZoom(float zoomFactor)
    {
        m_ZoomFactor = MathHelper.Max(zoomFactor, 0.1f);
        m_isDirty = true;
    }

    private void UpdateViewMatrix()
    {
        if (!m_isDirty) return;
        var viewCenter = new Vector2(Graphics.GraphicsDevice.Viewport.Width / 2,
            Graphics.GraphicsDevice.Viewport.Height / 2);

        var view =
            Matrix.CreateTranslation(new Vector3(-m_Position.X, -m_Position.Y, 0)) *
            Matrix.CreateRotationZ(m_Rotation) *
            Matrix.CreateScale(m_Zoom * m_ZoomFactor, m_Zoom * m_ZoomFactor, 1) *
            Matrix.CreateTranslation(new Vector3(viewCenter.X, viewCenter.Y, 0));

        m_Matrix = view;

        m_isDirty = false;
    }
}