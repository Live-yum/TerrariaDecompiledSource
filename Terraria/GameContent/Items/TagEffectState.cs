using System;
using System.IO;
using Terraria.ID;
using Terraria.Net;

namespace Terraria.GameContent.Items;

public class TagEffectState
{
	public class NetModule : Terraria.Net.NetModule
	{
		private enum MessageType
		{
			FullState,
			ChangeActiveEffect,
			ApplyTagToNPC,
			EnableProcOnNPC,
			ClearProcOnNPC
		}

		public static void WriteSparseNPCTimeArray(BinaryWriter writer, int[] array)
		{
			for (int i = 0; i < array.Length; i++)
			{
				int num = array[i];
				if (num != 0)
				{
					writer.Write((byte)i);
					writer.Write(num);
				}
			}
			writer.Write((byte)array.Length);
		}

		public static void ReadSparseNPCTimeArray(BinaryReader reader, int[] array)
		{
			Array.Clear(array, 0, array.Length);
			while (true)
			{
				int num = reader.ReadByte();
				if (num < array.Length)
				{
					array[num] = reader.ReadInt32();
					continue;
				}
				break;
			}
		}

		public static NetPacket WriteFullState(TagEffectState state)
		{
			NetPacket result = Terraria.Net.NetModule.CreatePacket<NetModule>();
			result.Writer.Write((byte)state._owner.whoAmI);
			result.Writer.Write((byte)0);
			result.Writer.Write((short)state.Type);
			WriteSparseNPCTimeArray(result.Writer, state.TimeLeftOnNPC);
			if (state._effect.SyncProcs)
			{
				WriteSparseNPCTimeArray(result.Writer, state.ProcTimeLeftOnNPC);
			}
			return result;
		}

		public static NetPacket WriteChangeActiveEffect(TagEffectState state)
		{
			NetPacket result = Terraria.Net.NetModule.CreatePacket<NetModule>();
			result.Writer.Write((byte)state._owner.whoAmI);
			result.Writer.Write((byte)1);
			result.Writer.Write((short)state.Type);
			return result;
		}

		private static NetPacket WriteNPCChange(TagEffectState state, MessageType msgType, int npcIndex)
		{
			NetPacket result = Terraria.Net.NetModule.CreatePacket<NetModule>();
			result.Writer.Write((byte)state._owner.whoAmI);
			result.Writer.Write((byte)msgType);
			result.Writer.Write((byte)npcIndex);
			return result;
		}

		public static NetPacket WriteApplyTagToNPC(TagEffectState state, int npcIndex)
		{
			return WriteNPCChange(state, MessageType.ApplyTagToNPC, npcIndex);
		}

		public static NetPacket WriteEnableProcOnNPC(TagEffectState state, int npcIndex)
		{
			return WriteNPCChange(state, MessageType.EnableProcOnNPC, npcIndex);
		}

		public static NetPacket WriteClearProcOnNPC(TagEffectState state, int npcIndex)
		{
			return WriteNPCChange(state, MessageType.ClearProcOnNPC, npcIndex);
		}

		public override bool Deserialize(BinaryReader reader, int userId)
		{
			int num = reader.ReadByte();
			if (Main.netMode == 2)
			{
				num = userId;
			}
			TagEffectState tagEffectState = Main.player[num].TagEffectState;
			MessageType messageType = (MessageType)reader.ReadByte();
			switch (messageType)
			{
			case MessageType.FullState:
				if (Main.netMode == 2)
				{
					return false;
				}
				tagEffectState.TrySetActiveEffect(reader.ReadInt16());
				ReadSparseNPCTimeArray(reader, tagEffectState.TimeLeftOnNPC);
				if (tagEffectState._effect.SyncProcs)
				{
					ReadSparseNPCTimeArray(reader, tagEffectState.ProcTimeLeftOnNPC);
				}
				break;
			case MessageType.ChangeActiveEffect:
				tagEffectState.TrySetActiveEffect(reader.ReadInt16());
				if (Main.netMode == 2)
				{
					NetManager.Instance.Broadcast(WriteChangeActiveEffect(tagEffectState), num);
				}
				break;
			case MessageType.ApplyTagToNPC:
			case MessageType.EnableProcOnNPC:
			case MessageType.ClearProcOnNPC:
			{
				int num2 = reader.ReadByte();
				switch (messageType)
				{
				case MessageType.ApplyTagToNPC:
					tagEffectState.ApplyTagToNPC(Main.npc[num2]);
					break;
				case MessageType.EnableProcOnNPC:
					tagEffectState.EnableProcOnNPC(Main.npc[num2]);
					break;
				case MessageType.ClearProcOnNPC:
					tagEffectState.ClearProcOnNPC(num2);
					break;
				}
				if (Main.netMode == 2)
				{
					NetManager.Instance.Broadcast(WriteNPCChange(tagEffectState, messageType, num2), num);
				}
				break;
			}
			}
			return true;
		}

		public static void SyncStateIfNecessary(TagEffectState state, int toClient, int ignoreClient)
		{
			if (state._effect != null && state._effect.NetSync)
			{
				NetPacket packet = WriteFullState(state);
				if (toClient >= 0)
				{
					NetManager.Instance.SendToClient(packet, toClient);
				}
				else
				{
					NetManager.Instance.Broadcast(packet, ignoreClient);
				}
			}
		}
	}

	private readonly Player _owner;

	private UniqueTagEffect _effect;

	private readonly int[] TimeLeftOnNPC = new int[Main.maxNPCs];

	private readonly int[] ProcTimeLeftOnNPC = new int[Main.maxNPCs];

	public int Type { get; private set; }

	public TagEffectState(Player owner)
	{
		_owner = owner;
	}

	public bool IsNPCTagged(int npcIndex)
	{
		return TimeLeftOnNPC[npcIndex] > 0;
	}

	public bool CanProcOnNPC(int npcIndex)
	{
		return ProcTimeLeftOnNPC[npcIndex] > 0;
	}

	public void ClearProcOnNPC(int npcIndex)
	{
		ProcTimeLeftOnNPC[npcIndex] = 0;
		if (_effect.NetSync && _owner == Main.LocalPlayer)
		{
			NetManager.Instance.SendToServer(NetModule.WriteClearProcOnNPC(this, npcIndex));
		}
	}

	public void ResetNPCSlotData(int npcIndex)
	{
		TimeLeftOnNPC[npcIndex] = 0;
		ProcTimeLeftOnNPC[npcIndex] = 0;
	}

	private void ApplyTagToNPC(NPC npc)
	{
		if (_effect != null)
		{
			TimeLeftOnNPC[npc.whoAmI] = _effect.TagDuration;
			if (_effect.NetSync && _owner == Main.LocalPlayer)
			{
				NetManager.Instance.SendToServer(NetModule.WriteApplyTagToNPC(this, npc.whoAmI));
			}
			_effect.OnTagAppliedToNPC(_owner, npc);
		}
	}

	private void EnableProcOnNPC(NPC npc)
	{
		if (_effect != null)
		{
			ProcTimeLeftOnNPC[npc.whoAmI] = _effect.TagDuration;
			if (_effect.NetSync && _owner == Main.LocalPlayer)
			{
				NetManager.Instance.SendToServer(NetModule.WriteEnableProcOnNPC(this, npc.whoAmI));
			}
		}
	}

	public void Update()
	{
		if (_effect == null)
		{
			return;
		}
		for (int i = 0; i < TimeLeftOnNPC.Length; i++)
		{
			if (TimeLeftOnNPC[i] > 0)
			{
				TimeLeftOnNPC[i]--;
			}
		}
		for (int j = 0; j < ProcTimeLeftOnNPC.Length; j++)
		{
			if (ProcTimeLeftOnNPC[j] > 0)
			{
				ProcTimeLeftOnNPC[j]--;
			}
		}
	}

	private void Clear()
	{
		Array.Clear(TimeLeftOnNPC, 0, TimeLeftOnNPC.Length);
		Array.Clear(ProcTimeLeftOnNPC, 0, ProcTimeLeftOnNPC.Length);
	}

	public void TryApplyTagToNPC(int itemType, NPC npc)
	{
		if (ItemID.Sets.UniqueTagEffects[itemType].CanApplyTagToNPC(npc.type))
		{
			TrySetActiveEffect(itemType);
			ApplyTagToNPC(npc);
		}
	}

	public void TryEnableProcOnNPC(int expectedActiveEffectType, NPC npc)
	{
		if (Type == expectedActiveEffectType)
		{
			EnableProcOnNPC(npc);
		}
	}

	public void TrySetActiveEffect(int type)
	{
		if (Type != type)
		{
			if (_effect != null)
			{
				_effect.OnRemovedFromPlayer(_owner);
			}
			Clear();
			UniqueTagEffect effect = _effect;
			Type = type;
			_effect = ItemID.Sets.UniqueTagEffects[type];
			if (_owner == Main.LocalPlayer && ((_effect != null && _effect.NetSync) || (effect != null && effect.NetSync)))
			{
				NetManager.Instance.SendToServer(NetModule.WriteChangeActiveEffect(this));
			}
			if (_effect != null)
			{
				_effect.OnSetToPlayer(_owner);
			}
		}
	}

	public void ModifyHit(Projectile optionalProjectile, NPC npcHit, ref int damageDealt, ref bool crit)
	{
		if (_effect != null && IsNPCTagged(npcHit.whoAmI) && _effect.CanRunHitEffects(_owner, optionalProjectile, npcHit))
		{
			_effect.ModifyTaggedHit(_owner, optionalProjectile, npcHit, ref damageDealt, ref crit);
			if (CanProcOnNPC(npcHit.whoAmI))
			{
				_effect.ModifyProcHit(_owner, optionalProjectile, npcHit, ref damageDealt, ref crit);
			}
		}
	}

	public void OnHit(Projectile optionalProjectile, NPC npcHit, int calcDamage)
	{
		if (_effect != null && IsNPCTagged(npcHit.whoAmI) && _effect.CanRunHitEffects(_owner, optionalProjectile, npcHit))
		{
			_effect.OnTaggedHit(_owner, optionalProjectile, npcHit, calcDamage);
			if (CanProcOnNPC(npcHit.whoAmI))
			{
				ClearProcOnNPC(npcHit.whoAmI);
				_effect.OnProcHit(_owner, optionalProjectile, npcHit, calcDamage);
			}
		}
	}
}
