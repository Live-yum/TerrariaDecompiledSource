using System;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;

namespace Terraria.GameContent.LeashedEntities;

internal class JumperLeashedCritter : LeashedCritter
{
	public static JumperLeashedCritter Prototype = new JumperLeashedCritter();

	private const int State_Normal = 0;

	private const int State_Recalling = 1;

	protected int minWaitTime;

	protected int maxWaitTime;

	protected float maxJumpWidth;

	protected float minJumpWidth;

	protected float maxJumpHeight;

	protected float maxJumpDuration;

	protected int jumpCooldown;

	protected bool canStandOnWater;

	public JumperLeashedCritter()
	{
		strayingRangeInBlocks = 12;
		minWaitTime = 180;
		maxWaitTime = 300;
		maxJumpWidth = 112f;
		minJumpWidth = 48f;
		maxJumpHeight = 64f;
		maxJumpDuration = 30f;
		jumpCooldown = 60;
		canStandOnWater = false;
	}

	public override void Spawn(bool newlyAdded)
	{
		base.Spawn(newlyAdded);
		PickNewTarget();
	}

	public override void Update()
	{
		base.Update();
		WaitTime--;
		if (WaitTime <= 0)
		{
			switch (State)
			{
			case 0:
				if (!TryStartJump())
				{
					PickNewTarget();
					SetJumpCooldown();
				}
				break;
			case 1:
				Recall();
				PickNewTarget();
				SetJumpCooldown();
				State = 0;
				break;
			}
		}
		Move(out var hitSomething);
		if (hitSomething && State != 1)
		{
			PickNewTarget();
			SetJumpCooldown();
		}
		if ((TargetPosition.ToWorldCoordinates() - base.Center).Length() < 8f)
		{
			base.Center = TargetPosition.ToWorldCoordinates();
			velocity = Vector2.Zero;
			PickNewTarget();
			SetJumpCooldown();
		}
		spriteDirection = direction;
		if (Main.netMode != 2)
		{
			VisualEffects();
		}
		CopyToDummy();
		LeashedCritter._dummy.FindFrame();
		CopyFromDummy();
	}

	private void SetJumpCooldown()
	{
		WaitTime = (short)rand.Next(minWaitTime, maxWaitTime + 1);
	}

	private bool TryStartJump()
	{
		Vector2 vector = TargetPosition.ToWorldCoordinates() - base.Center;
		if (vector.Y * -1f > maxJumpHeight)
		{
			return false;
		}
		float num = Math.Min(Math.Abs(vector.X), maxJumpWidth);
		if (num <= minJumpWidth)
		{
			return false;
		}
		direction = Math.Sign(vector.X);
		float num2 = num / maxJumpWidth;
		float num3 = maxJumpDuration * num2;
		velocity.X = num / num3 * (float)direction;
		velocity.Y = vector.Y * num2 / num3 - 0.5f * LeashedCritter.gravity * num3;
		if (velocity.Y >= 0f)
		{
			return false;
		}
		WaitTime = (short)(num3 + (float)jumpCooldown);
		return true;
	}

	private void Move(out bool hitSomething)
	{
		hitSomething = false;
		Point point = base.Center.ToTileCoordinates();
		int num = Math.Sign((int)velocity.X);
		if (num != 0)
		{
			direction = num;
		}
		int num2 = Math.Sign((int)velocity.Y);
		Vector2 vector = new Vector2(num, num2) * base.Size * 0.5f;
		Vector2 vec = base.Center + vector + velocity;
		if (!WorldGen.SolidTile2(vec.ToTileCoordinates()))
		{
			Move_NoObstruction(point, vec.Y);
			return;
		}
		hitSomething = true;
		bool flag = false;
		if (num2 != 0)
		{
			Point p = point;
			p.Y += num2;
			flag = WorldGen.SolidTile2(p);
		}
		bool flag2 = false;
		if (num != 0)
		{
			Point p2 = point;
			p2.X += num;
			flag2 = WorldGen.SolidTile2(p2);
		}
		if (flag)
		{
			velocity.Y = 0f;
		}
		if (flag2)
		{
			velocity.X = 0f;
		}
		if (!flag && !flag2)
		{
			velocity = Vector2.Zero;
		}
	}

	private void Move_NoObstruction(Point currentTile, float nextY)
	{
		if (velocity.Y >= 0f && nextY % 16f >= 8f)
		{
			Point p = currentTile;
			p.Y++;
			if (WorldGen.SolidTile2(p) || (canStandOnWater && WorldGen.AnyLiquidAt(p.X, p.Y, 0)))
			{
				base.Center = currentTile.ToWorldCoordinates();
				velocity = Vector2.Zero;
				return;
			}
		}
		base.Center += velocity;
		velocity.Y += LeashedCritter.gravity;
		if (velocity.Y > LeashedCritter.maxFallSpeed)
		{
			velocity.Y = LeashedCritter.maxFallSpeed;
		}
		if (State != 1 && currentTile.Y - base.AnchorPosition.Y > strayingRangeInBlocks)
		{
			State = 1;
			WaitTime = 20;
		}
	}

	private void PickNewTarget()
	{
		int num = (int)(maxJumpWidth / 16f);
		int num2 = (int)(minJumpWidth / 16f);
		int num3 = TargetPosition.X - (base.AnchorPosition.X - strayingRangeInBlocks);
		int num4 = base.AnchorPosition.X + strayingRangeInBlocks - TargetPosition.X;
		bool flag = num3 >= num2;
		bool flag2 = num4 >= num2;
		if (flag || flag2)
		{
			int num5 = ((!(flag && flag2)) ? ((!flag) ? 1 : (-1)) : (rand.Next(2) * 2 - 1));
			int num6 = ((num5 < 1) ? num3 : num4);
			int num7 = rand.Next(1, num6 / num + 1);
			int num8 = num6 % num;
			if (num8 < num2)
			{
				num8 = 0;
			}
			int startX = TargetPosition.X + (num7 * num + num8) * num5;
			if (TryGetReachableTile(startX, out var tile))
			{
				TargetPosition = tile;
			}
		}
	}

	private bool TryGetReachableTile(int startX, out Point16 tile)
	{
		tile = Point16.Zero;
		int num = Math.Sign(base.AnchorPosition.X - startX);
		if (num == 0)
		{
			return false;
		}
		for (int i = startX; i != base.AnchorPosition.X; i += num)
		{
			tile = new Point16(i, base.AnchorPosition.Y);
			if (WorldGen.SolidTile2(tile))
			{
				float num2 = maxJumpHeight / 16f;
				for (int j = 0; (float)j < num2; j++)
				{
					tile.Y--;
					if (!WorldGen.SolidTile2(tile))
					{
						return true;
					}
				}
				continue;
			}
			for (int k = 0; k < strayingRangeInBlocks; k++)
			{
				tile.Y++;
				if (WorldGen.SolidTile2(tile) || (canStandOnWater && WorldGen.AnyLiquidAt(tile.X, tile.Y, 0)))
				{
					tile.Y--;
					return true;
				}
			}
		}
		return false;
	}

	protected override void CopyToDummy()
	{
		base.CopyToDummy();
		if (State == 1)
		{
			LeashedCritter._dummy.Opacity = (float)WaitTime / 20f;
		}
	}

	public override Vector2 GetDrawOffset()
	{
		Point16 point = base.Center.ToTileCoordinates16();
		if (Framing.GetTileSafely(point.X, point.Y + 1).halfBrick())
		{
			return new Vector2(0f, base.Center.Y % 16f);
		}
		return base.GetDrawOffset();
	}
}
