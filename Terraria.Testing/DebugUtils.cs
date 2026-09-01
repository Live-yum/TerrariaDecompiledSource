using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.Testing.ChatCommands;
using Terraria.UI.Chat;

namespace Terraria.Testing;

public static class DebugUtils
{
	private static char[] _slopeIcons = new char[5] { '⬓', '◣', '◢', '◤', '◥' };

	internal static string GetTileDescription(int x, int y)
	{
		Tile tile = Main.tile[x, y];
		if (tile == null)
		{
			return "";
		}
		Point point = (Main.LocalPlayer.Bottom + new Vector2(-8f, 8f)).ToTileCoordinates();
		string text = default(string);
		if (!TileID.Search.TryGetName((int)tile.type, ref text))
		{
			text = "Unknown";
		}
		string text2 = default(string);
		if (!WallID.Search.TryGetName((int)tile.wall, ref text2))
		{
			text2 = "Unknown";
		}
		string text3 = "   ";
		return text3 + "Pos: " + x + ", " + y + "\n" + text3 + "Type: " + tile.type + ((tile.blockType() == 0) ? "" : (" " + _slopeIcons[tile.blockType() - 1])) + " (" + text + ")\n" + text3 + "Frame: " + tile.frameX + ", " + tile.frameY + "\n" + text3 + "FrameImportant: " + Main.tileFrameImportant[tile.type].ToString() + "\n" + text3 + "Liquid: " + tile.liquid + " (" + tile.liquidType() + ")\n" + text3 + "Wall: " + tile.wall + " (" + text2 + ")\n" + text3 + "Compare Spot: " + point.X + ", " + point.Y + "\n" + text3 + "Chunk: " + x / 200 + ", " + y / 150 + "\n" + text3 + "Paints: " + tile.color() + (tile.fullbrightBlock() ? " fullbright" : "") + (tile.invisibleBlock() ? " echo" : "") + ", " + tile.wallColor() + (tile.fullbrightWall() ? " fullbright" : "") + (tile.invisibleWall() ? " echo" : "") + "\n" + text3 + "Light: " + Lighting.GetColor(x, y);
	}

	public static void QuickSPMessage(string message)
	{
		ChatManager.DebugCommands.Process(new DebugMessage((byte)Main.myPlayer, message));
	}
}
