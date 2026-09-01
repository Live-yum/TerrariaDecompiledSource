using Microsoft.Xna.Framework;

namespace Terraria.Graphics.CameraModifiers;

public struct CameraInfo(Vector2 position)
{
	public Vector2 CameraPosition = OriginalCameraPosition;

	public Vector2 OriginalCameraCenter = position + Main.ScreenSize.ToVector2() / 2f;

	public Vector2 OriginalCameraPosition = position;
}
