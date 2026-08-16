using System;
using ReLogic.Utilities;
using Terraria.GameContent.Generation.Dungeon.Features;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace Terraria.GameContent.Generation.Dungeon.Rooms;

public class LegacyDungeonRoom : DungeonRoom
{
	private ShapeData _innerShapeData = new ShapeData();

	private ShapeData _outerShapeData = new ShapeData();

	private int _floodedTileCount;

	public Vector2D StartPosition;

	public Vector2D EndPosition;

	public int Strength;

	public LegacyDungeonRoom(DungeonRoomSettings settings)
		: base(settings)
	{
	}

	public override void CalculateRoom(DungeonData data)
	{
		calculated = false;
		int x = settings.RoomPosition.X;
		int y = settings.RoomPosition.Y;
		LegacyRoom(data, x, y, generating: false);
		calculated = true;
	}

	public override bool GenerateRoom(DungeonData data)
	{
		generated = false;
		int x = settings.RoomPosition.X;
		int y = settings.RoomPosition.Y;
		LegacyRoom(data, x, y, generating: true);
		generated = true;
		return true;
	}

	public override int GetFloodedRoomTileCount()
	{
		return _floodedTileCount;
	}

	public override void FloodRoom(byte liquidType)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (generated && _innerShapeData != null)
		{
			WorldUtils.Gen(StartPosition.ToPoint(), new ModShapes.All(_innerShapeData), Actions.Chain(new Modifiers.IsBelowHeight(InnerBounds.Center.Y, inclusive: true), new Modifiers.IsNotSolid(), new Actions.SetLiquid(liquidType)));
		}
	}

	public override ProtectionType GetProtectionTypeFromPoint(int x, int y)
	{
		if (_innerShapeData == null || _outerShapeData == null || (calculated && !OuterBounds.Contains(x, y)))
		{
			return base.GetProtectionTypeFromPoint(x, y);
		}
		if (!_outerShapeData.Contains(x - (int)StartPosition.X, y - (int)StartPosition.Y))
		{
			return ProtectionType.None;
		}
		return ProtectionType.Walls;
	}

	public override bool IsInsideRoom(int x, int y)
	{
		if (base.IsInsideRoom(x, y))
		{
			return _innerShapeData.Contains(x - (int)StartPosition.X, y - (int)StartPosition.Y);
		}
		return false;
	}

	public override bool TryGenerateChestInRoom(DungeonData data, DungeonGlobalBasicChests feature)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		Vector2D endPosition = EndPosition;
		int num = (int)((float)Strength * 0.4f);
		return DungeonUtils.GenerateDungeonRegularChest(data, feature, settings.StyleData, (int)endPosition.X - num, (int)endPosition.Y - num, (int)endPosition.X + num, (int)endPosition.Y + num);
	}

	public override bool DualDungeons_TryGenerateBiomeChestInRoom(DungeonData data, DungeonGlobalBiomeChests feature)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		Vector2D endPosition = EndPosition;
		int num = (int)((float)Strength * 0.4f);
		return DungeonUtils.GenerateDungeonBiomeChest(data, feature, settings.StyleData, (int)endPosition.X - num, (int)endPosition.Y - num, (int)endPosition.X + num, (int)endPosition.Y + num);
	}

	public void LegacyRoom(DungeonData data, int i, int j, bool generating)
	{
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_0349: Unknown result type (might be due to invalid IL or missing references)
		//IL_0679: Unknown result type (might be due to invalid IL or missing references)
		//IL_067b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0681: Unknown result type (might be due to invalid IL or missing references)
		//IL_0683: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_040e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0430: Unknown result type (might be due to invalid IL or missing references)
		//IL_05dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_05df: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0640: Unknown result type (might be due to invalid IL or missing references)
		//IL_047f: Unknown result type (might be due to invalid IL or missing references)
		//IL_048a: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c6: Unknown result type (might be due to invalid IL or missing references)
		LegacyDungeonRoomSettings legacyDungeonRoomSettings = (LegacyDungeonRoomSettings)settings;
		UnifiedRandom unifiedRandom = new UnifiedRandom(legacyDungeonRoomSettings.RandomSeed);
		ushort brickTileType = settings.StyleData.BrickTileType;
		ushort brickWallType = settings.StyleData.BrickWallType;
		double num = data.roomStrengthScalar;
		if (legacyDungeonRoomSettings.StartingRoom)
		{
			num = 1.0;
		}
		double num2 = (int)(15.0 * num) + unifiedRandom.Next(15);
		Vector2D val = default(Vector2D);
		val.X = (double)((float)unifiedRandom.Next(-10, 11) * 0.1f) * data.roomSlantVariantScalar;
		val.Y = (double)((float)unifiedRandom.Next(-10, 11) * 0.1f) * data.roomSlantVariantScalar;
		if (val.X == 0.0 && val.Y == 0.0)
		{
			if (unifiedRandom.Next(2) == 0)
			{
				val.X = ((unifiedRandom.Next(2) != 0) ? 1 : (-1));
			}
			else
			{
				val.Y = ((unifiedRandom.Next(2) != 0) ? 1 : (-1));
			}
		}
		Vector2D val2 = default(Vector2D);
		val2.X = i;
		val2.Y = (double)j - num2 / 2.0;
		if (calculated)
		{
			val2 = StartPosition;
		}
		Vector2D val3 = val2;
		double num3 = data.roomStepScalar;
		if (legacyDungeonRoomSettings.StartingRoom)
		{
			num3 = 1.0;
		}
		int num4 = (int)(10.0 * num3) + unifiedRandom.Next(10);
		double num5 = num2;
		double num6 = data.roomInteriorToExteriorRatio;
		if (legacyDungeonRoomSettings.OverrideStartPosition != default(Vector2D) && legacyDungeonRoomSettings.OverrideEndPosition != default(Vector2D))
		{
			val2 = (val3 = legacyDungeonRoomSettings.OverrideStartPosition);
			Vector2D v = legacyDungeonRoomSettings.OverrideEndPosition - val2;
			val = v.SafeNormalize(Vector2D.UnitX);
			num4 = (int)Math.Ceiling(((Vector2D)(ref v)).Length() / ((Vector2D)(ref val)).Length());
		}
		else if (legacyDungeonRoomSettings.OverrideVelocity != default(Vector2D))
		{
			val = legacyDungeonRoomSettings.OverrideVelocity;
		}
		if (legacyDungeonRoomSettings.OverrideStrength > 0)
		{
			num2 = (num5 = legacyDungeonRoomSettings.OverrideStrength);
		}
		if (legacyDungeonRoomSettings.OverrideSteps > 0)
		{
			num4 = legacyDungeonRoomSettings.OverrideSteps;
		}
		if (legacyDungeonRoomSettings.OverrideInteriorToExteriorRatio > 0.0)
		{
			num6 = legacyDungeonRoomSettings.OverrideInteriorToExteriorRatio;
		}
		InnerBounds.SetBounds((int)val2.X, (int)val2.Y, (int)val2.X, (int)val2.Y);
		OuterBounds.SetBounds((int)val2.X, (int)val2.Y, (int)val2.X, (int)val2.Y);
		while (num4 > 0)
		{
			num4--;
			int num7 = Math.Max(0, Math.Min(Main.maxTilesX - 1, (int)(val2.X - num2 * 0.800000011920929 - 5.0)));
			int num8 = Math.Max(0, Math.Min(Main.maxTilesX - 1, (int)(val2.X + num2 * 0.800000011920929 + 5.0)));
			int num9 = Math.Max(0, Math.Min(Main.maxTilesY - 1, (int)(val2.Y - num2 * 0.800000011920929 - 5.0)));
			int num10 = Math.Max(0, Math.Min(Main.maxTilesY - 1, (int)(val2.Y + num2 * 0.800000011920929 + 5.0)));
			if (legacyDungeonRoomSettings.IsEntranceRoom && data.Type == DungeonType.DualDungeon)
			{
				num10 = Math.Max(num10, DungeonUtils.GetDualDungeonBrickSupportCutoffY(data));
			}
			data.dungeonBounds.UpdateBounds(num7, num9, num8 - 1, num10 - 1);
			OuterBounds.UpdateBounds(num7, num9, num8 - 1, num10 - 1);
			int num11 = Math.Max(0, Math.Min(Main.maxTilesX - 1, (int)(val2.X - num2 * num6)));
			int num12 = Math.Max(0, Math.Min(Main.maxTilesX - 1, (int)(val2.X + num2 * num6)));
			int num13 = Math.Max(0, Math.Min(Main.maxTilesY - 1, (int)(val2.Y - num2 * num6)));
			int num14 = Math.Max(0, Math.Min(Main.maxTilesY - 1, (int)(val2.Y + num2 * num6)));
			InnerBounds.UpdateBounds(num11, num13, num12 - 1, num14 - 1);
			for (int k = num7; k < num8; k++)
			{
				for (int l = num9; l < num10; l++)
				{
					if (!generating)
					{
						_outerShapeData.Add(k - (int)val3.X, l - (int)val3.Y);
						if (k >= num11 && k <= num12 && l >= num13 && l <= num14)
						{
							_innerShapeData.Add(k - (int)val3.X, l - (int)val3.Y);
						}
					}
					else
					{
						Main.tile[k, l].liquid = 0;
						if (!DungeonUtils.IsHigherOrEqualTieredDungeonWall(data, Main.tile[k, l].wall, brickWallType))
						{
							DungeonUtils.ChangeTileType(Main.tile[k, l], brickTileType, resetTile: true, legacyDungeonRoomSettings.OverridePaintTile);
						}
					}
				}
			}
			if (generating)
			{
				for (int m = num7 + 1; m < num8 - 1; m++)
				{
					for (int n = num9 + 1; n < num10 - 1; n++)
					{
						DungeonUtils.ChangeWallType(Main.tile[m, n], brickWallType, resetTile: false, legacyDungeonRoomSettings.OverridePaintWall);
					}
				}
			}
			num7 = num11;
			num8 = num12;
			num9 = num13;
			num10 = num14;
			if (generating)
			{
				for (int num15 = num7; num15 < num8; num15++)
				{
					for (int num16 = num9; num16 < num10; num16++)
					{
						DungeonUtils.ChangeWallType(Main.tile[num15, num16], brickWallType, resetTile: true, legacyDungeonRoomSettings.OverridePaintWall);
					}
				}
			}
			val2 += val;
			val.X = Math.Max(-1.0, Math.Min(1.0, val.X + (double)((float)unifiedRandom.Next(-10, 11) * 0.05f) * data.roomSlantVariantScalar));
			val.Y = Math.Max(-1.0, Math.Min(1.0, val.Y + (double)((float)unifiedRandom.Next(-10, 11) * 0.05f) * data.roomSlantVariantScalar));
		}
		StartPosition = val3;
		EndPosition = val2;
		Strength = (int)num5;
		InnerBounds.CalculateHitbox();
		OuterBounds.CalculateHitbox();
		_floodedTileCount = DungeonUtils.CalculateFloodedTileCountFromShapeData(InnerBounds, _innerShapeData);
	}
}
