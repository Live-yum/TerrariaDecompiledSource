using Terraria.GameContent.Generation.Dungeon;
using Terraria.ID;

namespace Terraria;

public struct NPCSpawningFlagsForDualDungeons
{
	public bool ZoneDungeon;

	public bool ZoneSnow;

	public bool ZoneGlowshroom;

	public bool ZoneCorrupt;

	public bool ZoneCrimson;

	public bool ZoneJungle;

	public bool ZoneHallow;

	public bool ZoneLihzhardTemple;

	public bool ZoneUndergroundDesert;

	public bool CanScan(Tile tile)
	{
		ushort type = tile.type;
		if (!tile.active() || !Main.tileSolid[type] || Main.tileSolidTop[type])
		{
			return false;
		}
		if (TileID.Sets.Boulders[type])
		{
			return false;
		}
		if (type == 48 || type == 137 || type == 232)
		{
			return false;
		}
		return true;
	}

	public bool ScanZonesFor(bool scanOnly, int spawnTileX, int spawnTileY, int spawnTileType, int spawnWallType, bool npcSpawnPointIsInDualDungeon)
	{
		if (!npcSpawnPointIsInDualDungeon)
		{
			return false;
		}
		bool result = false;
		switch (spawnTileType)
		{
		case 109:
		case 110:
		case 113:
		case 116:
		case 117:
		case 118:
		case 164:
		case 385:
		case 402:
		case 403:
		case 492:
			if (!scanOnly)
			{
				ZoneHallow = true;
			}
			result = true;
			break;
		case 147:
		case 148:
		case 161:
		case 162:
		case 206:
		case 224:
			if (!scanOnly)
			{
				ZoneSnow = true;
			}
			result = true;
			break;
		case 60:
		case 61:
		case 62:
		case 74:
		case 225:
		case 383:
		case 384:
			if (!scanOnly)
			{
				ZoneJungle = true;
			}
			result = true;
			break;
		case 59:
		case 120:
			if (spawnWallType > 0 && WallID.Sets.DualDungeonsJungleBiomeWalls[spawnWallType])
			{
				if (!scanOnly)
				{
					ZoneJungle = true;
				}
				result = true;
			}
			break;
		case 1:
		case 38:
			if (spawnWallType > 0 && DungeonGenerationStyles.Cavern.WallIsInStyle(spawnWallType))
			{
				result = true;
			}
			break;
		case 191:
			result = true;
			break;
		case 22:
		case 23:
		case 24:
		case 25:
		case 32:
		case 112:
		case 140:
		case 398:
		case 400:
		case 474:
		case 661:
			if (!scanOnly)
			{
				ZoneCorrupt = true;
			}
			result = true;
			break;
		case 163:
			if (!scanOnly)
			{
				ZoneSnow = (ZoneCorrupt = true);
			}
			result = true;
			break;
		case 200:
			if (!scanOnly)
			{
				ZoneSnow = (ZoneCrimson = true);
			}
			result = true;
			break;
		case 195:
		case 199:
		case 201:
		case 203:
		case 204:
		case 234:
		case 347:
		case 352:
		case 399:
		case 401:
		case 662:
			if (!scanOnly)
			{
				ZoneCrimson = true;
			}
			result = true;
			break;
		case 41:
		case 43:
		case 44:
		case 481:
		case 482:
		case 483:
			if (!scanOnly && (double)spawnTileY > Main.rockLayer)
			{
				ZoneDungeon = true;
			}
			result = true;
			break;
		case 226:
			if (!scanOnly)
			{
				ZoneLihzhardTemple = true;
			}
			result = true;
			break;
		case 70:
		case 71:
		case 72:
		case 528:
			if (!scanOnly)
			{
				ZoneGlowshroom = true;
			}
			result = true;
			break;
		}
		if (spawnTileType == 123 && spawnWallType > 0)
		{
			if (spawnWallType > 0 && DungeonGenerationStyles.Cavern.WallIsInStyle(spawnWallType))
			{
				result = true;
			}
			else if (WallID.Sets.DualDungeonsJungleBiomeWalls[spawnWallType])
			{
				if (!scanOnly)
				{
					ZoneJungle = true;
				}
				result = true;
			}
			else if (spawnWallType == DungeonGenerationStyles.Temple.BrickWallType)
			{
				if (!scanOnly)
				{
					ZoneLihzhardTemple = true;
				}
				result = true;
			}
		}
		switch (spawnTileType)
		{
		case 53:
		case 112:
		case 116:
		case 234:
		case 396:
		case 397:
		case 398:
		case 399:
		case 400:
		case 401:
		case 402:
		case 403:
			if (WallID.Sets.Conversion.HardenedSand[spawnWallType] || WallID.Sets.Conversion.Sandstone[spawnWallType] || spawnWallType == 223)
			{
				if (!scanOnly)
				{
					ZoneUndergroundDesert = true;
				}
				result = true;
			}
			break;
		}
		return result;
	}
}
