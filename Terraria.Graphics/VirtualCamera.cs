using Microsoft.Xna.Framework;

namespace Terraria.Graphics;

public struct VirtualCamera(Player player)
{
	public readonly Player Player = player;

	public Vector2 Position => Center - Size * 0.5f;

	public Vector2 Size => new Vector2(Main.maxScreenW, Main.maxScreenH);

	public Vector2 Center => Player.Center;
}
