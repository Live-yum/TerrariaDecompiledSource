using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria.Utilities;

namespace Terraria.GameContent.Generation.Dungeon.Rooms;

public class RegularDungeonRoom : DungeonRoom
{
	public int _innerBoundsSize;

	public RegularDungeonRoom(DungeonRoomSettings settings)
		: base(settings)
	{
	}

	public override void CalculateRoom(DungeonData data)
	{
		calculated = false;
		int x = settings.RoomPosition.X;
		int y = settings.RoomPosition.Y;
		RegularRoom(data, x, y, generating: false);
		calculated = true;
	}

	public override bool GenerateRoom(DungeonData data)
	{
		generated = false;
		int x = settings.RoomPosition.X;
		int y = settings.RoomPosition.Y;
		RegularRoom(data, x, y, generating: true);
		generated = true;
		return true;
	}

	public void RegularRoom(DungeonData data, int i, int j, bool generating)
	{
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		UnifiedRandom unifiedRandom = new UnifiedRandom(settings.RandomSeed);
		RegularDungeonRoomSettings regularDungeonRoomSettings = (RegularDungeonRoomSettings)settings;
		ushort brickTileType = settings.StyleData.BrickTileType;
		ushort brickWallType = settings.StyleData.BrickWallType;
		Point point = new Point(i, j);
		if (base.Processed)
		{
			point = InnerBounds.Center;
		}
		int num = 6 + unifiedRandom.Next(7);
		int num2 = 8;
		if (regularDungeonRoomSettings.OverrideInnerBoundsSize > 0)
		{
			num = regularDungeonRoomSettings.OverrideInnerBoundsSize;
		}
		if (regularDungeonRoomSettings.OverrideOuterBoundsSize > 0)
		{
			num2 = regularDungeonRoomSettings.OverrideOuterBoundsSize;
		}
		if (base.Processed)
		{
			num = _innerBoundsSize;
		}
		int num3 = num + num2;
		InnerBounds.SetBounds(point.X, point.Y, point.X, point.Y);
		OuterBounds.SetBounds(point.X, point.Y, point.X, point.Y);
		OuterBounds.UpdateBounds(point.X - num3, point.Y - num3, point.X + num3, point.Y + num3);
		InnerBounds.UpdateBounds(OuterBounds.Left + num2, OuterBounds.Top + num2, OuterBounds.Right - num2, OuterBounds.Bottom - num2);
		GenerateDungeonSquareRoom(data, InnerBounds, OuterBounds, Vector2D.op_Implicit(point), brickTileType, brickWallType, num, num3, generating, generating);
		_innerBoundsSize = num;
		InnerBounds.CalculateHitbox();
		OuterBounds.CalculateHitbox();
	}
}
