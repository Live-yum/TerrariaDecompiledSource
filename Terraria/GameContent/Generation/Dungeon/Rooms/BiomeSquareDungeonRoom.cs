using System;
using Microsoft.Xna.Framework;
using Terraria.Utilities;

namespace Terraria.GameContent.Generation.Dungeon.Rooms;

public class BiomeSquareDungeonRoom : BiomeDungeonRoom
{
	public Vector2 Position;

	public int RoomInnerSize;

	public int RoomOuterSize;

	public int WallDepth;

	public BiomeSquareDungeonRoom(DungeonRoomSettings settings)
		: base(settings)
	{
	}

	public override void CalculateRoom(DungeonData data)
	{
		calculated = false;
		int x = settings.RoomPosition.X;
		int y = settings.RoomPosition.Y;
		BiomeRoom(data, x, y, generating: false);
		calculated = true;
	}

	public override bool GenerateRoom(DungeonData data)
	{
		generated = false;
		int x = settings.RoomPosition.X;
		int y = settings.RoomPosition.Y;
		BiomeRoom(data, x, y, generating: true);
		generated = true;
		return true;
	}

	public void BiomeRoom(DungeonData data, int i, int j, bool generating)
	{
		UnifiedRandom genRand = new UnifiedRandom(settings.RandomSeed);
		BiomeDungeonRoomSettings biomeDungeonRoomSettings = (BiomeDungeonRoomSettings)settings;
		ushort brickTileType = settings.StyleData.BrickTileType;
		ushort brickWallType = settings.StyleData.BrickWallType;
		Vector2 position = new Vector2(i, j);
		int num = BiomeDungeonRoom.GetBiomeRoomInnerSize(biomeDungeonRoomSettings.StyleData);
		int num2 = 8;
		int num3 = BiomeDungeonRoom.GetBiomeRoomOuterSize(biomeDungeonRoomSettings.StyleData);
		if (calculated)
		{
			position = Position;
			num = RoomInnerSize;
			num3 = RoomOuterSize;
			num2 = WallDepth;
		}
		int num4 = 20;
		int num5 = Math.Max(num4 + num2, Math.Min(Main.maxTilesX - num4 - num2, (int)position.X - num));
		int num6 = Math.Max(num4 + num2, Math.Min(Main.maxTilesX - num4 - num2, (int)position.X + num));
		int num7 = Math.Max(num4 + num2, Math.Min(Main.maxTilesY - num4 - num2, (int)position.Y - num));
		int num8 = Math.Max(num4 + num2, Math.Min(Main.maxTilesY - num4 - num2, (int)position.Y + num));
		int num9 = Math.Max(num4, Math.Min(Main.maxTilesX - num4, (int)position.X - num3));
		int num10 = Math.Max(num4, Math.Min(Main.maxTilesX - num4, (int)position.X + num3));
		int num11 = Math.Max(num4, Math.Min(Main.maxTilesY - num4, (int)position.Y - num3));
		int num12 = Math.Max(num4, Math.Min(Main.maxTilesY - num4, (int)position.Y + num3));
		InnerBounds.SetBounds(num5, num7, num6, num8);
		OuterBounds.SetBounds(num9, num11, num10, num12);
		data.dungeonBounds.UpdateBounds(num9, num11, num10, num12);
		if (generating)
		{
			_ = OuterBounds.Center;
			for (int k = num9; k < num10; k++)
			{
				int num13 = k;
				for (int l = num11; l < num12; l++)
				{
					bool flag = false;
					bool flag2 = false;
					int num14 = l;
					Tile tile = Main.tile[num13, num14];
					_ = Main.tile[num13, num14 - 1];
					_ = Main.tile[num13, num14 + 1];
					_ = Main.tile[num13, num14 + 2];
					if (tile.type == 484 || tile.type == 485)
					{
						tile.active(active: false);
					}
					if (InnerBounds.Contains(num13, num14))
					{
						if (tile.liquid > 0)
						{
							if (flag2)
							{
								tile.liquid = 0;
							}
							tile.liquidType(0);
						}
						if (flag)
						{
							DungeonUtils.ChangeWallType(tile, 0, resetTile: false, biomeDungeonRoomSettings.OverridePaintWall);
						}
						else
						{
							DungeonUtils.ChangeWallType(tile, brickWallType, resetTile: false, biomeDungeonRoomSettings.OverridePaintWall);
						}
						tile.active(active: false);
					}
					else
					{
						bool num15 = k == num9 || k == num10 - 1 || l == num11 || l == num12 - 1 || k == num5 - 1 || k == num6 || l == num7 - 1 || l == num8;
						if (tile.liquid > 0)
						{
							tile.liquid = 0;
						}
						DungeonUtils.ChangeTileType(tile, brickTileType, resetTile: true, biomeDungeonRoomSettings.OverridePaintTile);
						if (!num15)
						{
							DungeonUtils.ChangeWallType(tile, brickWallType, resetTile: false, biomeDungeonRoomSettings.OverridePaintWall);
						}
					}
				}
			}
			BiomeRoom_FinishRoom(genRand, num9, num10, num11, num12);
		}
		RoomInnerSize = num;
		RoomOuterSize = num3;
		WallDepth = num2;
		Position = position;
		InnerBounds.CalculateHitbox();
		OuterBounds.CalculateHitbox();
	}
}
