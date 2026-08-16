using System;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.ID;

namespace Terraria.GameContent.LeashedEntities;

public class LeashedKite : LeashedEntity
{
	public static LeashedKite Prototype;

	private static Projectile _dummy = new Projectile();

	public int projType;

	public int frame;

	public int frameCounter;

	public float rotation;

	public int spriteDirection = 1;

	public float kiteDistance = 250f;

	public float windTarget;

	public float windCurrent;

	public float timeCounter;

	public float cloudAlpha;

	public int timeWithoutWind;

	public float projectileLocalAI0;

	public float projectileLocalAI1;

	public Vector2[] oldPos;

	public float[] oldRot;

	public int[] oldSpriteDirection;

	public Vector2 netOffset;

	private Vector2 AnchorWorldPosition => base.AnchorPosition.ToWorldCoordinates();

	public void SetDefaults(int projType)
	{
		this.projType = projType;
		_dummy.SetDefaults(projType);
		base.Size = _dummy.Size;
	}

	public override void NetSend(BinaryWriter writer, bool full)
	{
		if (full)
		{
			Utils.Write7BitEncodedInt(writer, projType);
		}
		writer.WriteVector2(position);
		writer.WritePackedVector2(velocity);
		writer.Write((byte)((double)(rotation * 256f) / (Math.PI * 2.0)));
		writer.Write(windTarget);
		writer.Write(cloudAlpha);
		writer.Write(timeCounter);
	}

	public override void NetReceive(BinaryReader reader, bool full)
	{
		if (full)
		{
			SetDefaults(Utils.Read7BitEncodedInt(reader));
		}
		Vector2 vector = position;
		position = reader.ReadVector2();
		velocity = reader.ReadPackedVector2();
		rotation = (float)((double)(int)reader.ReadByte() * Math.PI * 2.0 / 256.0);
		windTarget = reader.ReadSingle();
		cloudAlpha = reader.ReadSingle();
		timeCounter = reader.ReadSingle();
		if (full)
		{
			netOffset = Vector2.Zero;
		}
		else
		{
			netOffset += vector - position;
		}
		if (full)
		{
			Update();
		}
	}

	public override void Draw()
	{
		Main.instance.LoadProjectile(projType);
		CopyToDummy();
		_dummy.position += netOffset;
		Main.DrawKite(_dummy, AnchorWorldPosition);
	}

	public override void Update()
	{
		Update(fastForward: false);
	}

	public void Update(bool fastForward)
	{
		if (oldPos == null)
		{
			int num = ProjectileID.Sets.TrailCacheLength[projType];
			oldPos = new Vector2[num];
			oldRot = new float[num];
			oldSpriteDirection = new int[num];
		}
		if (NearbySectionsMissing())
		{
			return;
		}
		if (fastForward || Vector2.DistanceSquared(position, oldPos[0]) > 256f)
		{
			for (int i = 0; i < oldPos.Length; i++)
			{
				oldPos[i] = position;
				oldRot[i] = rotation;
				oldSpriteDirection[i] = spriteDirection;
			}
		}
		if (Main.netMode != 1)
		{
			windTarget = Main.WindForVisuals;
			cloudAlpha = Main.cloudAlpha;
		}
		windCurrent = (fastForward ? windTarget : MathHelper.Lerp(windCurrent, windTarget, 0.05f));
		bool flag = Math.Abs(windCurrent) >= 0.2f;
		timeWithoutWind = ((!flag) ? (fastForward ? 3600 : (timeWithoutWind + 1)) : 0);
		kiteDistance = Utils.Remap(timeWithoutWind, 120f, 420f, 250f, 48f);
		MoveKite(fastForward);
		netOffset = netOffset.MoveTowards(Vector2.Zero, 2f);
	}

	private void MoveKite(bool fastForward = false)
	{
		CopyToDummy();
		_dummy.owner = 255;
		Player player = Main.player[255];
		Vector2 vector = (player.Center = AnchorWorldPosition);
		if (timeWithoutWind == 0)
		{
			int num = ((!(_dummy.Center.X - vector.X < 0f)) ? 1 : (-1));
			_dummy.spriteDirection = num;
			player.direction = num;
		}
		timeCounter += 1f / 60f;
		KiteFlyingInfo info = new KiteFlyingInfo
		{
			BobOffset = (vector.X + vector.Y * 0.92f) * 0.0025f,
			WindInWorld = windCurrent,
			CloudAlpha = cloudAlpha,
			GlobalTime = timeCounter,
			CanReelThroughBlocks = false
		};
		if (fastForward)
		{
			_dummy.KiteLogic(vector, info);
			timeCounter = 6f;
			Vector2 wetVelocity = new Vector2(info.WindInWorld, -2f).SafeNormalize(Vector2.Zero) * kiteDistance;
			Vector2 targetPosition = _dummy.position;
			_dummy.velocity = wetVelocity;
			_dummy.HandleMovement(wetVelocity);
			_dummy.position = _dummy.position.MoveTowards(targetPosition, 1f);
			if (_dummy.velocity.Length() > 4f)
			{
				_dummy.velocity = _dummy.velocity.SafeNormalize(Vector2.Zero) * 4f;
			}
			_dummy.KiteLogic(vector, info);
			for (int num2 = oldPos.Length - 1; num2 >= 0; num2--)
			{
				oldPos[num2] = _dummy.position;
				oldRot[num2] = _dummy.rotation;
				oldSpriteDirection[num2] = _dummy.spriteDirection;
			}
		}
		else
		{
			Utils.Shift(oldPos, 1);
			Utils.Shift(oldRot, 1);
			Utils.Shift(oldSpriteDirection, 1);
			oldPos[0] = position;
			oldRot[0] = rotation;
			oldSpriteDirection[0] = spriteDirection;
			_dummy.KiteLogic(vector, info);
			_dummy.HandleMovement(_dummy.velocity);
			_dummy.GetCollisionParams(out var resizeAnchor, out var colWidth, out var colHeight);
			if (Collision.SolidFullTiles(_dummy.position + _dummy.Size / 2f - new Vector2(colWidth, colHeight) * resizeAnchor, new Vector2(colWidth, colHeight)))
			{
				_dummy.Bottom = _dummy.Bottom.MoveTowards(vector, 2f);
			}
		}
		CopyFromDummy();
	}

	public override void Spawn(bool newlyAdded)
	{
		base.Center = AnchorWorldPosition;
		velocity = new Vector2(0f, -5f);
		Update(!newlyAdded);
		windCurrent = (windTarget = Main.WindForVisuals);
		cloudAlpha = Main.cloudAlpha;
	}

	private void CopyToDummy()
	{
		_dummy.type = projType;
		_dummy.Size = base.Size;
		_dummy.frame = frame;
		_dummy.frameCounter = frameCounter;
		_dummy.position = position;
		_dummy.velocity = velocity;
		_dummy.rotation = rotation;
		_dummy.spriteDirection = spriteDirection;
		_dummy.oldPos = oldPos;
		_dummy.oldRot = oldRot;
		_dummy.oldSpriteDirection = oldSpriteDirection;
		_dummy.scale = 1f;
		_dummy.ai[0] = kiteDistance;
		_dummy.localAI[0] = projectileLocalAI0;
		_dummy.localAI[1] = projectileLocalAI1;
		_dummy.extraUpdates = 0;
	}

	private void CopyFromDummy()
	{
		frame = _dummy.frame;
		frameCounter = _dummy.frameCounter;
		position = _dummy.position;
		velocity = _dummy.velocity;
		rotation = _dummy.rotation;
		spriteDirection = _dummy.spriteDirection;
		projectileLocalAI0 = _dummy.localAI[0];
		projectileLocalAI1 = _dummy.localAI[1];
	}
}
