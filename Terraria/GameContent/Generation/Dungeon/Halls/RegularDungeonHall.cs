using System;
using System.Collections.Generic;
using ReLogic.Utilities;
using Terraria.GameContent.Generation.Dungeon.Rooms;
using Terraria.Utilities;

namespace Terraria.GameContent.Generation.Dungeon.Halls;

public class RegularDungeonHall : DungeonHall
{
	public RegularDungeonHall(DungeonHallSettings settings)
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
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		calculated = false;
		RegularHall(data, startPoint, endPoint);
		calculated = true;
	}

	public override void GenerateHall(DungeonData data)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		generated = false;
		RegularHall(data, StartPosition, EndPosition, generating: true);
		generated = true;
	}

	public void RegularHall(DungeonData data, Vector2D startPoint, Vector2D endPoint, bool generating = false)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		RegularDungeonHallSettings regularDungeonHallSettings = (RegularDungeonHallSettings)settings;
		UnifiedRandom unifiedRandom = new UnifiedRandom(regularDungeonHallSettings.RandomSeed);
		ushort brickTileType = settings.StyleData.BrickTileType;
		ushort brickCrackedTileType = settings.StyleData.BrickCrackedTileType;
		ushort brickWallType = settings.StyleData.BrickWallType;
		Vector2D val = startPoint;
		bool flag = false;
		if (regularDungeonHallSettings.CrackedBrickChance > 0.0)
		{
			flag = unifiedRandom.NextDouble() <= regularDungeonHallSettings.CrackedBrickChance;
		}
		int num = 3;
		int num2 = 8;
		if (regularDungeonHallSettings.OverrideInnerBoundsSize > 0)
		{
			num = regularDungeonHallSettings.OverrideInnerBoundsSize;
		}
		if (regularDungeonHallSettings.OverrideOuterBoundsSize > 0)
		{
			num2 = regularDungeonHallSettings.OverrideOuterBoundsSize;
		}
		int num3 = num + num2;
		Vector2D v = endPoint - startPoint;
		Vector2D val2 = v.SafeNormalize(Vector2D.UnitX);
		int num4 = (int)Math.Ceiling(((Vector2D)(ref v)).Length() / ((Vector2D)(ref val2)).Length());
		Bounds.SetBounds((int)startPoint.X, (int)startPoint.Y, (int)startPoint.X, (int)startPoint.Y);
		DungeonRoomSearchSettings dungeonRoomSearchSettings = new DungeonRoomSearchSettings
		{
			Fluff = num3
		};
		List<DungeonRoom> allRoomsInSpots = DungeonUtils.GetAllRoomsInSpots(data.dungeonRooms, startPoint, endPoint, dungeonRoomSearchSettings);
		Vector2D val3 = val2;
		while (num4 > 0 && WorldGen.InWorld((int)(val.X + val2.X), (int)(val.Y + val2.Y), 10))
		{
			if (!base.Processed)
			{
				data.dungeonBounds.UpdateBounds((int)val.X - num3, (int)val.Y - num3, (int)val.Y + num3, (int)val.Y + num3);
				Bounds.UpdateBounds((int)val.X - num3, (int)val.Y - num3, (int)val.Y + num3, (int)val.Y + num3);
			}
			if (generating)
			{
				GenerateDungeonSquareHall(data, allRoomsInSpots, val, brickTileType, brickCrackedTileType, brickWallType, num, num2, regularDungeonHallSettings.PlaceOverProtectedBricks, flag);
			}
			val += val2;
			num4--;
		}
		data.genVars.generatingDungeonPositionX = (int)endPoint.X;
		data.genVars.generatingDungeonPositionY = (int)endPoint.Y;
		StartPosition = startPoint;
		EndPosition = endPoint;
		StartDirection = new Vector2D(val3.X, val3.Y);
		EndDirection = new Vector2D(val2.X, val2.Y);
		CrackedBrick = flag;
		Bounds.CalculateHitbox();
	}
}
