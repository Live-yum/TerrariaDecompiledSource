using Microsoft.Xna.Framework;

namespace Terraria.GameContent;

public class TeleportHelpers
{
	public static bool FindClosestTeleportSpotNoSpace(Player player, out Vector2 resultPosition)
	{
		bool result = false;
		resultPosition = player.position;
		player.velocity = Vector2.Zero;
		Vector2 vector = new Vector2((float)player.width * 0.5f, player.height);
		Vector2 bottom = player.Bottom;
		Point point = bottom.ToTileCoordinates();
		int value = point.X - 25;
		int value2 = point.X + 25;
		int value3 = point.Y - 25;
		int value4 = point.Y + 25;
		value = Utils.Clamp(value, 40, Main.maxTilesX - 40);
		value2 = Utils.Clamp(value2, 40, Main.maxTilesX - 40);
		value3 = Utils.Clamp(value3, 40, Main.maxTilesY - 40);
		value4 = Utils.Clamp(value4, 40, Main.maxTilesY - 40);
		float num = float.MaxValue;
		for (int i = value; i < value2; i++)
		{
			for (int j = value3; j < value4; j++)
			{
				Vector2 vector2 = new Vector2(i * 16 + 8, j * 16 + 15) - vector;
				Tile tile = Main.tile[i, j];
				Tile tile2 = Main.tile[i, j + 1];
				bool flag = WorldGen.SolidOrSlopedTile(tile) || tile.liquid > 0;
				bool flag2 = WorldGen.SolidOrSlopedTile(tile2) && tile2.liquid == 0;
				if (!TileIsDangerous(i, j) && !flag && flag2 && !Collision.LavaCollision(vector2, player.width, player.height) && !Collision.AnyHurtingTiles(vector2, player.width, player.height) && !Collision.SolidCollision(vector2, player.width, player.height))
				{
					float num2 = (vector2 - bottom).Length();
					if (num2 < num)
					{
						resultPosition = vector2;
						num = num2;
						result = true;
					}
				}
			}
		}
		return result;
	}

	public static bool RequestMagicConchTeleportPosition(Player player, int crawlOffsetX, int startX, out Point landingPoint)
	{
		landingPoint = default(Point);
		Point point = new Point(startX, 50);
		int num = 1;
		int num2 = -1;
		int num3 = 1;
		int num4 = 0;
		int num5 = 5000;
		Vector2 vector = new Vector2((float)player.width * 0.5f, player.height);
		int num6 = 40;
		bool flag = WorldGen.SolidOrSlopedTile(Main.tile[point.X, point.Y]);
		int num7 = 0;
		int num8 = 400;
		while (num4 < num5 && num7 < num8)
		{
			num4++;
			Tile tile = Main.tile[point.X, point.Y];
			Tile tile2 = Main.tile[point.X, point.Y + num3];
			bool flag2 = WorldGen.SolidOrSlopedTile(tile) || tile.liquid > 0;
			bool flag3 = WorldGen.SolidOrSlopedTile(tile2) || tile2.liquid > 0;
			if (IsInSolidTilesExtended(new Vector2(point.X * 16 + 8, point.Y * 16 + 15) - vector, player.velocity, player.width, player.height, (int)player.gravDir))
			{
				if (flag)
				{
					point.Y += num;
				}
				else
				{
					point.Y += num2;
				}
				continue;
			}
			if (flag2)
			{
				if (flag)
				{
					point.Y += num;
				}
				else
				{
					point.Y += num2;
				}
				continue;
			}
			flag = false;
			if (!IsInSolidTilesExtended(new Vector2(point.X * 16 + 8, point.Y * 16 + 15 + 16) - vector, player.velocity, player.width, player.height, (int)player.gravDir) && !flag3 && (double)point.Y < Main.worldSurface)
			{
				point.Y += num;
				continue;
			}
			if (tile2.liquid > 0)
			{
				point.X += crawlOffsetX;
				num7++;
				continue;
			}
			if (TileIsDangerous(point.X, point.Y))
			{
				point.X += crawlOffsetX;
				num7++;
				continue;
			}
			if (TileIsDangerous(point.X, point.Y + num3))
			{
				point.X += crawlOffsetX;
				num7++;
				continue;
			}
			if (point.Y >= num6)
			{
				break;
			}
			point.Y += num;
		}
		if (num4 == num5 || num7 >= num8)
		{
			return false;
		}
		if (!WorldGen.InWorld(point.X, point.Y, 40))
		{
			return false;
		}
		bool flag4 = false;
		for (int i = 0; i < 10; i++)
		{
			int num9 = point.Y + i;
			Tile tile3 = Main.tile[point.X, num9];
			if (WorldGen.SolidOrSlopedTile(tile3) || tile3.liquid > 0)
			{
				flag4 = true;
				break;
			}
		}
		if (!flag4)
		{
			return false;
		}
		landingPoint = point;
		return true;
	}

	private static bool TileIsDangerous(int x, int y)
	{
		Tile tile = Main.tile[x, y];
		if (tile.liquid > 0 && tile.lava())
		{
			return true;
		}
		if (tile.wall == 87 && (double)y > Main.worldSurface && !NPC.downedPlantBoss)
		{
			return true;
		}
		if (Main.wallDungeon[tile.wall] && (double)y > Main.worldSurface && !NPC.downedBoss3)
		{
			return true;
		}
		return false;
	}

	private static bool IsInSolidTilesExtended(Vector2 testPosition, Vector2 playerVelocity, int width, int height, int gravDir)
	{
		if (Collision.LavaCollision(testPosition, width, height))
		{
			return true;
		}
		if (Collision.AnyHurtingTiles(testPosition, width, height))
		{
			return true;
		}
		if (Collision.SolidCollision(testPosition, width, height))
		{
			return true;
		}
		Vector2 vector = Vector2.UnitX * 16f;
		if (Collision.TileCollision(testPosition - vector, vector, width, height, fallThrough: true, fall2: true, gravDir) != vector)
		{
			return true;
		}
		vector = -Vector2.UnitX * 16f;
		if (Collision.TileCollision(testPosition - vector, vector, width, height, fallThrough: true, fall2: true, gravDir) != vector)
		{
			return true;
		}
		vector = Vector2.UnitY * 16f;
		if (Collision.TileCollision(testPosition - vector, vector, width, height, fallThrough: true, fall2: true, gravDir) != vector)
		{
			return true;
		}
		vector = -Vector2.UnitY * 16f;
		if (Collision.TileCollision(testPosition - vector, vector, width, height, fallThrough: true, fall2: true, gravDir) != vector)
		{
			return true;
		}
		return false;
	}
}
