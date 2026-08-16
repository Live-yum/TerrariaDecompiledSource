using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria.GameContent.Generation.Dungeon.Rooms;
using Terraria.Utilities;

namespace Terraria.GameContent.Generation.Dungeon.Halls;

public class SineDungeonHall : DungeonHall
{
	public List<Tuple<Vector2D, Vector2D>> PotentialPlatformPoints = new List<Tuple<Vector2D, Vector2D>>();

	public SineDungeonHall(DungeonHallSettings settings)
		: base(settings)
	{
	}

	public override void CalculatePlatformsAndDoors(DungeonData data)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		if (!base.Processed)
		{
			return;
		}
		DungeonUtils.CalculatePlatformAndDoorsOnHallway(data, StartPosition, StartDirection.Y, settings.ForceStyleForDoorsAndPlatforms ? settings.StyleData : null);
		DungeonUtils.CalculatePlatformAndDoorsOnHallway(data, EndPosition, EndDirection.Y, settings.ForceStyleForDoorsAndPlatforms ? settings.StyleData : null);
		float num = 0.65f;
		for (int i = 0; i < PotentialPlatformPoints.Count; i++)
		{
			Tuple<Vector2D, Vector2D> tuple = PotentialPlatformPoints[i];
			Vector2D item = tuple.Item1;
			Vector2D item2 = tuple.Item2;
			if (!(item2.Y < (double)num) || !(item2.Y > (double)(0f - num)))
			{
				DungeonUtils.CalculatePlatformAndDoorsOnHallway(data, item, item2.Y, settings.ForceStyleForDoorsAndPlatforms ? settings.StyleData : null);
			}
		}
	}

	public override void CalculateHall(DungeonData data, Vector2D startPoint, Vector2D endPoint)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		calculated = false;
		SineHall(data, startPoint, endPoint);
		calculated = true;
	}

	public override void GenerateHall(DungeonData data)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		generated = false;
		SineHall(data, StartPosition, EndPosition, generating: true);
		generated = true;
	}

	public void SineHall(DungeonData data, Vector2D startPoint, Vector2D endPoint, bool generating = false)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		SineDungeonHallSettings sineDungeonHallSettings = (SineDungeonHallSettings)settings;
		UnifiedRandom unifiedRandom = new UnifiedRandom(sineDungeonHallSettings.RandomSeed);
		ushort brickTileType = settings.StyleData.BrickTileType;
		ushort brickCrackedTileType = settings.StyleData.BrickCrackedTileType;
		ushort brickWallType = settings.StyleData.BrickWallType;
		Vector2D val = startPoint;
		bool flag = false;
		if (sineDungeonHallSettings.CrackedBrickChance > 0.0)
		{
			flag = unifiedRandom.NextDouble() <= sineDungeonHallSettings.CrackedBrickChance;
		}
		int num = 3;
		int num2 = 8;
		int num3 = num + num2;
		Vector2D v = endPoint - startPoint;
		Vector2D val2 = v.SafeNormalize(Vector2D.UnitX);
		int num4 = (int)Math.Ceiling(((Vector2D)(ref v)).Length() / ((Vector2D)(ref val2)).Length());
		int num5 = num4;
		Bounds.SetBounds((int)startPoint.X, (int)startPoint.Y, (int)startPoint.X, (int)startPoint.Y);
		DungeonRoomSearchSettings dungeonRoomSearchSettings = new DungeonRoomSearchSettings
		{
			Fluff = (int)((float)num * sineDungeonHallSettings.Magnitude) + num2
		};
		List<DungeonRoom> allRoomsInSpots = DungeonUtils.GetAllRoomsInSpots(data.dungeonRooms, startPoint, endPoint, dungeonRoomSearchSettings);
		Vector2D val3 = val2;
		Vector2D v2 = (Vector2.UnitY * sineDungeonHallSettings.Magnitude).ToVector2D();
		v2 = v2.ToVector2().RotatedBy(val3.ToRotation(), Vector2.Zero).ToVector2D();
		float num6 = 0f;
		float num7 = 0f;
		float num8 = (float)sineDungeonHallSettings.Iterations / (float)num5 * ((float)Math.PI * 2f);
		while (num4 > 0)
		{
			Vector2D val4 = v2 * (double)(float)Math.Sin(num7);
			Vector2D val5 = (sineDungeonHallSettings.FlipSine ? (val - val4) : (val + val4));
			if (!WorldGen.InWorld((int)(val5.X + val2.X), (int)(val5.Y + val2.Y), 10))
			{
				break;
			}
			if (!base.Processed)
			{
				data.dungeonBounds.UpdateBounds((int)val5.X - num3, (int)val5.Y - num3, (int)val5.Y + num3, (int)val5.Y + num3);
				Bounds.UpdateBounds((int)val5.X - num3, (int)val5.Y - num3, (int)val5.Y + num3, (int)val5.Y + num3);
			}
			if (generating)
			{
				GenerateDungeonSquareHall(data, allRoomsInSpots, val5, brickTileType, brickCrackedTileType, brickWallType, num, num2, sineDungeonHallSettings.PlaceOverProtectedBricks, flag);
			}
			val += val2;
			num6 += num8;
			num7 += num8;
			if (num6 >= 0.5f)
			{
				num6 = 0f;
				PotentialPlatformPoints.Add(new Tuple<Vector2D, Vector2D>(val, val4));
			}
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
