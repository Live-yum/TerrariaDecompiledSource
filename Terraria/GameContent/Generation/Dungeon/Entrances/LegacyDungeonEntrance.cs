using System;
using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria.DataStructures;
using Terraria.Utilities;

namespace Terraria.GameContent.Generation.Dungeon.Entrances;

public class LegacyDungeonEntrance : DungeonEntrance
{
	public LegacyDungeonEntrance(DungeonEntranceSettings settings)
		: base(settings)
	{
	}

	public override void CalculateEntrance(DungeonData data, int x, int y)
	{
		calculated = false;
		LegacyEntrance(data, x, y, generating: false);
		calculated = true;
	}

	public override bool GenerateEntrance(DungeonData data, int x, int y)
	{
		generated = false;
		LegacyEntrance(data, x, y, generating: true);
		generated = true;
		return true;
	}

	public void LegacyEntrance(DungeonData data, int i, int j, bool generating)
	{
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_080d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0836: Unknown result type (might be due to invalid IL or missing references)
		//IL_085f: Unknown result type (might be due to invalid IL or missing references)
		//IL_08f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_090d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a90: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ac3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0af6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b29: Unknown result type (might be due to invalid IL or missing references)
		//IL_0988: Unknown result type (might be due to invalid IL or missing references)
		//IL_0de3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e0c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bd5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bb7: Unknown result type (might be due to invalid IL or missing references)
		//IL_127c: Unknown result type (might be due to invalid IL or missing references)
		//IL_12a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_12ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_12f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_1175: Unknown result type (might be due to invalid IL or missing references)
		//IL_119e: Unknown result type (might be due to invalid IL or missing references)
		//IL_11c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_11f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_13cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_145d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1474: Unknown result type (might be due to invalid IL or missing references)
		//IL_169d: Unknown result type (might be due to invalid IL or missing references)
		//IL_16c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_16ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_1718: Unknown result type (might be due to invalid IL or missing references)
		//IL_1765: Unknown result type (might be due to invalid IL or missing references)
		//IL_1767: Unknown result type (might be due to invalid IL or missing references)
		//IL_1549: Unknown result type (might be due to invalid IL or missing references)
		//IL_1560: Unknown result type (might be due to invalid IL or missing references)
		//IL_17f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_1801: Unknown result type (might be due to invalid IL or missing references)
		//IL_17a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_17ba: Unknown result type (might be due to invalid IL or missing references)
		UnifiedRandom unifiedRandom = new UnifiedRandom(((LegacyDungeonEntranceSettings)settings).RandomSeed);
		ushort brickTileType = settings.StyleData.BrickTileType;
		ushort brickWallType = settings.StyleData.BrickWallType;
		bool dungeonEntranceIsBuried = SpecialSeedFeatures.DungeonEntranceIsBuried;
		bool dungeonEntranceIsUnderground = SpecialSeedFeatures.DungeonEntranceIsUnderground;
		if (generating)
		{
			int num = 60;
			for (int k = i - num; k < i + num; k++)
			{
				for (int l = j - num; l < j + num; l++)
				{
					if (WorldGen.InWorld(k, l))
					{
						Main.tile[k, l].liquid = 0;
						Main.tile[k, l].lava(lava: false);
						Main.tile[k, l].Clear(TileDataType.Slope);
					}
				}
			}
		}
		Vector2D zero = Vector2D.Zero;
		double dungeonEntranceStrengthX = data.dungeonEntranceStrengthX;
		double dungeonEntranceStrengthY = data.dungeonEntranceStrengthY;
		zero.X = i;
		zero.Y = (double)j - dungeonEntranceStrengthY / 2.0;
		data.dungeonBounds.Top = (int)zero.Y;
		int num2 = 1;
		if (i > Main.maxTilesX / 2)
		{
			num2 = -1;
		}
		if (WorldGen.drunkWorldGen || WorldGen.getGoodWorldGen)
		{
			num2 *= -1;
		}
		Bounds.SetBounds((int)zero.X, (int)zero.Y, (int)zero.X, (int)zero.Y);
		int num3 = Math.Max(0, Math.Min(Main.maxTilesX - 1, (int)(zero.X - dungeonEntranceStrengthX * 0.6000000238418579 - (double)unifiedRandom.Next(2, 5))));
		int num4 = Math.Max(0, Math.Min(Main.maxTilesX - 1, (int)(zero.X + dungeonEntranceStrengthX * 0.6000000238418579 + (double)unifiedRandom.Next(2, 5))));
		int num5 = Math.Max(0, Math.Min(Main.maxTilesY - 1, (int)(zero.Y - dungeonEntranceStrengthY * 0.6000000238418579 - (double)unifiedRandom.Next(2, 5))));
		int num6 = Math.Max(0, Math.Min(Main.maxTilesY - 1, (int)(zero.Y + dungeonEntranceStrengthY * 0.6000000238418579 + (double)unifiedRandom.Next(8, 16))));
		Bounds.UpdateBounds(num3, num5, num4, num6);
		if (generating)
		{
			for (int m = num3; m < num4; m++)
			{
				for (int n = num5; n < num6; n++)
				{
					Main.tile[m, n].liquid = 0;
					if (Main.tile[m, n].wall != brickWallType)
					{
						Main.tile[m, n].wall = 0;
						if (m > num3 + 1 && m < num4 - 2 && n > num5 + 1 && n < num6 - 2)
						{
							Main.tile[m, n].wall = brickWallType;
						}
						Main.tile[m, n].active(active: true);
						Main.tile[m, n].type = brickTileType;
						Main.tile[m, n].Clear(TileDataType.Slope);
					}
				}
			}
		}
		int num7 = Math.Max(0, Math.Min(Main.maxTilesX - 1, num3));
		int num8 = Math.Max(0, Math.Min(Main.maxTilesX - 1, num3 + 5 + unifiedRandom.Next(4)));
		int num9 = Math.Max(0, Math.Min(Main.maxTilesY - 1, num5 - 3 - unifiedRandom.Next(3)));
		int num10 = Math.Max(0, Math.Min(Main.maxTilesY - 1, num5));
		Bounds.UpdateBounds(num7, num9, num8, num10);
		if (generating)
		{
			for (int num11 = num7; num11 < num8; num11++)
			{
				for (int num12 = num9; num12 < num10; num12++)
				{
					Main.tile[num11, num12].liquid = 0;
					if (Main.tile[num11, num12].wall != brickWallType)
					{
						Main.tile[num11, num12].active(active: true);
						Main.tile[num11, num12].type = brickTileType;
						Main.tile[num11, num12].Clear(TileDataType.Slope);
					}
				}
			}
		}
		num7 = Math.Max(0, Math.Min(Main.maxTilesX - 1, num4 - 5 - unifiedRandom.Next(4)));
		num8 = Math.Max(0, Math.Min(Main.maxTilesX - 1, num4));
		num9 = Math.Max(0, Math.Min(Main.maxTilesY - 1, num5 - 3 - unifiedRandom.Next(3)));
		num10 = Math.Max(0, Math.Min(Main.maxTilesY - 1, num5));
		Bounds.UpdateBounds(num7, num9, num8, num10);
		if (generating)
		{
			for (int num13 = num7; num13 < num8; num13++)
			{
				for (int num14 = num9; num14 < num10; num14++)
				{
					Main.tile[num13, num14].liquid = 0;
					if (Main.tile[num13, num14].wall != brickWallType)
					{
						Main.tile[num13, num14].active(active: true);
						Main.tile[num13, num14].type = brickTileType;
						Main.tile[num13, num14].Clear(TileDataType.Slope);
					}
				}
			}
		}
		int num15 = 2 + unifiedRandom.Next(4);
		int num16 = 1 + unifiedRandom.Next(2);
		int num17 = 0;
		int num18 = Math.Max(0, Math.Min(Main.maxTilesY - 1, num5 - num16));
		data.dungeonBounds.UpdateBounds(num3, num18, num4, num5);
		if (generating)
		{
			for (int num19 = num3; num19 < num4; num19++)
			{
				for (int num20 = num18; num20 < num5; num20++)
				{
					Bounds.UpdateBounds(num19, num20);
					Main.tile[num19, num20].liquid = 0;
					if (Main.tile[num19, num20].wall != brickWallType)
					{
						Main.tile[num19, num20].active(active: true);
						Main.tile[num19, num20].type = brickTileType;
						Main.tile[num19, num20].Clear(TileDataType.Slope);
					}
				}
				num17++;
				if (num17 >= num15)
				{
					num19 += num15;
					num17 = 0;
				}
			}
		}
		if (generating)
		{
			double num21 = Main.worldSurface;
			if (data.Type == DungeonType.DualDungeon)
			{
				num21 = DungeonUtils.GetDualDungeonBrickSupportCutoffY(data);
			}
			for (int num22 = num3; num22 < num4; num22++)
			{
				for (int num23 = num5; (double)num23 < num21; num23++)
				{
					Main.tile[num22, num23].liquid = 0;
					if (DungeonUtils.InAnyPotentialDungeonBounds(num22, num23 - 5))
					{
						continue;
					}
					Tile tile = Main.tile[num22, num23];
					bool flag = tile.active() && !settings.StyleData.TileIsInStyle(tile.type);
					bool flag2 = !settings.StyleData.WallIsInStyle(tile.wall);
					bool flag3 = DungeonUtils.IsConsideredDungeonWall(tile.wall);
					if ((tile.active() && flag) || !flag3)
					{
						Main.tile[num22, num23].active(active: true);
						Main.tile[num22, num23].type = brickTileType;
						if (num22 > num3 && num22 < num4 - 1)
						{
							Main.tile[num22, num23].wall = brickWallType;
						}
						Main.tile[num22, num23].Clear(TileDataType.Slope);
					}
					else if (flag2 && num22 > num3 && num22 < num4 - 1)
					{
						Main.tile[num22, num23].wall = brickWallType;
					}
				}
			}
		}
		num3 = Math.Max(0, Math.Min(Main.maxTilesX - 1, (int)(zero.X - dungeonEntranceStrengthX * 0.5)));
		num4 = Math.Max(0, Math.Min(Main.maxTilesX - 1, (int)(zero.X + dungeonEntranceStrengthX * 0.5)));
		num5 = Math.Max(0, Math.Min(Main.maxTilesY - 1, (int)(zero.Y - dungeonEntranceStrengthY * 0.5)));
		num6 = Math.Max(0, Math.Min(Main.maxTilesY - 1, (int)(zero.Y + dungeonEntranceStrengthY * 0.5)));
		Bounds.UpdateBounds(num3, num5, num4, num6);
		if (generating)
		{
			for (int num24 = num3; num24 < num4; num24++)
			{
				for (int num25 = num5; num25 < num6; num25++)
				{
					Main.tile[num24, num25].liquid = 0;
					Main.tile[num24, num25].active(active: false);
					Main.tile[num24, num25].wall = brickWallType;
				}
			}
		}
		int num26 = (int)zero.X;
		int num27 = num6;
		for (int num28 = 0; num28 < 20; num28++)
		{
			num26 = (int)zero.X - num28;
			if (num26 <= 0)
			{
				break;
			}
			if (!Main.tile[num26, num27].active() && Main.wallDungeon[Main.tile[num26, num27].wall])
			{
				DungeonPlatformData item = new DungeonPlatformData
				{
					Position = new Point(num26, num27),
					InAHallway = false
				};
				data.dungeonPlatformData.Add(item);
				break;
			}
			num26 = (int)zero.X + num28;
			if (num26 >= Main.maxTilesX)
			{
				break;
			}
			if (!Main.tile[num26, num27].active() && Main.wallDungeon[Main.tile[num26, num27].wall])
			{
				DungeonPlatformData item2 = new DungeonPlatformData
				{
					Position = new Point(num26, num27),
					InAHallway = false
				};
				data.dungeonPlatformData.Add(item2);
				break;
			}
		}
		zero.X += dungeonEntranceStrengthX * 0.6000000238418579 * (double)num2;
		zero.Y += dungeonEntranceStrengthY * 0.5;
		dungeonEntranceStrengthX = data.dungeonEntranceStrengthX2;
		dungeonEntranceStrengthY = data.dungeonEntranceStrengthY2;
		zero.X += dungeonEntranceStrengthX * 0.550000011920929 * (double)num2;
		zero.Y -= dungeonEntranceStrengthY * 0.5;
		num3 = Math.Max(0, Math.Min(Main.maxTilesX - 1, (int)(zero.X - dungeonEntranceStrengthX * 0.6000000238418579 - (double)unifiedRandom.Next(1, 3))));
		num4 = Math.Max(0, Math.Min(Main.maxTilesX - 1, (int)(zero.X + dungeonEntranceStrengthX * 0.6000000238418579 + (double)unifiedRandom.Next(1, 3))));
		num5 = Math.Max(0, Math.Min(Main.maxTilesY - 1, (int)(zero.Y - dungeonEntranceStrengthY * 0.6000000238418579 - (double)unifiedRandom.Next(1, 3))));
		num6 = Math.Max(0, Math.Min(Main.maxTilesY - 1, (int)(zero.Y + dungeonEntranceStrengthY * 0.6000000238418579 + (double)unifiedRandom.Next(6, 16))));
		Bounds.UpdateBounds(num3, num5, num4, num6);
		if (generating)
		{
			for (int num29 = num3; num29 < num4; num29++)
			{
				for (int num30 = num5; num30 < num6; num30++)
				{
					Tile tile2 = Main.tile[num29, num30];
					if (tile2.active() && tile2.type == brickTileType)
					{
						continue;
					}
					tile2.liquid = 0;
					bool flag4 = true;
					if (num2 < 0)
					{
						if ((double)num29 < zero.X - dungeonEntranceStrengthX * 0.5)
						{
							flag4 = false;
						}
					}
					else if ((double)num29 > zero.X + dungeonEntranceStrengthX * 0.5 - 1.0)
					{
						flag4 = false;
					}
					if (flag4)
					{
						tile2.wall = 0;
						tile2.active(active: true);
						tile2.type = brickTileType;
						tile2.Clear(TileDataType.Slope);
					}
				}
			}
		}
		Bounds.UpdateBounds(num3, num5, num4, (int)Main.worldSurface);
		if (generating)
		{
			double num31 = Main.worldSurface;
			if (data.Type == DungeonType.DualDungeon)
			{
				num31 = DungeonCrawler.CurrentDungeonData.genVars.outerPotentialDungeonBounds.Top - 5;
			}
			for (int num32 = num3; num32 < num4; num32++)
			{
				for (int num33 = num6; (double)num33 < num31; num33++)
				{
					Main.tile[num32, num33].liquid = 0;
					if (DungeonUtils.InAnyPotentialDungeonBounds(num32, num33 - 5))
					{
						continue;
					}
					Tile tile3 = Main.tile[num32, num33];
					bool flag5 = tile3.active() && !settings.StyleData.TileIsInStyle(tile3.type);
					bool flag6 = !settings.StyleData.WallIsInStyle(tile3.wall);
					bool flag7 = DungeonUtils.IsConsideredDungeonWall(tile3.wall);
					if ((tile3.active() && flag5) || !flag7)
					{
						Main.tile[num32, num33].active(active: true);
						Main.tile[num32, num33].type = brickTileType;
						if (num32 > num3 && num32 < num4 - 1)
						{
							Main.tile[num32, num33].wall = brickWallType;
						}
						Main.tile[num32, num33].Clear(TileDataType.Slope);
					}
					else if (flag6 && num32 > num3 && num32 < num4 - 1)
					{
						Main.tile[num32, num33].wall = brickWallType;
					}
				}
			}
		}
		num3 = Math.Max(0, Math.Min(Main.maxTilesX - 1, (int)(zero.X - dungeonEntranceStrengthX * 0.5)));
		num4 = Math.Max(0, Math.Min(Main.maxTilesX - 1, (int)(zero.X + dungeonEntranceStrengthX * 0.5)));
		num7 = num3;
		if (num2 < 0)
		{
			Math.Max(0, Math.Min(Main.maxTilesX - 1, num7++));
		}
		num8 = Math.Max(0, Math.Min(Main.maxTilesX - 1, num7 + 5 + unifiedRandom.Next(4)));
		num9 = Math.Max(0, Math.Min(Main.maxTilesY - 1, num5 - 3 - unifiedRandom.Next(3)));
		num10 = Math.Max(0, Math.Min(Main.maxTilesY - 1, num5));
		Bounds.UpdateBounds(num7, num9, num8, num10);
		if (generating)
		{
			for (int num34 = num7; num34 < num8; num34++)
			{
				for (int num35 = num9; num35 < num10; num35++)
				{
					Main.tile[num34, num35].liquid = 0;
					if (Main.tile[num34, num35].wall != brickWallType)
					{
						Main.tile[num34, num35].active(active: true);
						Main.tile[num34, num35].type = brickTileType;
						Main.tile[num34, num35].Clear(TileDataType.Slope);
					}
				}
			}
		}
		num7 = Math.Max(0, Math.Min(Main.maxTilesX - 1, num4 - 5 - unifiedRandom.Next(4)));
		num8 = Math.Max(0, Math.Min(Main.maxTilesX - 1, num4));
		num9 = Math.Max(0, Math.Min(Main.maxTilesY - 1, num5 - 3 - unifiedRandom.Next(3)));
		num10 = Math.Max(0, Math.Min(Main.maxTilesY - 1, num5));
		Bounds.UpdateBounds(num7, num9, num8, num10);
		if (generating)
		{
			for (int num36 = num7; num36 < num8; num36++)
			{
				for (int num37 = num9; num37 < num10; num37++)
				{
					Main.tile[num36, num37].liquid = 0;
					if (Main.tile[num36, num37].wall != brickWallType)
					{
						Main.tile[num36, num37].active(active: true);
						Main.tile[num36, num37].type = brickTileType;
						Main.tile[num36, num37].Clear(TileDataType.Slope);
					}
				}
			}
		}
		if (num2 < 0)
		{
			num4++;
		}
		num16 = 1 + unifiedRandom.Next(2);
		num15 = 2 + unifiedRandom.Next(4);
		num17 = 0;
		num18 = Math.Max(0, Math.Min(Main.maxTilesY - 1, num5 - num16));
		if (generating)
		{
			for (int num38 = num3 + 1; num38 < num4 - 1; num38++)
			{
				for (int num39 = num18; num39 < num5; num39++)
				{
					Main.tile[num38, num39].liquid = 0;
					if (Main.tile[num38, num39].wall != brickWallType)
					{
						Main.tile[num38, num39].active(active: true);
						Main.tile[num38, num39].type = brickTileType;
						Main.tile[num38, num39].Clear(TileDataType.Slope);
					}
				}
				num17++;
				if (num17 >= num15)
				{
					num38 += num15;
					num17 = 0;
				}
			}
		}
		if (!dungeonEntranceIsUnderground && !dungeonEntranceIsBuried)
		{
			num3 = Math.Max(0, Math.Min(Main.maxTilesX - 1, (int)(zero.X - dungeonEntranceStrengthX * 0.6)));
			num4 = Math.Max(0, Math.Min(Main.maxTilesX - 1, (int)(zero.X + dungeonEntranceStrengthX * 0.6)));
			num5 = Math.Max(0, Math.Min(Main.maxTilesX - 1, (int)(zero.Y - dungeonEntranceStrengthY * 0.6)));
			num6 = Math.Max(0, Math.Min(Main.maxTilesX - 1, (int)(zero.Y + dungeonEntranceStrengthY * 0.6)));
			Bounds.UpdateBounds(num3, num5, num4, num6);
			if (generating)
			{
				for (int num40 = num3; num40 < num4; num40++)
				{
					for (int num41 = num5; num41 < num6; num41++)
					{
						Main.tile[num40, num41].liquid = 0;
						Main.tile[num40, num41].wall = 0;
					}
				}
			}
		}
		num3 = Math.Max(0, Math.Min(Main.maxTilesX - 1, (int)(zero.X - dungeonEntranceStrengthX * 0.5)));
		num4 = Math.Max(0, Math.Min(Main.maxTilesX - 1, (int)(zero.X + dungeonEntranceStrengthX * 0.5)));
		num5 = Math.Max(0, Math.Min(Main.maxTilesY - 1, (int)(zero.Y - dungeonEntranceStrengthY * 0.5)));
		num6 = Math.Max(0, Math.Min(Main.maxTilesY - 1, (int)(zero.Y + dungeonEntranceStrengthY * 0.5)));
		if ((dungeonEntranceIsUnderground || dungeonEntranceIsBuried) && num2 == -1)
		{
			num3 = Math.Max(0, Math.Min(Main.maxTilesX - 1, num3 + 1));
			num4 = Math.Max(0, Math.Min(Main.maxTilesX - 1, num4 + 1));
		}
		Bounds.UpdateBounds(num3, num5, num4, num6);
		if (generating)
		{
			for (int num42 = num3; num42 < num4; num42++)
			{
				for (int num43 = num5; num43 < num6; num43++)
				{
					Main.tile[num42, num43].liquid = 0;
					Main.tile[num42, num43].active(active: false);
					Main.tile[num42, num43].wall = 0;
				}
			}
		}
		OldManSpawn = DungeonUtils.SetOldManSpawnAndSpawnOldManIfDefaultDungeon((int)zero.X, num6, generating);
		if (generating && SpecialSeedFeatures.DungeonEntranceHasATree)
		{
			DungeonUtils.GenerateDungeonTree(data, data.genVars.generatingDungeonPositionX, (int)Main.worldSurface, data.genVars.generatingDungeonPositionY);
		}
		if (generating && SpecialSeedFeatures.DungeonEntranceHasStairs)
		{
			int i2 = ((num2 == 1) ? num4 : num3);
			int depth = DungeonUtils.GetDualDungeonBrickSupportCutoffY(data) - num6 + 5;
			DungeonUtils.GenerateDungeonStairs(data, i2, num6, num2, brickTileType, brickWallType, depth);
		}
		num16 = 1 + unifiedRandom.Next(2);
		num15 = 2 + unifiedRandom.Next(4);
		num17 = 0;
		num3 = (int)(zero.X - dungeonEntranceStrengthX * 0.5);
		num4 = (int)(zero.X + dungeonEntranceStrengthX * 0.5);
		if (dungeonEntranceIsUnderground || dungeonEntranceIsBuried)
		{
			if (num2 == -1)
			{
				num3++;
				num4++;
			}
		}
		else
		{
			num3 += 2;
			num4 -= 2;
		}
		num3 = Math.Max(0, Math.Min(Main.maxTilesX - 1, num3));
		num4 = Math.Max(0, Math.Min(Main.maxTilesX - 1, num4));
		if (generating)
		{
			for (int num44 = num3; num44 < num4; num44++)
			{
				for (int num45 = num5; num45 < num6 + 1; num45++)
				{
					WorldGen.PlaceWall(num44, num45, brickWallType, mute: true);
				}
				if (!dungeonEntranceIsUnderground && !dungeonEntranceIsBuried)
				{
					num17++;
					if (num17 >= num15)
					{
						num44 += num15 * 2;
						num17 = 0;
					}
				}
			}
		}
		if (WorldGen.drunkWorldGen && !WorldGen.SecretSeed.noSurface.Enabled)
		{
			num3 = (int)(zero.X - dungeonEntranceStrengthX * 0.5);
			num4 = (int)(zero.X + dungeonEntranceStrengthX * 0.5);
			if (num2 == 1)
			{
				num3 = num4 - 3;
			}
			else
			{
				num4 = num3 + 3;
			}
			num3 = Math.Max(0, Math.Min(Main.maxTilesX - 1, num3));
			num4 = Math.Max(0, Math.Min(Main.maxTilesX - 1, num4));
			Bounds.UpdateBounds(num3, num5, num4, num6);
			if (generating)
			{
				for (int num46 = num3; num46 < num4; num46++)
				{
					for (int num47 = num5; num47 < num6 + 1; num47++)
					{
						Main.tile[num46, num47].active(active: true);
						Main.tile[num46, num47].type = brickTileType;
						Main.tile[num46, num47].Clear(TileDataType.Slope);
					}
				}
			}
		}
		zero.X -= dungeonEntranceStrengthX * 0.6000000238418579 * (double)num2;
		zero.Y += dungeonEntranceStrengthY * 0.5;
		dungeonEntranceStrengthX = 15.0;
		dungeonEntranceStrengthY = 3.0;
		zero.Y -= dungeonEntranceStrengthY * 0.5;
		num3 = Math.Max(0, Math.Min(Main.maxTilesX - 1, (int)(zero.X - dungeonEntranceStrengthX * 0.5)));
		num4 = Math.Max(0, Math.Min(Main.maxTilesX - 1, (int)(zero.X + dungeonEntranceStrengthX * 0.5)));
		num5 = Math.Max(0, Math.Min(Main.maxTilesY - 1, (int)(zero.Y - dungeonEntranceStrengthY * 0.5)));
		num6 = Math.Max(0, Math.Min(Main.maxTilesY - 1, (int)(zero.Y + dungeonEntranceStrengthY * 0.5)));
		Bounds.UpdateBounds(num3, num5, num4, num6);
		if (num2 < 0)
		{
			zero.X -= 1.0;
		}
		Vector2D val = zero;
		val.Y += 1.0;
		if (generating)
		{
			for (int num48 = num3; num48 < num4; num48++)
			{
				for (int num49 = num5; num49 < num6; num49++)
				{
					Main.tile[num48, num49].active(active: false);
					if ((num2 > 0 && (double)num48 < val.X) || (num2 < 0 && (double)num48 > val.X) || dungeonEntranceIsUnderground || dungeonEntranceIsBuried)
					{
						Main.tile[num48, num49].wall = brickWallType;
					}
				}
			}
		}
		if (generating)
		{
			WorldGen.PlaceTile((int)val.X, (int)val.Y, 10, mute: true, forced: false, -1, 13);
		}
		Bounds.CalculateHitbox();
	}
}
