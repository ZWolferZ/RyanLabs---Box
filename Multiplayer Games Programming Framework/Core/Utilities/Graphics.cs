// DID NOT MODIFY THIS FILE (DO NOT MARK)
// DID NOT MODIFY THIS FILE (DO NOT MARK)
// DID NOT MODIFY THIS FILE (DO NOT MARK)

using Microsoft.Xna.Framework.Graphics;

namespace Multiplayer_Games_Programming_Framework.Core.Utilities;

internal static class Graphics
{
    public static GraphicsDevice GraphicsDevice;

    public static readonly int InitScreenWidth = 1280;
    public static readonly int InitScreenHeight = 720;

    //total amount of pixels of horizontal pixels
    public static float PixelsPerWorldUnit = 1000.0f;

    public static Viewport Viewport => GraphicsDevice.Viewport;
}