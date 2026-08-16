using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Utilities;

namespace Terraria.GameContent.LeashedEntities;

public abstract class LeashedCritter : LeashedEntity
{
	protected static NPC _dummy = new NPC();

	public int anchorStyle;

	protected int npcType;

	protected int spriteDirection;

	protected Rectangle frame;

	protected double frameCounter;

	protected LCG32Random rand;

	protected short WaitTime;

	protected byte State;

	protected Point16 TargetPosition;

	protected Vector2 netOffset;

	protected float scale = 1f;

	protected int strayingRangeInBlocks;

	protected bool isAquatic;

	protected static readonly float gravity = 0.3f;

	protected static readonly float maxFallSpeed = 10f;

	protected const int RecallDuration = 20;

	public void SetDefaults(int itemType)
	{
		SetDefaults(ContentSamples.ItemsByType[itemType]);
	}

	protected virtual void SetDefaults(Item sample)
	{
		npcType = sample.makeNPC;
		_dummy.SetDefaults(npcType);
		base.Size = _dummy.Size;
	}

	public override void NetSend(BinaryWriter writer, bool full)
	{
		if (full)
		{
			Utils.Write7BitEncodedInt(writer, npcType);
			writer.WriteVector2(base.Size);
		}
		writer.WritePackedVector2(position - base.AnchorPosition.ToWorldCoordinates());
		writer.Write(direction > 0);
		writer.Write(rand.state);
		writer.Write(WaitTime);
		writer.Write(State);
		writer.Write((sbyte)(TargetPosition.X - base.AnchorPosition.X));
		writer.Write((sbyte)(TargetPosition.Y - base.AnchorPosition.Y));
	}

	public override void NetReceive(BinaryReader reader, bool full)
	{
		if (full)
		{
			npcType = Utils.Read7BitEncodedInt(reader);
			base.Size = reader.ReadVector2();
		}
		Vector2 vector = position;
		position = reader.ReadPackedVector2() + base.AnchorPosition.ToWorldCoordinates();
		direction = (reader.ReadBoolean() ? 1 : (-1));
		rand.state = reader.ReadUInt32();
		WaitTime = reader.ReadInt16();
		State = reader.ReadByte();
		TargetPosition = new Point16(base.AnchorPosition.X + reader.ReadSByte(), base.AnchorPosition.Y + reader.ReadSByte());
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

	public override void Spawn(bool newlyAdded)
	{
		base.Center = base.AnchorPosition.ToWorldCoordinates();
		TargetPosition = base.AnchorPosition;
		rand = new LCG32Random((uint)Main.rand.Next());
	}

	public override void Update()
	{
		netOffset = netOffset.MoveTowards(Vector2.Zero, 2f);
	}

	protected void Recall()
	{
		bool flag = Main.netMode != 2;
		if (flag)
		{
			for (int i = 0; i < 10; i++)
			{
				Dust.NewDustDirect(position, width, height, 15, 0f, 0f, 150, default(Color), 1.1f);
			}
		}
		base.Center = base.AnchorPosition.ToWorldCoordinates() - new Vector2(0f, 16f);
		velocity = Vector2.Zero;
		if (flag)
		{
			for (int j = 0; j < 10; j++)
			{
				Dust.NewDustDirect(position, width, height, 15, 0f, 0f, 150, default(Color), 1.1f);
			}
		}
	}

	protected virtual void VisualEffects()
	{
		if (npcType < 0 || !NPCID.Sets.IsGoldCritter[npcType])
		{
			return;
		}
		position += netOffset;
		Color color = Lighting.GetColor((int)base.Center.X / 16, (int)base.Center.Y / 16);
		if (color.R > 20 || color.B > 20 || color.G > 20)
		{
			int num = color.R;
			if (color.G > num)
			{
				num = color.G;
			}
			if (color.B > num)
			{
				num = color.B;
			}
			num /= 30;
			if (Main.rand.Next(300) < num)
			{
				int num2 = Dust.NewDust(position, width, height, 43, 0f, 0f, 254, new Color(255, 255, 0), 0.5f);
				Main.dust[num2].velocity *= 0f;
			}
		}
		position -= netOffset;
	}

	protected virtual void CopyToDummy()
	{
		_dummy.type = npcType;
		_dummy.Size = base.Size;
		_dummy.frame = frame;
		_dummy.frameCounter = frameCounter;
		_dummy.position = base.Center + new Vector2(0f, 8f) - new Vector2(base.Size.X / 2f, base.Size.Y);
		_dummy.velocity = velocity;
		_dummy.direction = direction;
		_dummy.spriteDirection = spriteDirection;
		_dummy.scale = scale;
		_dummy.rotation = 0f;
		_dummy.alpha = 0;
		_dummy.wet = false;
		Array.Clear(_dummy.ai, 0, _dummy.ai.Length);
		Array.Clear(_dummy.localAI, 0, _dummy.localAI.Length);
	}

	protected void CopyFromDummy()
	{
		frame = _dummy.frame;
		frameCounter = _dummy.frameCounter;
		spriteDirection = _dummy.spriteDirection;
	}

	public override void Draw()
	{
		Main.instance.LoadNPC(npcType);
		if (frame.Width == 0 || frame.Height == 0)
		{
			frame = new Rectangle(0, 0, TextureAssets.Npc[npcType].Width(), TextureAssets.Npc[npcType].Height() / Main.npcFrameCount[npcType]);
		}
		CopyToDummy();
		_dummy.position += netOffset + GetDrawOffset();
		Main.instance.DrawNPCDirect(Main.spriteBatch, _dummy, behindTiles: true, Main.screenPosition);
		Point point = _dummy.Center.ToTileCoordinates();
		byte liquid = Framing.GetTileSafely(point.X, point.Y).liquid;
		if ((isAquatic && liquid < byte.MaxValue) || (!isAquatic && liquid > 0))
		{
			DrawBubble();
		}
	}

	public virtual Vector2 GetDrawOffset()
	{
		return Vector2.Zero;
	}

	protected void DrawBubble()
	{
		Main.instance.LoadGore(413);
		Texture2D value = TextureAssets.Gore[413].Value;
		Rectangle rectangle = value.Frame();
		Vector2 origin = rectangle.Size() / 2f;
		Vector2 vector = position;
		vector += netOffset + GetDrawOffset() + _dummy.Size * new Vector2(0.5f, 0.5f);
		Point tileCoords = vector.ToTileCoordinates();
		Main.spriteBatch.Draw(value, vector - Main.screenPosition, rectangle, Lighting.GetColor(tileCoords), 0f, origin, 1f, SpriteEffects.None, 0f);
	}
}
