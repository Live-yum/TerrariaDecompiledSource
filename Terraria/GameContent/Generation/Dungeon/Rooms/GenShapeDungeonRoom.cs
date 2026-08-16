using System;
using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria.GameContent.Generation.Dungeon.Features;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace Terraria.GameContent.Generation.Dungeon.Rooms;

public class GenShapeDungeonRoom : DungeonRoom
{
	private ShapeData _innerShapeData = new ShapeData();

	private ShapeData _outerShapeData = new ShapeData();

	private int _floodedTileCount;

	public GenShapeDungeonRoom(DungeonRoomSettings settings)
		: base(settings)
	{
		_ = (GenShapeDungeonRoomSettings)settings;
	}

	public override void CalculateRoom(DungeonData data)
	{
		calculated = false;
		int x = settings.RoomPosition.X;
		int y = settings.RoomPosition.Y;
		GenShapeRoom(data, x, y, generating: false);
		calculated = true;
	}

	public override bool GenerateRoom(DungeonData data)
	{
		generated = false;
		int x = settings.RoomPosition.X;
		int y = settings.RoomPosition.Y;
		GenShapeRoom(data, x, y, generating: true);
		generated = true;
		return true;
	}

	public override bool CanGenerateFeatureAt(DungeonData data, IDungeonFeature feature, int x, int y)
	{
		GenShapeType shapeType = ((GenShapeDungeonRoomSettings)settings).ShapeType;
		if ((uint)(shapeType - 2) <= 1u && feature is DungeonWindow)
		{
			return false;
		}
		return base.CanGenerateFeatureAt(data, feature, x, y);
	}

	public override void GenerateEarlyDungeonFeaturesInRoom(DungeonData data)
	{
		GenShapeDungeonRoomSettings genShapeDungeonRoomSettings = (GenShapeDungeonRoomSettings)settings;
		if (genShapeDungeonRoomSettings.ShapeType == GenShapeType.Doughnut)
		{
			DungeonShapes.CircleRoom circleRoom = (DungeonShapes.CircleRoom)genShapeDungeonRoomSettings.InnerShape;
			GenAction genAction = new Actions.Blank();
			if (genShapeDungeonRoomSettings.OverridePaintTile == 0)
			{
				genAction = new Actions.ClearTilePaint();
			}
			else if (genShapeDungeonRoomSettings.OverridePaintTile > 0)
			{
				genAction = new Actions.SetTilePaint((byte)genShapeDungeonRoomSettings.OverridePaintTile);
			}
			DungeonShapes.CircleRoom shape = new DungeonShapes.CircleRoom(Math.Max(1, circleRoom.HorizontalRadius / 2), Math.Max(1, circleRoom.VerticalRadius / 2));
			WorldUtils.Gen(InnerBounds.Center, shape, Actions.Chain(new Actions.ClearTile(), new Actions.SetTile(genShapeDungeonRoomSettings.StyleData.BrickTileType, setSelfFrames: false, setNeighborFrames: false, clearTile: false), genAction));
		}
		base.GenerateEarlyDungeonFeaturesInRoom(data);
	}

	public override Point GetRoomCenterForDungeonFeature(DungeonData data, DungeonFeature feature)
	{
		GenShapeDungeonRoomSettings genShapeDungeonRoomSettings = (GenShapeDungeonRoomSettings)settings;
		Point roomCenterForDungeonFeature = base.GetRoomCenterForDungeonFeature(data, feature);
		if (feature is DungeonWindow && genShapeDungeonRoomSettings.ShapeType == GenShapeType.Mound)
		{
			roomCenterForDungeonFeature.Y += InnerBounds.Height / 5;
		}
		return roomCenterForDungeonFeature;
	}

	public override Point GetRoomCenterForHallway(Vector2D otherRoomPos)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		GenShapeDungeonRoomSettings obj = (GenShapeDungeonRoomSettings)settings;
		Vector2D val = base.GetRoomCenterForHallway(otherRoomPos).ToVector2D();
		Vector2D unitX = Vector2D.UnitX;
		DungeonRoomType roomType = obj.RoomType;
		if (roomType != DungeonRoomType.GenShapeDoughnut)
		{
			return val.ToPoint();
		}
		unitX = (otherRoomPos - val).SafeNormalize(Vector2D.UnitX);
		Point result = (val + unitX * (double)(InnerBounds.Size / 2)).ToPoint();
		result.X = (int)(val.X + unitX.X * (double)(InnerBounds.Width / 2));
		result.Y = (int)(val.Y + unitX.Y * (double)(InnerBounds.Height / 2));
		return result;
	}

	public override int GetFloodedRoomTileCount()
	{
		return _floodedTileCount;
	}

	public override void FloodRoom(byte liquidType)
	{
		GenShapeDungeonRoomSettings genShapeDungeonRoomSettings = (GenShapeDungeonRoomSettings)settings;
		WorldUtils.Gen(InnerBounds.Center, genShapeDungeonRoomSettings.InnerShape, Actions.Chain(new Modifiers.IsBelowHeight(InnerBounds.Center.Y, inclusive: true), new Modifiers.IsNotSolid(), new Actions.SetLiquid(liquidType)));
	}

	public override ProtectionType GetProtectionTypeFromPoint(int x, int y)
	{
		if (_innerShapeData == null || _outerShapeData == null || (calculated && !OuterBounds.Contains(x, y)))
		{
			return base.GetProtectionTypeFromPoint(x, y);
		}
		if (!_outerShapeData.Contains(x - base.Center.X, y - base.Center.Y))
		{
			return ProtectionType.None;
		}
		return ProtectionType.Walls;
	}

	public override bool IsInsideRoom(int x, int y)
	{
		if (base.IsInsideRoom(x, y))
		{
			return _innerShapeData.Contains(x - base.Center.X, y - base.Center.Y);
		}
		return false;
	}

	public void GenShapeRoom(DungeonData data, int i, int j, bool generating)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		new UnifiedRandom(settings.RandomSeed);
		GenShapeDungeonRoomSettings genShapeDungeonRoomSettings = (GenShapeDungeonRoomSettings)settings;
		ushort brickTileType = settings.StyleData.BrickTileType;
		ushort brickWallType = settings.StyleData.BrickWallType;
		Vector2D val = default(Vector2D);
		((Vector2D)(ref val))._002Ector((double)i, (double)j);
		if (base.Processed)
		{
			val = Vector2D.op_Implicit(base.Center);
		}
		InnerBounds.SetBounds((int)val.X, (int)val.Y, (int)val.X, (int)val.Y);
		OuterBounds.SetBounds((int)val.X, (int)val.Y, (int)val.X, (int)val.Y);
		GenAction genAction = new Actions.Blank();
		if (genShapeDungeonRoomSettings.OverridePaintTile == 0)
		{
			genAction = new Actions.ClearTilePaint();
		}
		else if (genShapeDungeonRoomSettings.OverridePaintTile > 0)
		{
			genAction = new Actions.SetTilePaint((byte)genShapeDungeonRoomSettings.OverridePaintTile);
		}
		GenAction genAction2 = new Actions.Blank();
		if (genShapeDungeonRoomSettings.OverridePaintWall == 0)
		{
			genAction2 = new Actions.ClearWallPaint();
		}
		else if (genShapeDungeonRoomSettings.OverridePaintWall > 0)
		{
			genAction2 = new Actions.SetWallPaint((byte)genShapeDungeonRoomSettings.OverridePaintWall);
		}
		WorldUtils.Gen(val.ToPoint(), genShapeDungeonRoomSettings.OuterShape, Actions.Chain(new Modifiers.Expand(1), new Actions.UpdateBounds(data.dungeonBounds).Output(_outerShapeData), new Actions.UpdateBounds(OuterBounds), new Modifiers.Conditions(new Conditions.BoolCheck(generating)), new Actions.ClearTile(), new Actions.SetTile(brickTileType, setSelfFrames: false, setNeighborFrames: false, clearTile: false), genAction));
		if (generating)
		{
			WorldUtils.Gen(val.ToPoint(), genShapeDungeonRoomSettings.OuterShape, Actions.Chain(new Actions.SetWall(brickWallType, setSelfFrames: false, setNeighborFrames: false, clearTile: false), genAction2));
		}
		WorldUtils.Gen(val.ToPoint(), genShapeDungeonRoomSettings.InnerShape, Actions.Chain(new Actions.UpdateBounds(data.dungeonBounds).Output(_innerShapeData), new Actions.UpdateBounds(InnerBounds), new Modifiers.Conditions(new Conditions.BoolCheck(generating)), new Actions.Clear(), new Actions.SetWall(brickWallType, setSelfFrames: false, setNeighborFrames: false, clearTile: false), genAction2));
		InnerBounds.CalculateHitbox();
		OuterBounds.CalculateHitbox();
		_floodedTileCount = DungeonUtils.CalculateFloodedTileCountFromShapeData(InnerBounds, _innerShapeData);
	}
}
