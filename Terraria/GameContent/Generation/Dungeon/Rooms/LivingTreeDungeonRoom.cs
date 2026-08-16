using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace Terraria.GameContent.Generation.Dungeon.Rooms;

public class LivingTreeDungeonRoom : DungeonRoom
{
	private ShapeData _innerShapeData = new ShapeData();

	private ShapeData _outerShapeData = new ShapeData();

	private int _floodedTileCount;

	private Point BasePosition;

	public LivingTreeDungeonRoom(DungeonRoomSettings settings)
		: base(settings)
	{
	}

	public override void CalculateRoom(DungeonData data)
	{
		calculated = false;
		int x = settings.RoomPosition.X;
		int y = settings.RoomPosition.Y;
		LivingTreeRoom(data, x, y, generating: false);
		calculated = true;
	}

	public override bool GenerateRoom(DungeonData data)
	{
		generated = false;
		int x = settings.RoomPosition.X;
		int y = settings.RoomPosition.Y;
		LivingTreeRoom(data, x, y, generating: true);
		generated = true;
		return true;
	}

	public override int GetFloodedRoomTileCount()
	{
		return _floodedTileCount;
	}

	public override void FloodRoom(byte liquidType)
	{
		if (_innerShapeData == null)
		{
			base.FloodRoom(liquidType);
			return;
		}
		_ = (WormlikeDungeonRoomSettings)settings;
		WorldUtils.Gen(BasePosition, new ModShapes.All(_innerShapeData), Actions.Chain(new Modifiers.IsBelowHeight(base.Center.Y, inclusive: true), new Modifiers.IsNotSolid(), new Actions.SetLiquid(liquidType)));
	}

	public override ProtectionType GetProtectionTypeFromPoint(int x, int y)
	{
		if (_innerShapeData == null || _outerShapeData == null || (calculated && !OuterBounds.Contains(x, y)))
		{
			return base.GetProtectionTypeFromPoint(x, y);
		}
		Point basePosition = BasePosition;
		if (!_outerShapeData.Contains(x - basePosition.X, y - basePosition.Y))
		{
			return ProtectionType.None;
		}
		return ProtectionType.Walls;
	}

	public override bool IsInsideRoom(int x, int y)
	{
		Point basePosition = BasePosition;
		if (base.IsInsideRoom(x, y))
		{
			return _innerShapeData.Contains(x - basePosition.X, y - basePosition.Y);
		}
		return false;
	}

	public override void GenerateEarlyDungeonFeaturesInRoom(DungeonData data)
	{
		UnifiedRandom unifiedRandom = new UnifiedRandom(settings.RandomSeed);
		ushort brickTileType = settings.StyleData.BrickTileType;
		ushort brickCrackedTileType = settings.StyleData.BrickCrackedTileType;
		int growthLength = (int)((float)InnerBounds.Height * 0.1f) + unifiedRandom.Next(4);
		int branchDensity = 2 + unifiedRandom.Next(2);
		int leafDensity = 3 + unifiedRandom.Next(4);
		DungeonUtils.GenerateHangingLeafCluster(startPoint: new Point(InnerBounds.Center.X, InnerBounds.Top), data: data, genRand: unifiedRandom, bounds: OuterBounds, growthLength: growthLength, branchDensity: branchDensity, leafDensity: leafDensity, leafType: brickCrackedTileType, woodType: brickTileType, leafPaintColor: settings.OverridePaintTile, woodPaintColor: settings.OverridePaintTile);
		growthLength = (int)((float)InnerBounds.Height * 0.15f) + unifiedRandom.Next(5);
		branchDensity = 3 + unifiedRandom.Next(2);
		leafDensity = 4 + unifiedRandom.Next(4);
		DungeonUtils.GenerateHangingLeafCluster(startPoint: new Point(InnerBounds.Left + 2 + unifiedRandom.Next(3), InnerBounds.Top), data: data, genRand: unifiedRandom, bounds: OuterBounds, growthLength: growthLength, branchDensity: branchDensity, leafDensity: leafDensity, leafType: brickCrackedTileType, woodType: brickTileType, leafPaintColor: settings.OverridePaintTile, woodPaintColor: settings.OverridePaintTile);
		growthLength = (int)((float)InnerBounds.Height * 0.15f) + unifiedRandom.Next(5);
		branchDensity = 3 + unifiedRandom.Next(2);
		leafDensity = 4 + unifiedRandom.Next(4);
		DungeonUtils.GenerateHangingLeafCluster(startPoint: new Point(InnerBounds.Right - 2 - unifiedRandom.Next(3), InnerBounds.Top), data: data, genRand: unifiedRandom, bounds: OuterBounds, growthLength: growthLength, branchDensity: branchDensity, leafDensity: leafDensity, leafType: brickCrackedTileType, woodType: brickTileType, leafPaintColor: settings.OverridePaintTile, woodPaintColor: settings.OverridePaintTile);
		base.GenerateEarlyDungeonFeaturesInRoom(data);
	}

	public override void GenerateLateDungeonFeaturesInRoom(DungeonData data)
	{
		UnifiedRandom unifiedRandom = new UnifiedRandom(settings.RandomSeed);
		LivingTreeDungeonRoomSettings livingTreeDungeonRoomSettings = (LivingTreeDungeonRoomSettings)settings;
		ushort brickTileType = settings.StyleData.BrickTileType;
		ushort brickCrackedTileType = settings.StyleData.BrickCrackedTileType;
		ushort brickWallType = settings.StyleData.BrickWallType;
		for (int i = 0; i < 50; i++)
		{
			int x = unifiedRandom.Next(InnerBounds.Left + 1, InnerBounds.Right);
			int y = unifiedRandom.Next(InnerBounds.Top + 1, InnerBounds.Bottom);
			Point point = DungeonUtils.FirstSolid(ceiling: false, new Point(x, y), InnerBounds);
			x = point.X;
			y = point.Y - 1;
			Tile tile = Main.tile[x, y];
			if (tile.active() || tile.wall != brickWallType)
			{
				continue;
			}
			if (unifiedRandom.Next(2) == 0)
			{
				WorldGen.PlaceTile(x, y, 187, mute: true, forced: false, -1, unifiedRandom.Next(47, 50));
				continue;
			}
			int num = unifiedRandom.Next(2);
			int pileStyle = 72;
			if (num == 1)
			{
				pileStyle = unifiedRandom.Next(59, 62);
			}
			WorldGen.PlaceSmallPile(x, y, pileStyle, num, 185);
		}
		for (int j = 0; j < 10; j++)
		{
			int x2 = unifiedRandom.Next(InnerBounds.Left + 1, InnerBounds.Right);
			int y2 = unifiedRandom.Next(InnerBounds.Top + 1, InnerBounds.Bottom);
			Point point2 = DungeonUtils.FirstSolid(ceiling: true, new Point(x2, y2), InnerBounds);
			x2 = point2.X;
			y2 = point2.Y + 1;
			Tile tile2 = Main.tile[x2, y2];
			Tile tile3 = Main.tile[x2, y2 - 1];
			if (tile2.active() || tile2.wall != brickWallType || !tile3.active() || tile3.type != brickCrackedTileType)
			{
				continue;
			}
			ushort type = 52;
			if (brickTileType == 383)
			{
				type = 62;
			}
			for (int num2 = unifiedRandom.Next(3, 12); num2 > 0; num2--)
			{
				Tile tile4 = Main.tile[x2, y2];
				if (tile4.active())
				{
					break;
				}
				tile4.ClearTile();
				tile4.active(active: true);
				tile4.type = type;
				if (livingTreeDungeonRoomSettings.OverridePaintTile > -1)
				{
					WorldGen.paintTile(x2, y2, (byte)livingTreeDungeonRoomSettings.OverridePaintTile, broadCast: false, paintEffects: false);
				}
				y2++;
			}
		}
	}

	public void LivingTreeRoom(DungeonData data, int i, int j, bool generating)
	{
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		UnifiedRandom unifiedRandom = new UnifiedRandom(settings.RandomSeed);
		LivingTreeDungeonRoomSettings livingTreeDungeonRoomSettings = (LivingTreeDungeonRoomSettings)settings;
		ushort brickTileType = settings.StyleData.BrickTileType;
		ushort brickCrackedTileType = settings.StyleData.BrickCrackedTileType;
		ushort brickWallType = settings.StyleData.BrickWallType;
		Point basePosition = new Point(i, j);
		if (calculated)
		{
			basePosition = BasePosition;
		}
		Point point = new Point(basePosition.X, basePosition.Y + livingTreeDungeonRoomSettings.InnerHeight / 2);
		int num = point.Y - livingTreeDungeonRoomSettings.InnerHeight;
		int innerWidth = livingTreeDungeonRoomSettings.InnerWidth;
		int depth = livingTreeDungeonRoomSettings.Depth;
		int num2 = innerWidth;
		int num3 = num2 + depth;
		OuterBounds.SetBounds(basePosition.X, basePosition.Y, basePosition.X, basePosition.Y);
		InnerBounds.SetBounds(basePosition.X, basePosition.Y, basePosition.X, basePosition.Y);
		while (point.Y > num)
		{
			OuterBounds.UpdateBounds(point.X - num3, point.Y - num3, point.X + num3, point.Y + num3);
			InnerBounds.UpdateBounds(point.X - num2, point.Y - num2, point.X + num2, point.Y + num2);
			_outerShapeData.AddBounds(point.X - num3 - basePosition.X, point.Y - num3 - basePosition.Y, point.X + num3 - basePosition.X, point.Y + num3 - basePosition.Y);
			_innerShapeData.AddBounds(point.X - num2 - basePosition.X, point.Y - num2 - basePosition.Y, point.X + num2 - basePosition.X, point.Y + num2 - basePosition.Y);
			if (generating)
			{
				GenerateDungeonSquareRoom(data, Vector2D.op_Implicit(point), brickTileType, brickCrackedTileType, brickWallType, livingTreeDungeonRoomSettings.InnerWidth, livingTreeDungeonRoomSettings.Depth);
			}
			if (point.Y % 4 == 0)
			{
				point.X += ((unifiedRandom.Next(2) != 0) ? 1 : (-1));
			}
			point.Y--;
		}
		InnerBounds.CalculateHitbox();
		OuterBounds.CalculateHitbox();
		BasePosition = basePosition;
		_floodedTileCount = DungeonUtils.CalculateFloodedTileCountFromShapeData(InnerBounds, _innerShapeData);
	}
}
