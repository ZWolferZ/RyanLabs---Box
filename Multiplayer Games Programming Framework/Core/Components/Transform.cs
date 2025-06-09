// DID NOT MODIFY THIS FILE (DO NOT MARK)
// DID NOT MODIFY THIS FILE (DO NOT MARK)
// DID NOT MODIFY THIS FILE (DO NOT MARK)

using Microsoft.Xna.Framework;
using System;

namespace Multiplayer_Games_Programming_Framework.Core.Components;

internal class Transform
{
    public Vector2 Position;
    public float Rotation;

    public Transform()
    {
        Position = new Vector2(0, 0);
        Scale = new Vector2(1, 1);
        Rotation = 0;

        var direction = Direction.Up;

        switch (direction)
        {
            case Direction.Up:
                Console.WriteLine("Direction is Up");
                break;

            case Direction.Down:
                Console.WriteLine("Direction is Down");
                break;

            case Direction.Left:
                Console.WriteLine("Direction is Left");
                break;

            case Direction.Right:
                Console.WriteLine("Direction is Right");
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public Transform(Vector2 position)
    {
        Position = position;
        Scale = new Vector2(1, 1);
        Rotation = 0;
    }

    public Transform(Vector2 position, Vector2 scale, float rotation)
    {
        Position = position;
        Scale = scale;
        Rotation = rotation;
    }

    public Vector2 Scale { get; set; }

    public Vector2 Right
    {
        get
        {
            var RotationRad = MathHelper.ToRadians(Rotation);
            var right = new Vector2((float)Math.Cos(RotationRad), (float)Math.Sin(RotationRad));
            right.Normalize();
            return right;
        }
    }

    public Vector2 Up
    {
        get
        {
            var RotationRad = MathHelper.ToRadians(Rotation);
            var up = new Vector2((float)-Math.Sin(RotationRad), (float)Math.Cos(RotationRad));
            up.Normalize();
            return up;
        }
    }

    public event Action OnScaleChanged;

    public void SetScale(Vector2 scale)
    {
        Scale = scale;
        OnScaleChanged?.Invoke();
    }

    private enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
}