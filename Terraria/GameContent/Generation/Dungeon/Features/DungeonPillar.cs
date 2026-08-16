namespace Terraria.GameContent.Generation.Dungeon.Features;

public class DungeonPillar : DungeonFeature
{
	public DungeonPillar(DungeonFeatureSettings settings)
		: base(settings)
	{
		DungeonCrawler.CurrentDungeonData.dungeonFeatures.Add(this);
	}

	public override bool GenerateFeature(DungeonData data, int x, int y)
	{
		generated = false;
		DungeonGenerationStyleData style = ((DungeonPillarSettings)settings).Style;
		if (Pillar(data, x, y, style.BrickTileType, style.BrickWallType, generating: true))
		{
			generated = true;
			return true;
		}
		return false;
	}

	public override bool CanGenerateFeatureAt(DungeonData data, IDungeonFeature feature, int x, int y)
	{
		return true;
	}

	public bool Pillar(DungeonData data, int i, int j, ushort tileType, ushort wallType, bool generating = false)
	{
		_ = WorldGen.genRand;
		DungeonPillarSettings dungeonPillarSettings = (DungeonPillarSettings)settings;
		int width = dungeonPillarSettings.Width;
		int height = dungeonPillarSettings.Height;
		bool crowningOnTop = dungeonPillarSettings.CrowningOnTop;
		bool crowningOnBottom = dungeonPillarSettings.CrowningOnBottom;
		bool crowningStopsAtPillar = dungeonPillarSettings.CrowningStopsAtPillar;
		int num = 3;
		int topY = 0;
		Bounds.SetBounds(i, j, i, j);
		_ = width / 2;
		for (int k = 0; k < width; k++)
		{
			int num2 = i + k - width / 2;
			int topY2 = j;
			int bottomY = j;
			GenerateTileStrip(dungeonPillarSettings, upwards: true, out topY2, out bottomY, num2, j, height, tileType, wallType, smoothTop: false, smoothBottom: false);
			Bounds.UpdateBounds(num2, topY2, num2, bottomY);
			if (crowningOnTop)
			{
				int pillarHeight = (crowningStopsAtPillar ? (num + 1) : 0);
				if (k == 0)
				{
					GenerateTileStrip(dungeonPillarSettings, upwards: true, out topY, out topY, num2 - 1, topY2 + num, pillarHeight, tileType, wallType, smoothTop: false, smoothBottom: true);
				}
				else if (k == width - 1)
				{
					GenerateTileStrip(dungeonPillarSettings, upwards: true, out topY, out topY, num2 + 1, topY2 + num, pillarHeight, tileType, wallType, smoothTop: false, smoothBottom: true);
				}
			}
			if (crowningOnBottom)
			{
				int pillarHeight2 = (crowningStopsAtPillar ? (num + 1) : 0);
				if (k == 0)
				{
					GenerateTileStrip(dungeonPillarSettings, upwards: false, out topY, out topY, num2 - 1, bottomY - num, pillarHeight2, tileType, wallType, smoothTop: true, smoothBottom: false);
				}
				else if (k == width - 1)
				{
					GenerateTileStrip(dungeonPillarSettings, upwards: false, out topY, out topY, num2 + 1, bottomY - num, pillarHeight2, tileType, wallType, smoothTop: true, smoothBottom: false);
				}
			}
		}
		Bounds.CalculateHitbox();
		return true;
	}

	public static void GenerateTileStrip(DungeonPillarSettings pillarSettings, bool upwards, out int topY, out int bottomY, int placeX, int placeY, int pillarHeight, int tileType, int wallType, bool smoothTop, bool smoothBottom)
	{
		PillarType pillarType = pillarSettings.PillarType;
		ushort num = (ushort)((pillarType == PillarType.Wall) ? wallType : tileType);
		bool flag = pillarType == PillarType.Wall;
		int num2 = (flag ? pillarSettings.OverridePaintWall : pillarSettings.OverridePaintTile);
		bool flag2 = pillarType == PillarType.BlockActuatedSolidTop || pillarType == PillarType.BlockActuatedSolidTopAndBottom;
		bool flag3 = pillarType == PillarType.BlockActuatedSolidBottom || pillarType == PillarType.BlockActuatedSolidTopAndBottom;
		bool flag4 = pillarType == PillarType.BlockActuated || pillarType == PillarType.BlockActuatedSolidTop || pillarType == PillarType.BlockActuatedSolidBottom || pillarType == PillarType.BlockActuatedSolidTopAndBottom;
		int num3 = pillarHeight;
		if (num3 == 0)
		{
			num3 = 0;
			int i = 0;
			if (upwards)
			{
				while (i > -100 && WorldGen.InWorld(placeX, placeY + i, 10) && !Main.tile[placeX, placeY + i].active())
				{
					i--;
				}
				num3 = -i;
			}
			else
			{
				for (; i < 100 && WorldGen.InWorld(placeX, placeY + i, 10) && !Main.tile[placeX, placeY + i].active(); i++)
				{
				}
				num3 = i;
				placeY += num3 - 1;
			}
		}
		topY = placeY;
		bottomY = placeY;
		if (num3 == 0)
		{
			return;
		}
		int num4 = -num3 + 1;
		int num5 = 0;
		if (upwards)
		{
			for (int j = num4; j <= num5; j++)
			{
				int num6 = placeY + j;
				if (num6 <= 10)
				{
					break;
				}
				Tile tile = Main.tile[placeX, num6];
				if (!pillarSettings.AlwaysPlaceEntirePillar && tile.active())
				{
					break;
				}
				if (flag)
				{
					tile.wall = num;
					if (num2 >= 0)
					{
						tile.wallColor((byte)num2);
					}
				}
				else
				{
					tile.ClearTile();
					tile.active(active: true);
					tile.type = num;
					if (num2 >= 0)
					{
						tile.color((byte)num2);
					}
					if ((j == num4 && smoothTop) || (j == num5 && smoothBottom))
					{
						Tile.SmoothSlope(placeX, num6, applyToNeighbors: false);
					}
					if ((!flag2 || j >= num4 + 2) && (!flag3 || j <= num5 - 2) && flag4)
					{
						tile.inActive(inActive: true);
					}
				}
				if (num6 < topY)
				{
					topY = num6;
				}
				if (num6 > bottomY)
				{
					bottomY = num6;
				}
			}
			return;
		}
		for (int num7 = num5; num7 >= num4; num7--)
		{
			int num8 = placeY + num7;
			if (num8 >= Main.maxTilesY - 10)
			{
				break;
			}
			Tile tile2 = Main.tile[placeX, num8];
			if (!pillarSettings.AlwaysPlaceEntirePillar && tile2.active())
			{
				break;
			}
			if (flag)
			{
				tile2.wall = num;
				if (num2 >= 0)
				{
					tile2.wallColor((byte)num2);
				}
			}
			else
			{
				tile2.ClearTile();
				tile2.active(active: true);
				tile2.type = num;
				if (num2 >= 0)
				{
					tile2.color((byte)num2);
				}
				if ((num7 == num4 && smoothTop) || (num7 == num5 && smoothBottom))
				{
					Tile.SmoothSlope(placeX, num8, applyToNeighbors: false);
				}
				if ((!flag2 || num7 >= num4 + 2) && (!flag3 || num7 <= num5 - 2) && flag4)
				{
					tile2.inActive(inActive: true);
				}
			}
			if (num8 < topY)
			{
				topY = num8;
			}
			if (num8 > bottomY)
			{
				bottomY = num8;
			}
		}
	}
}
