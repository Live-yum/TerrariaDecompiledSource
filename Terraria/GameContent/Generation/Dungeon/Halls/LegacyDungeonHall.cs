using System;
using System.Collections.Generic;
using ReLogic.Utilities;
using Terraria.GameContent.Generation.Dungeon.Rooms;
using Terraria.Utilities;

namespace Terraria.GameContent.Generation.Dungeon.Halls;

public class LegacyDungeonHall : DungeonHall
{
	public Vector2D LastHall;

	public int Strength;

	public int Steps;

	protected Vector2D OverrideStartPosition;

	protected Vector2D OverrideEndPosition;

	public LegacyDungeonHall(DungeonHallSettings settings)
		: base(settings)
	{
	}

	public override void CalculatePlatformsAndDoors(DungeonData data)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if (base.Processed)
		{
			DungeonUtils.CalculatePlatformAndDoorsOnHallway(data, StartPosition, StartDirection.Y, settings.ForceStyleForDoorsAndPlatforms ? settings.StyleData : null);
			DungeonUtils.CalculatePlatformAndDoorsOnHallway(data, EndPosition, EndDirection.Y, settings.ForceStyleForDoorsAndPlatforms ? settings.StyleData : null);
		}
	}

	public override void CalculateHall(DungeonData data, Vector2D startPoint, Vector2D endPoint)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		calculated = false;
		OverrideStartPosition = startPoint;
		OverrideEndPosition = endPoint;
		LegacyHall(data, 0, 0);
		calculated = true;
	}

	public override void GenerateHall(DungeonData data)
	{
		generated = false;
		LegacyHall(data, 0, 0, generating: true);
		generated = true;
	}

	public bool GenerateHall(DungeonData data, int x, int y)
	{
		generated = false;
		LegacyHall(data, x, y, generating: true);
		generated = true;
		return true;
	}

	public virtual void LegacyHall(DungeonData dungeonData, int i, int j, bool generating = false)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_07cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0807: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0818: Unknown result type (might be due to invalid IL or missing references)
		//IL_0829: Unknown result type (might be due to invalid IL or missing references)
		//IL_083a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d21: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d23: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d26: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d28: Unknown result type (might be due to invalid IL or missing references)
		//IL_076d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0772: Unknown result type (might be due to invalid IL or missing references)
		//IL_085d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0daa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0db0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0db5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dbf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0de1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0de3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d85: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d8d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d95: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d9d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d39: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d45: Unknown result type (might be due to invalid IL or missing references)
		//IL_08f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0992: Unknown result type (might be due to invalid IL or missing references)
		//IL_1457: Unknown result type (might be due to invalid IL or missing references)
		//IL_146a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1478: Unknown result type (might be due to invalid IL or missing references)
		//IL_147a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1480: Unknown result type (might be due to invalid IL or missing references)
		//IL_1482: Unknown result type (might be due to invalid IL or missing references)
		//IL_1488: Unknown result type (might be due to invalid IL or missing references)
		//IL_148a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1490: Unknown result type (might be due to invalid IL or missing references)
		//IL_1492: Unknown result type (might be due to invalid IL or missing references)
		//IL_14a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_14aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a3e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e31: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dfd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e04: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e0d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e14: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e5d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e43: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f56: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f89: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fbc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e89: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e6f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0eaf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e9b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ec1: Unknown result type (might be due to invalid IL or missing references)
		//IL_140f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1411: Unknown result type (might be due to invalid IL or missing references)
		//IL_1413: Unknown result type (might be due to invalid IL or missing references)
		//IL_1418: Unknown result type (might be due to invalid IL or missing references)
		//IL_118f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0edf: Unknown result type (might be due to invalid IL or missing references)
		//IL_11b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f1b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ef1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f2d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1201: Unknown result type (might be due to invalid IL or missing references)
		//IL_1228: Unknown result type (might be due to invalid IL or missing references)
		//IL_124f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1276: Unknown result type (might be due to invalid IL or missing references)
		LegacyDungeonHallSettings legacyDungeonHallSettings = (LegacyDungeonHallSettings)settings;
		UnifiedRandom unifiedRandom = new UnifiedRandom(legacyDungeonHallSettings.RandomSeed);
		ushort brickTileType = settings.StyleData.BrickTileType;
		ushort brickCrackedTileType = settings.StyleData.BrickCrackedTileType;
		ushort brickWallType = settings.StyleData.BrickWallType;
		Vector2D val = default(Vector2D);
		((Vector2D)(ref val))._002Ector((double)i, (double)j);
		Vector2D startPosition = val;
		Vector2D val2 = Vector2D.Zero;
		int num = (int)(4.0 * dungeonData.hallStrengthScalar) + unifiedRandom.Next(2);
		Vector2D zero = Vector2D.Zero;
		Vector2D zero2 = Vector2D.Zero;
		int num2 = 1;
		double hallStepScalar = dungeonData.hallStepScalar;
		int num3 = (int)(35.0 * hallStepScalar) + unifiedRandom.Next(45);
		bool flag = false;
		if (legacyDungeonHallSettings.CrackedBrickChance > 0.0)
		{
			flag = unifiedRandom.NextDouble() <= legacyDungeonHallSettings.CrackedBrickChance;
		}
		if (legacyDungeonHallSettings.ForceHorizontal)
		{
			num3 += (int)(20.0 * hallStepScalar);
			dungeonData.lastDungeonHall = Vector2D.Zero;
		}
		else
		{
			if (unifiedRandom.Next(5) == 0)
			{
				num *= 2;
				num3 /= 2;
			}
			if (WorldGen.SecretSeed.errorWorld.Enabled && unifiedRandom.Next(2) == 0)
			{
				num *= 2;
			}
			if (WorldGen.SecretSeed.errorWorld.Enabled && unifiedRandom.Next(2) == 0)
			{
				num3 *= 2;
			}
		}
		Vector2D lastHall = dungeonData.lastDungeonHall;
		if (calculated)
		{
			val = (startPosition = StartPosition);
			val2 = (EndPosition - StartPosition).SafeNormalize(Vector2D.UnitX);
			num = Strength;
			num3 = Steps;
			lastHall = LastHall;
		}
		int steps = num3;
		int num4 = num;
		double num5 = dungeonData.hallInteriorToExteriorRatio;
		if ((float)legacyDungeonHallSettings.OverrideStrength > 0f)
		{
			num = (num4 = legacyDungeonHallSettings.OverrideStrength);
		}
		if (legacyDungeonHallSettings.OverrideSteps > 0)
		{
			num3 = (steps = legacyDungeonHallSettings.OverrideSteps);
		}
		if (legacyDungeonHallSettings.OverrideInteriorToExteriorRatio > 0.0)
		{
			num5 = legacyDungeonHallSettings.OverrideInteriorToExteriorRatio;
		}
		bool flag2 = false;
		int num6 = Main.UnderworldLayer - (int)(100.0 * ((dungeonData.HallSizeScalar > dungeonData.RoomSizeScalar) ? dungeonData.HallSizeScalar : dungeonData.RoomSizeScalar));
		bool flag3 = false;
		if (OverrideStartPosition != default(Vector2D) && OverrideEndPosition != default(Vector2D))
		{
			flag3 = true;
			Vector2D overrideStartPosition = OverrideStartPosition;
			Vector2D v = OverrideEndPosition - overrideStartPosition;
			Vector2D val3 = v.SafeNormalize(Vector2D.UnitX);
			num3 = (steps = (int)Math.Ceiling(((Vector2D)(ref v)).Length() / ((Vector2D)(ref val3)).Length()));
			val = overrideStartPosition;
			startPosition = val;
			zero.X = val3.X;
			zero.Y = val3.Y;
			zero2.X = 0.0 - val3.X;
			zero2.Y = 0.0 - val3.Y;
			val2 = val3;
		}
		else
		{
			bool flag4 = false;
			bool flag5 = true;
			bool flag6 = false;
			while (!flag4)
			{
				flag6 = false;
				if (flag5 && !legacyDungeonHallSettings.ForceHorizontal)
				{
					bool flag7 = true;
					bool flag8 = true;
					bool flag9 = true;
					bool flag10 = true;
					bool flag11 = false;
					int num7 = num3;
					bool flag12 = false;
					for (int num8 = j; num8 > j - num7; num8--)
					{
						if (!WorldGen.InWorld(i, num8, 50))
						{
							flag7 = false;
							break;
						}
						if (DungeonUtils.IsConsideredDungeonWall(Main.tile[i, num8].wall))
						{
							if (flag12)
							{
								flag7 = false;
								break;
							}
						}
						else
						{
							flag12 = true;
						}
					}
					flag12 = false;
					for (int k = j; k < j + num7; k++)
					{
						if (!WorldGen.InWorld(i, k, 50))
						{
							flag8 = false;
							break;
						}
						if (k >= num6)
						{
							flag11 = true;
							flag8 = false;
							break;
						}
						if (DungeonUtils.IsConsideredDungeonWall(Main.tile[i, k].wall))
						{
							if (flag12)
							{
								flag8 = false;
								break;
							}
						}
						else
						{
							flag12 = true;
						}
					}
					flag12 = false;
					for (int num9 = i; num9 > i - num7; num9--)
					{
						if (!WorldGen.InWorld(num9, j, 50))
						{
							flag9 = false;
							break;
						}
						if (DungeonUtils.IsConsideredDungeonWall(Main.tile[num9, j].wall))
						{
							if (flag12)
							{
								flag9 = false;
								break;
							}
						}
						else
						{
							flag12 = true;
						}
					}
					flag12 = false;
					for (int l = i; l < i + num7; l++)
					{
						if (!WorldGen.InWorld(l, j, 50))
						{
							flag10 = false;
							break;
						}
						if (DungeonUtils.IsConsideredDungeonWall(Main.tile[l, j].wall))
						{
							if (flag12)
							{
								flag10 = false;
								break;
							}
						}
						else
						{
							flag12 = true;
						}
					}
					if (!flag9 && !flag10 && !flag7 && !flag8)
					{
						num2 = ((unifiedRandom.Next(2) != 0) ? 1 : (-1));
						if (unifiedRandom.Next(2) == 0)
						{
							flag6 = true;
						}
						if (num2 == 1 && !flag6 && flag11)
						{
							num2 = ((unifiedRandom.Next(2) == 0) ? 1 : (-1));
							flag6 = true;
						}
					}
					else
					{
						int num10 = 0;
						int num11 = 100;
						do
						{
							num11--;
							if (num11 <= 0)
							{
								num10 = 0;
								break;
							}
							num10 = unifiedRandom.Next(4);
							if (num10 == 1 && flag11)
							{
								num10 = ((unifiedRandom.Next(2) == 0) ? 2 : 3);
							}
						}
						while (!(num10 == 0 && flag7) && !(num10 == 1 && flag8) && !(num10 == 2 && flag9) && !(num10 == 3 && flag10));
						switch (num10)
						{
						case 0:
							num2 = -1;
							break;
						case 1:
							num2 = 1;
							break;
						default:
							flag6 = true;
							num2 = ((num10 != 2) ? 1 : (-1));
							break;
						}
					}
				}
				else
				{
					num2 = ((unifiedRandom.Next(2) != 0) ? 1 : (-1));
					if (unifiedRandom.Next(2) == 0)
					{
						flag6 = true;
					}
					if (num2 == 1 && j + num3 >= num6)
					{
						num2 = ((unifiedRandom.Next(2) != 0) ? 1 : (-1));
						flag6 = true;
					}
				}
				flag5 = false;
				if (legacyDungeonHallSettings.ForceHorizontal)
				{
					flag6 = true;
				}
				if (flag6)
				{
					zero.Y = 0.0;
					zero.X = num2;
					zero2.Y = 0.0;
					zero2.X = -num2;
					val2.Y = 0.0;
					val2.X = num2;
					if (unifiedRandom.Next(3) == 0)
					{
						if (unifiedRandom.Next(2) == 0)
						{
							val2.Y = -0.20000000298023224 * dungeonData.hallSlantVariantScalar;
						}
						else
						{
							val2.Y = 0.20000000298023224 * dungeonData.hallSlantVariantScalar;
						}
					}
				}
				else
				{
					num++;
					val2.Y = num2;
					val2.X = 0.0;
					zero.X = 0.0;
					zero.Y = num2;
					zero2.X = 0.0;
					zero2.Y = -num2;
					if (legacyDungeonHallSettings.ZigzagChance > 0.0 && unifiedRandom.NextDouble() <= legacyDungeonHallSettings.ZigzagChance)
					{
						flag2 = true;
						if (unifiedRandom.Next(2) == 0)
						{
							val2.X = (double)unifiedRandom.Next(10, 20) * 0.1 * dungeonData.hallSlantVariantScalar;
						}
						else
						{
							val2.X = (double)(-unifiedRandom.Next(10, 20)) * 0.1 * dungeonData.hallSlantVariantScalar;
						}
					}
					else if (unifiedRandom.Next(2) == 0)
					{
						if (unifiedRandom.Next(2) == 0)
						{
							val2.X = (double)unifiedRandom.Next(20, 40) * 0.01 * dungeonData.hallSlantVariantScalar;
						}
						else
						{
							val2.X = (double)(-unifiedRandom.Next(20, 40)) * 0.01 * dungeonData.hallSlantVariantScalar;
						}
					}
					else
					{
						num3 /= 2;
					}
				}
				if (dungeonData.lastDungeonHall != zero2)
				{
					flag4 = true;
				}
			}
		}
		int num12 = 0;
		float num13 = (float)Main.maxTilesX * 0.25f;
		float num14 = (float)Main.maxTilesX * 0.75f;
		if (WorldGen.SecretSeed.errorWorld.Enabled)
		{
			num13 = (float)Main.maxTilesX * 0.4f;
			num14 = (float)Main.maxTilesX * 0.6f;
		}
		bool flag13 = val.Y < Main.rockLayer + 100.0;
		if (WorldGen.remixWorldGen)
		{
			flag13 = val.Y < Main.worldSurface + 100.0;
		}
		bool flag14 = val.X < (double)(Main.maxTilesX / 2) && val.X > (double)num13;
		bool flag15 = val.X > (double)(Main.maxTilesX / 2) && val.X < (double)num14;
		if (!flag3 && !legacyDungeonHallSettings.ForceHorizontal)
		{
			if (val.X > (double)(Main.maxTilesX - 200))
			{
				num2 = -1;
				zero.X = num2;
				zero.Y = 0.0;
				val2.X = num2;
				val2.Y = 0.0;
				if (unifiedRandom.Next(3) == 0)
				{
					if (unifiedRandom.Next(2) == 0)
					{
						val2.Y = -0.20000000298023224 * dungeonData.hallSlantVariantScalar;
					}
					else
					{
						val2.Y = 0.20000000298023224 * dungeonData.hallSlantVariantScalar;
					}
				}
			}
			else if (val.X < 200.0)
			{
				num2 = 1;
				zero.X = num2;
				zero.Y = 0.0;
				val2.X = num2;
				val2.Y = 0.0;
				if (unifiedRandom.Next(3) == 0)
				{
					if (unifiedRandom.Next(2) == 0)
					{
						val2.Y = -0.20000000298023224 * dungeonData.hallSlantVariantScalar;
					}
					else
					{
						val2.Y = 0.20000000298023224 * dungeonData.hallSlantVariantScalar;
					}
				}
			}
			else if (val.Y >= (double)num6)
			{
				num2 = -1;
				num++;
				zero.X = 0.0;
				zero.Y = num2;
				val2.X = 0.0;
				val2.Y = num2;
				if (unifiedRandom.Next(2) == 0)
				{
					if (unifiedRandom.Next(2) == 0)
					{
						val2.X = (double)((float)unifiedRandom.Next(20, 50) * 0.01f) * dungeonData.hallSlantVariantScalar;
					}
					else
					{
						val2.X = (double)((float)(-unifiedRandom.Next(20, 50)) * 0.01f) * dungeonData.hallSlantVariantScalar;
					}
				}
			}
			else if (val.Y < 200.0)
			{
				num2 = 1;
				num++;
				zero.X = 0.0;
				zero.Y = num2;
				val2.X = 0.0;
				val2.Y = num2;
				if (unifiedRandom.Next(2) == 0)
				{
					if (unifiedRandom.Next(2) == 0)
					{
						val2.X = (double)((float)unifiedRandom.Next(20, 50) * 0.01f) * dungeonData.hallSlantVariantScalar;
					}
					else
					{
						val2.X = (double)((float)(-unifiedRandom.Next(20, 50)) * 0.01f) * dungeonData.hallSlantVariantScalar;
					}
				}
			}
			else if (!flag3)
			{
				if (flag13)
				{
					num2 = 1;
					num++;
					zero.X = 0.0;
					zero.Y = num2;
					val2.X = 0.0;
					val2.Y = num2;
					if (legacyDungeonHallSettings.ZigzagChance > 0.0 && unifiedRandom.NextDouble() <= legacyDungeonHallSettings.ZigzagChance)
					{
						flag2 = true;
						if (unifiedRandom.Next(2) == 0)
						{
							val2.X = (double)unifiedRandom.Next(10, 20) * 0.1 * dungeonData.hallSlantVariantScalar;
						}
						else
						{
							val2.X = (double)(-unifiedRandom.Next(10, 20)) * 0.1 * dungeonData.hallSlantVariantScalar;
						}
					}
					else if (unifiedRandom.Next(2) == 0)
					{
						if (unifiedRandom.Next(2) == 0)
						{
							val2.X = (double)unifiedRandom.Next(20, 50) * 0.01 * dungeonData.hallSlantVariantScalar;
						}
						else
						{
							val2.X = (double)unifiedRandom.Next(20, 50) * 0.01 * dungeonData.hallSlantVariantScalar;
						}
					}
				}
				else if (flag14)
				{
					num2 = -1;
					zero.Y = 0.0;
					zero.X = num2;
					val2.Y = 0.0;
					val2.X = num2;
					if (unifiedRandom.Next(3) == 0)
					{
						if (unifiedRandom.Next(2) == 0)
						{
							val2.Y = -0.20000000298023224 * dungeonData.hallSlantVariantScalar;
						}
						else
						{
							val2.Y = 0.20000000298023224 * dungeonData.hallSlantVariantScalar;
						}
					}
				}
				else if (flag15)
				{
					num2 = 1;
					zero.Y = 0.0;
					zero.X = num2;
					val2.Y = 0.0;
					val2.X = num2;
					if (unifiedRandom.Next(3) == 0)
					{
						if (unifiedRandom.Next(2) == 0)
						{
							val2.Y = -0.20000000298023224 * dungeonData.hallSlantVariantScalar;
						}
						else
						{
							val2.Y = 0.20000000298023224 * dungeonData.hallSlantVariantScalar;
						}
					}
				}
			}
		}
		Vector2D startDirection = zero;
		dungeonData.lastDungeonHall = zero;
		if (!calculated && !flag3 && Math.Abs(val2.X) > Math.Abs(val2.Y) && unifiedRandom.Next(3) != 0)
		{
			num = (int)((float)num4 * ((float)unifiedRandom.Next(110, 150) * 0.01f));
		}
		if (!base.Processed)
		{
			Bounds.SetBounds((int)val.X, (int)val.Y, (int)val.X, (int)val.Y);
		}
		Vector2D startPos = val;
		Vector2D endPos = val + val2 * (double)num3;
		DungeonRoomSearchSettings dungeonRoomSearchSettings = new DungeonRoomSearchSettings
		{
			Fluff = num3 / 2 + num
		};
		List<DungeonRoom> allRoomsInSpots = DungeonUtils.GetAllRoomsInSpots(dungeonData.dungeonRooms, startPos, endPos, dungeonRoomSearchSettings);
		while (num3 > 0)
		{
			num12++;
			if (flag3)
			{
				if (!WorldGen.InWorld((int)(val.X + zero.X), (int)(val.Y + zero.Y), 10))
				{
					num3 = 0;
				}
			}
			else if (zero.X > 0.0 && val.X > (double)(Main.maxTilesX - 100))
			{
				num3 = 0;
			}
			else if (zero.X < 0.0 && val.X < 100.0)
			{
				num3 = 0;
			}
			else if (zero.Y > 0.0 && val.Y >= (double)num6)
			{
				num3 = 0;
			}
			else if (zero.Y < 0.0 && val.Y < 100.0)
			{
				num3 = 0;
			}
			else if (WorldGen.remixWorldGen && zero.Y < 0.0 && val.Y < (Main.rockLayer + Main.worldSurface) / 2.0)
			{
				num3 = 0;
			}
			else if (!WorldGen.remixWorldGen && zero.Y < 0.0 && val.Y < Main.rockLayer + 50.0)
			{
				num3 = 0;
			}
			num3--;
			int num15 = Math.Max(0, Math.Min(Main.maxTilesX - 1, (int)(val.X - (double)num - 4.0 - (double)unifiedRandom.Next(6))));
			int num16 = Math.Max(0, Math.Min(Main.maxTilesX - 1, (int)(val.X + (double)num + 4.0 + (double)unifiedRandom.Next(6))));
			int num17 = Math.Max(0, Math.Min(Main.maxTilesY - 1, (int)(val.Y - (double)num - 4.0 - (double)unifiedRandom.Next(6))));
			int num18 = Math.Max(0, Math.Min(Main.maxTilesY - 1, (int)(val.Y + (double)num + 4.0 + (double)unifiedRandom.Next(6))));
			if (!base.Processed)
			{
				dungeonData.dungeonBounds.UpdateBounds(num15, num17, num16, num18);
				Bounds.UpdateBounds(num15, num17, num16, num18);
			}
			if (generating && !settings.CarveOnly)
			{
				for (int m = num15; m < num16; m++)
				{
					for (int n = num17; n < num18; n++)
					{
						bool flag16 = true;
						ProtectionType highestProtectionTypeFromPoint = DungeonUtils.GetHighestProtectionTypeFromPoint(m, n, allRoomsInSpots);
						if (highestProtectionTypeFromPoint != ProtectionType.TilesAndWalls)
						{
							if (highestProtectionTypeFromPoint == ProtectionType.Tiles)
							{
								flag16 = false;
							}
							Tile tile = Main.tile[m, n];
							tile.liquid = 0;
							if (flag16 && n <= Main.UnderworldLayer + 7 && CanPlaceTileAt(dungeonData, tile, brickTileType, brickCrackedTileType))
							{
								DungeonUtils.ChangeTileType(tile, brickTileType, resetTile: true, settings.OverridePaintTile);
							}
						}
					}
				}
				for (int num19 = num15 + 1; num19 < num16 - 1; num19++)
				{
					for (int num20 = num17 + 1; num20 < num18 - 1; num20++)
					{
						if (num20 >= Main.UnderworldLayer + 7)
						{
							continue;
						}
						bool flag17 = true;
						ProtectionType highestProtectionTypeFromPoint2 = DungeonUtils.GetHighestProtectionTypeFromPoint(num19, num20, allRoomsInSpots);
						if (highestProtectionTypeFromPoint2 != ProtectionType.TilesAndWalls)
						{
							if (highestProtectionTypeFromPoint2 == ProtectionType.Walls && DungeonUtils.IsConsideredDungeonWall(Main.tile[num19, num20].wall))
							{
								flag17 = false;
							}
							if (flag17)
							{
								DungeonUtils.ChangeWallType(Main.tile[num19, num20], brickWallType, resetTile: false, settings.OverridePaintWall);
							}
						}
					}
				}
			}
			if (generating)
			{
				int num21 = 0;
				if (val2.Y == 0.0 && unifiedRandom.Next(num + 1) == 0)
				{
					num21 = unifiedRandom.Next(1, 3);
				}
				else if (val2.X == 0.0 && unifiedRandom.Next(num - 1) == 0)
				{
					num21 = unifiedRandom.Next(1, 3);
				}
				else if (unifiedRandom.Next(num * 3) == 0)
				{
					num21 = unifiedRandom.Next(1, 3);
				}
				num15 = Math.Max(0, Math.Min(Main.maxTilesX - 1, (int)(val.X - (double)num * num5 - (double)num21)));
				num16 = Math.Max(0, Math.Min(Main.maxTilesX - 1, (int)(val.X + (double)num * num5 + (double)num21)));
				num17 = Math.Max(0, Math.Min(Main.maxTilesY - 1, (int)(val.Y - (double)num * num5 - (double)num21)));
				num18 = Math.Max(0, Math.Min(Main.maxTilesY - 1, (int)(val.Y + (double)num * num5 + (double)num21)));
				for (int num22 = num15; num22 < num16; num22++)
				{
					for (int num23 = num17; num23 < num18; num23++)
					{
						bool flag18 = true;
						bool flag19 = true;
						ProtectionType highestProtectionTypeFromPoint3 = DungeonUtils.GetHighestProtectionTypeFromPoint(num22, num23, allRoomsInSpots);
						if (highestProtectionTypeFromPoint3 == ProtectionType.TilesAndWalls)
						{
							continue;
						}
						if (highestProtectionTypeFromPoint3 == ProtectionType.Tiles)
						{
							flag18 = false;
						}
						if (highestProtectionTypeFromPoint3 == ProtectionType.Walls && DungeonUtils.IsConsideredDungeonWall(Main.tile[num22, num23].wall))
						{
							flag19 = false;
						}
						if (!CanRemoveTileAt(dungeonData, Main.tile[num22, num23], brickCrackedTileType))
						{
							continue;
						}
						if (flag)
						{
							if ((Main.tile[num22, num23].active() || !DungeonUtils.IsConsideredDungeonWall(Main.tile[num22, num23].wall)) && num23 < Main.UnderworldLayer)
							{
								if (settings.CarveOnly)
								{
									Main.tile[num22, num23].ClearTile();
								}
								else
								{
									Main.tile[num22, num23].ClearTile();
									if (flag18)
									{
										DungeonUtils.ChangeTileType(Main.tile[num22, num23], brickCrackedTileType, resetTile: false, settings.OverridePaintTile);
									}
								}
							}
						}
						else
						{
							Main.tile[num22, num23].ClearTile();
						}
						if (flag19 && num23 < Main.UnderworldLayer && !settings.CarveOnly)
						{
							DungeonUtils.ChangeWallType(Main.tile[num22, num23], brickWallType, resetTile: false, settings.OverridePaintWall);
						}
					}
				}
			}
			val += val2;
			if (!flag3 && flag2 && num12 > unifiedRandom.Next(10, 20))
			{
				num12 = 0;
				val2.X *= -1.0;
			}
		}
		dungeonData.genVars.generatingDungeonPositionX = (int)val.X;
		dungeonData.genVars.generatingDungeonPositionY = (int)val.Y;
		StartPosition = startPosition;
		EndPosition = val;
		StartDirection = startDirection;
		EndDirection = zero;
		Strength = num4;
		Steps = steps;
		LastHall = lastHall;
		CrackedBrick = flag;
		if (!base.Processed)
		{
			Bounds.CalculateHitbox();
		}
	}
}
