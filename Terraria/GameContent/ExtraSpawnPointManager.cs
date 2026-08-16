using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.WorldBuilding;

namespace Terraria.GameContent;

public class ExtraSpawnPointManager
{
	public static Point[] extraSpawnPoints = new Point[0];

	public static ExtraSpawnSettings settings = default(ExtraSpawnSettings);

	private static List<LandmassData> _listOfLandmasses = new List<LandmassData>();

	public static bool TryGetExtraSpawnPointForTeam(int team, out Point spawnPoint)
	{
		spawnPoint = Point.Zero;
		if (!Main.teamBasedSpawnsSeed)
		{
			return false;
		}
		if (team < 0 || team >= extraSpawnPoints.Length)
		{
			return false;
		}
		try
		{
			spawnPoint = extraSpawnPoints[team];
		}
		catch (IndexOutOfRangeException)
		{
			return false;
		}
		return true;
	}

	public static void GenerateExtraSpawns_Setup()
	{
		if (settings.skyblock)
		{
			_listOfLandmasses.Clear();
			for (int i = 0; i < GenVars.landmassData.Count; i++)
			{
				LandmassData item = GenVars.landmassData[i];
				if (item.DataType == LandmassDataType.SkyblockIsland && item.Style == 0 && !(item.Position.Distance(new Vector2(Main.spawnTileX, Main.spawnTileY)) < 300f))
				{
					_listOfLandmasses.Add(item);
				}
			}
		}
		else if (settings.roundLandmass)
		{
			_listOfLandmasses.Clear();
			for (int j = 0; j < GenVars.landmassData.Count; j++)
			{
				LandmassData item2 = GenVars.landmassData[j];
				if (item2.DataType == LandmassDataType.RoundLandmass && !(item2.Position.Distance(new Vector2(Main.spawnTileX, Main.spawnTileY)) < 300f))
				{
					_listOfLandmasses.Add(item2);
				}
			}
		}
		else
		{
			if (!settings.extraLiquid)
			{
				return;
			}
			_listOfLandmasses.Clear();
			for (int k = 0; k < GenVars.landmassData.Count; k++)
			{
				LandmassData item3 = GenVars.landmassData[k];
				if (item3.DataType == LandmassDataType.ExtraLiquidBubbleSquare && !(item3.Position.Distance(new Vector2(Main.spawnTileX, Main.spawnTileY)) < 300f))
				{
					_listOfLandmasses.Add(item3);
				}
			}
		}
	}

	public static void ResetExtraSpawns()
	{
		_listOfLandmasses.Clear();
		extraSpawnPoints = new Point[0];
		settings = default(ExtraSpawnSettings);
	}

	public static void GenerateExtraSpawns()
	{
		GenerateExtraSpawns_Setup();
		ExtraSpawnType spawnType = settings.spawnType;
		if (spawnType == ExtraSpawnType.None || spawnType != ExtraSpawnType.TeamBased)
		{
			extraSpawnPoints = new Point[0];
			return;
		}
		extraSpawnPoints = new Point[PlayerTeamID.Count];
		extraSpawnPoints[0] = new Point(Main.spawnTileX, Main.spawnTileY);
		List<Point> list = new List<Point>();
		for (int i = 1; i < PlayerTeamID.Count; i++)
		{
			GenerateExtraSpawns_TryFindSpawnRandomly(list, GenerateExtraSpawns_GetFallbackSpawn(i, PlayerTeamID.Count));
		}
		for (int j = 1; j < PlayerTeamID.Count; j++)
		{
			Point point = list[WorldGen.genRand.Next(list.Count)];
			extraSpawnPoints[j] = point;
			list.Remove(point);
		}
	}

	private static bool GenerateExtraSpawns_TryFindSpawnRandomly(List<Point> spawnPoints, Point fallbackSpawn)
	{
		int num = 500;
		int num2 = 60;
		int num3 = 60;
		bool flag = true;
		LandmassData item = default(LandmassData);
		for (int i = 0; i < num; i++)
		{
			int num4 = 0;
			int spawnY = 0;
			int num5 = (int)Main.worldSurface;
			num4 = ((!flag) ? WorldGen.genRand.Next(Main.maxTilesX / 2, Main.maxTilesX - num2) : WorldGen.genRand.Next(num2, Main.maxTilesX / 2));
			if (!settings.surface)
			{
				num5 = Main.UnderworldLayer - 50 - num3;
				spawnY = WorldGen.genRand.Next((int)Main.worldSurface + 200 + num3, num5);
			}
			if (settings.remix)
			{
				num5 = GenVars.remixMushroomLayerHigh - num3;
				spawnY = GenVars.remixSurfaceLayerLow + 50 + num3;
			}
			if (settings.skyblock)
			{
				LandmassData landmassData = default(LandmassData);
				int num6 = 500;
				while (num6 > 0)
				{
					num6--;
					if (_listOfLandmasses.Count <= 0)
					{
						break;
					}
					landmassData = _listOfLandmasses[WorldGen.genRand.Next(_listOfLandmasses.Count)];
					if ((!settings.surface || !((double)landmassData.Position.Y > Main.worldSurface)) && (!settings.remix || (!(landmassData.Position.Y < (float)GenVars.remixSurfaceLayerLow) && !(landmassData.Position.Y > (float)GenVars.remixMushroomLayerHigh))))
					{
						break;
					}
				}
				num4 = (int)MathHelper.Clamp(landmassData.Top.X, num2, Main.maxTilesX - num2);
				spawnY = (int)MathHelper.Clamp(landmassData.Top.Y, num3, Main.maxTilesY - num3);
				num5 = (int)MathHelper.Clamp(spawnY + landmassData.RadiusOrHalfSize / 2, num3, Main.maxTilesY - num3);
				item = landmassData;
			}
			else if (settings.roundLandmass || settings.extraLiquid)
			{
				LandmassData landmassData2 = default(LandmassData);
				Vector2 vector = Vector2.Zero;
				int num7 = 500;
				while (num7 > 0)
				{
					num7--;
					if (_listOfLandmasses.Count <= 0)
					{
						break;
					}
					landmassData2 = _listOfLandmasses[WorldGen.genRand.Next(_listOfLandmasses.Count)];
					vector = landmassData2.Position;
					if (settings.roundLandmass)
					{
						vector = landmassData2.Top;
					}
					if ((!settings.surface || !((double)vector.Y > Main.worldSurface)) && (!settings.remix || (!(vector.Y < (float)GenVars.remixSurfaceLayerLow) && !(vector.Y > (float)GenVars.remixMushroomLayerHigh))) && (!settings.roundLandmass || num7 <= 250 || landmassData2.RadiusOrHalfSize >= 40) && (!settings.extraLiquid || num7 <= 250 || landmassData2.RadiusOrHalfSize >= 10))
					{
						break;
					}
				}
				num4 = (int)MathHelper.Clamp(vector.X, num2, Main.maxTilesX - num2);
				spawnY = (int)MathHelper.Clamp(vector.Y, num3, Main.maxTilesY - num3);
				num5 = (int)MathHelper.Clamp(spawnY + landmassData2.RadiusOrHalfSize / 2, num3, Main.maxTilesY - num3);
				item = landmassData2;
			}
			flag = !flag;
			if (GenerateExtraSpawns_TryFindSpawnAt(spawnPoints, ref num4, ref spawnY, num5))
			{
				spawnPoints.Add(new Point(num4, spawnY));
				if (!item.Equals(default(LandmassData)))
				{
					_listOfLandmasses.Remove(item);
				}
				return true;
			}
		}
		spawnPoints.Add(fallbackSpawn);
		return false;
	}

	private static bool GenerateExtraSpawns_TryFindSpawnAt(List<Point> spawnPoints, ref int spawnX, ref int spawnY, int maxY)
	{
		spawnY = GenerateExtraSpawns_IterateDownToFloor(spawnX, spawnY, maxY);
		int num = 50;
		if (settings.skyblock)
		{
			num = 15;
		}
		if (settings.extraLiquid)
		{
			num = 30;
		}
		bool canSpawn = false;
		int teleportStartX = Math.Max(0, spawnX - num);
		int teleportRangeX = num;
		int teleportStartY = Math.Max(0, spawnY - num);
		int teleportRangeY = num;
		int[] tilesToAvoidForSpawn_TeamBasedSpawns = WorldGen.GetTilesToAvoidForSpawn_TeamBasedSpawns();
		int tilesToAvoidRange = 50;
		int maximumFallDistanceFromOrignalPoint = 100;
		Func<Tile, int, int, bool> specializedConditions = delegate(Tile tile, int x, int y)
		{
			Point point = new Point(x, y);
			int num2 = 250;
			if (settings.extraLiquid && tile.type == 379)
			{
				return false;
			}
			if (settings.skyblock && tile.type != 0)
			{
				return false;
			}
			if (settings.remix)
			{
				if (WorldGen.GetWorldSize() == 0)
				{
					num2 = 150;
				}
				if (y < GenVars.remixSurfaceLayerLow || y > GenVars.remixMushroomLayerHigh)
				{
					return false;
				}
			}
			if (settings.roundLandmass && WorldGen.GetWorldSize() == 0)
			{
				num2 = 150;
			}
			for (int i = 0; i < spawnPoints.Count; i++)
			{
				Point point2 = spawnPoints[i];
				int num3 = Math.Abs(point2.X - point.X);
				int num4 = Math.Abs(point2.Y - point.Y);
				if (num3 < num2 && num4 < num2)
				{
					return false;
				}
			}
			return true;
		};
		Vector2 vector = Utils.CheckForGoodTeleportationSpot(ref canSpawn, teleportStartX, teleportRangeX, teleportStartY, teleportRangeY, new Utils.RandomTeleportationAttemptSettings
		{
			teleporteeSize = new Vector2(20f, 42f),
			teleporteeVelocity = Vector2.Zero,
			teleporteeGravityDirection = 1f,
			avoidLava = true,
			avoidAnyLiquid = true,
			avoidHurtTiles = true,
			avoidWalls = true,
			mostlySolidFloor = true,
			strictRange = true,
			maximumFallDistanceFromOrignalPoint = maximumFallDistanceFromOrignalPoint,
			attemptsBeforeGivingUp = 250,
			tilesToAvoid = tilesToAvoidForSpawn_TeamBasedSpawns,
			tilesToAvoidRange = tilesToAvoidRange,
			specializedConditions = specializedConditions
		});
		if (canSpawn)
		{
			spawnX = (int)(vector.X / 16f);
			spawnY = (int)(vector.Y / 16f);
			return true;
		}
		return false;
	}

	private static Point GenerateExtraSpawns_GetFallbackSpawn(int iteration, int iterationMax)
	{
		float num = GenerateExtraSpawns_WorldPercentileAvoidWorldSpawnIfNeeded((float)iteration / (float)iterationMax);
		int num2 = (int)((float)Main.maxTilesX * num);
		int spawnY = 0;
		_ = Main.worldSurface;
		if (!settings.surface)
		{
			spawnY = (int)((float)Main.maxTilesY * 0.5f);
			_ = Main.UnderworldLayer;
		}
		if (settings.roundLandmass)
		{
			if (settings.surface)
			{
				spawnY = 0;
				_ = Main.worldSurface;
			}
			else
			{
				spawnY = (int)Main.worldSurface + 100;
				_ = Main.UnderworldLayer;
			}
		}
		if (settings.remix)
		{
			spawnY = (int)MathHelper.Lerp(GenVars.remixSurfaceLayerLow, GenVars.remixMushroomLayerHigh, 0.5f);
			_ = GenVars.remixMushroomLayerHigh;
		}
		spawnY = GenerateExtraSpawns_IterateDownToFloor(num2, spawnY, (int)Main.worldSurface);
		return new Point(num2, spawnY);
	}

	private static float GenerateExtraSpawns_WorldPercentileAvoidWorldSpawnIfNeeded(float currentPercentile)
	{
		if (settings.surface || settings.remix)
		{
			float num = 0.1f;
			if (currentPercentile < 0.5f)
			{
				return Utils.Remap(currentPercentile, 0f, 0.5f, 0f, 0.5f - num);
			}
			return Utils.Remap(currentPercentile, 0.5f, 1f, 0.5f + num, 1f);
		}
		return currentPercentile;
	}

	private static int GenerateExtraSpawns_IterateDownToFloor(int spawnX, int spawnY, int maxY)
	{
		if (spawnY > Main.maxTilesY - 5)
		{
			spawnY = Main.maxTilesY - 5;
		}
		else if (spawnY < 5)
		{
			spawnY = 5;
		}
		if (maxY <= spawnY)
		{
			return spawnY;
		}
		bool extraLiquid = settings.extraLiquid;
		for (int i = spawnY; i < maxY && i < Main.maxTilesY; i++)
		{
			Tile tile = Main.tile[spawnX, i];
			if (tile.active() && (extraLiquid || tile.liquid <= 0) && (tile.type < 0 || Main.tileSolid[tile.type]))
			{
				return i;
			}
		}
		return spawnY;
	}

	public static void PrepareExtraSpawns()
	{
		GenerateExtraSpawns_Setup();
		for (int i = 1; i < extraSpawnPoints.Length; i++)
		{
			Point point = extraSpawnPoints[i];
			if ((double)point.Y >= Main.worldSurface && point.Y < Main.UnderworldLayer)
			{
				WorldGen.PlaceTorchesAroundSpawn(point.X, point.Y);
			}
		}
	}

	public static void Clear()
	{
		extraSpawnPoints = new Point[0];
		settings = default(ExtraSpawnSettings);
		_listOfLandmasses.Clear();
	}

	public static void Read(BinaryReader reader, bool networking = false)
	{
		byte b = reader.ReadByte();
		extraSpawnPoints = new Point[b];
		for (int i = 0; i < b; i++)
		{
			int x = reader.ReadInt16();
			int y = reader.ReadInt16();
			extraSpawnPoints[i] = new Point(x, y);
		}
	}

	public static void Write(BinaryWriter writer, bool networking = false)
	{
		writer.Write((byte)extraSpawnPoints.Length);
		for (int i = 0; i < extraSpawnPoints.Length; i++)
		{
			writer.Write((short)extraSpawnPoints[i].X);
			writer.Write((short)extraSpawnPoints[i].Y);
		}
	}
}
