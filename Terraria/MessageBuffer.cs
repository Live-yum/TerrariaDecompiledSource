using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Achievements;
using Terraria.GameContent.Creative;
using Terraria.GameContent.Events;
using Terraria.GameContent.Golf;
using Terraria.GameContent.Tile_Entities;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Map;
using Terraria.Net;
using Terraria.Net.Sockets;
using Terraria.Testing;
using Terraria.UI;

namespace Terraria;

public class MessageBuffer
{
	public const int readBufferMax = 131070;

	public const int writeBufferMax = 131070;

	public bool broadcast;

	public byte[] readBuffer = new byte[131070];

	public byte[] writeBuffer = new byte[131070];

	public bool writeLocked;

	public int messageLength;

	public int totalData;

	public int whoAmI;

	public int spamCount;

	public int maxSpam;

	public bool checkBytes;

	public MemoryStream readerStream;

	public MemoryStream writerStream;

	public BinaryReader reader;

	public BinaryWriter writer;

	public PacketHistory History = new PacketHistory();

	private float[] _temporaryProjectileAI = new float[Projectile.maxAI];

	private float[] _temporaryNPCAI = new float[NPC.maxAI];

	public int RemainingReadBufferLength => readBuffer.Length - totalData;

	public static event TileChangeReceivedEvent OnTileChangeReceived;

	public void Reset()
	{
		Array.Clear(readBuffer, 0, readBuffer.Length);
		Array.Clear(writeBuffer, 0, writeBuffer.Length);
		writeLocked = false;
		messageLength = 0;
		totalData = 0;
		spamCount = 0;
		broadcast = false;
		checkBytes = false;
		ResetReader();
		ResetWriter();
	}

	public void ResetReader()
	{
		if (readerStream != null)
		{
			readerStream.Close();
		}
		readerStream = new MemoryStream(readBuffer);
		reader = new BinaryReader(readerStream);
	}

	public void ResetWriter()
	{
		if (writerStream != null)
		{
			writerStream.Close();
		}
		writerStream = new MemoryStream(writeBuffer);
		writer = new BinaryWriter(writerStream);
	}

	private float[] ReUseTemporaryProjectileAI()
	{
		for (int i = 0; i < _temporaryProjectileAI.Length; i++)
		{
			_temporaryProjectileAI[i] = 0f;
		}
		return _temporaryProjectileAI;
	}

	private float[] ReUseTemporaryNPCAI()
	{
		for (int i = 0; i < _temporaryNPCAI.Length; i++)
		{
			_temporaryNPCAI[i] = 0f;
		}
		return _temporaryNPCAI;
	}

	public void GetData(int start, int length, out int messageType)
	{
		if (whoAmI < 256)
		{
			Netplay.Clients[whoAmI].TimeOutTimer = 0;
		}
		else
		{
			Netplay.Connection.TimeOutTimer = 0;
		}
		byte b = 0;
		int num = 0;
		num = start + 1;
		b = (byte)(messageType = readBuffer[start]);
		if (b >= MessageID.Count)
		{
			return;
		}
		Main.ActiveNetDiagnosticsUI.CountReadMessage(b, length);
		if (Main.netMode == 1 && Netplay.Connection.StatusMax > 0)
		{
			Netplay.Connection.StatusCount++;
		}
		if (Main.verboseNetplay)
		{
			for (int i = start; i < start + length; i++)
			{
			}
			for (int j = start; j < start + length; j++)
			{
				_ = readBuffer[j];
			}
		}
		if (Main.netMode == 2 && b != 38 && Netplay.Clients[whoAmI].State == -1)
		{
			NetMessage.TrySendData(2, whoAmI, -1, Lang.mp[1].ToNetworkText());
			return;
		}
		if (Main.netMode == 2)
		{
			if (Netplay.Clients[whoAmI].State < 10 && b > 12 && b != 93 && b != 16 && b != 42 && b != 50 && b != 38 && b != 68 && b != 147 && b != 161)
			{
				NetMessage.BootPlayer(whoAmI, Lang.mp[2].ToNetworkText());
			}
			if (Netplay.Clients[whoAmI].State == 0 && b != 1)
			{
				NetMessage.BootPlayer(whoAmI, Lang.mp[2].ToNetworkText());
			}
		}
		if (reader == null)
		{
			ResetReader();
		}
		reader.BaseStream.Position = num;
		switch (b)
		{
		case 1:
			if (Main.netMode != 2)
			{
				break;
			}
			if (Main.dedServ && Netplay.IsBanned(Netplay.Clients[whoAmI].Socket.GetRemoteAddress()))
			{
				NetMessage.TrySendData(2, whoAmI, -1, Lang.mp[3].ToNetworkText());
			}
			else
			{
				if (Netplay.Clients[whoAmI].State != 0)
				{
					break;
				}
				if (reader.ReadString() == "Terraria" + 318)
				{
					if (string.IsNullOrEmpty(Netplay.ServerPassword))
					{
						Netplay.Clients[whoAmI].State = 1;
						NetMessage.TrySendData(3, whoAmI);
					}
					else
					{
						Netplay.Clients[whoAmI].State = -1;
						NetMessage.TrySendData(37, whoAmI);
					}
				}
				else
				{
					NetMessage.TrySendData(2, whoAmI, -1, Lang.mp[4].ToNetworkText());
				}
			}
			break;
		case 2:
			if (Main.netMode == 1)
			{
				Netplay.Disconnect = true;
				Main.statusText = NetworkText.Deserialize(reader).ToString();
			}
			break;
		case 3:
			if (Main.netMode == 1)
			{
				if (Netplay.Connection.State == 1)
				{
					Netplay.Connection.State = 2;
				}
				int num91 = reader.ReadByte();
				bool value2 = reader.ReadBoolean();
				Netplay.Connection.ServerSpecialFlags[2] = value2;
				if (num91 != Main.myPlayer)
				{
					Main.player[num91] = Main.ActivePlayerFileData.Player;
					Main.player[Main.myPlayer] = new Player();
				}
				Main.player[num91].whoAmI = num91;
				Main.myPlayer = num91;
				Player player9 = Main.player[num91];
				NetMessage.TrySendData(4, -1, -1, null, num91);
				NetMessage.TrySendData(68, -1, -1, null, num91);
				NetMessage.TrySendData(16, -1, -1, null, num91);
				NetMessage.TrySendData(42, -1, -1, null, num91);
				NetMessage.TrySendData(50, -1, -1, null, num91);
				NetMessage.TrySendData(147, -1, -1, null, num91, player9.CurrentLoadoutIndex);
				for (int num92 = 0; num92 < 59; num92++)
				{
					NetMessage.TrySendData(5, -1, -1, null, num91, PlayerItemSlotID.Inventory0 + num92);
				}
				TrySendingItemArray(num91, player9.armor, PlayerItemSlotID.Armor0);
				TrySendingItemArray(num91, player9.dye, PlayerItemSlotID.Dye0);
				TrySendingItemArray(num91, player9.miscEquips, PlayerItemSlotID.Misc0);
				TrySendingItemArray(num91, player9.miscDyes, PlayerItemSlotID.MiscDye0);
				TrySendingItemArray(num91, player9.bank.item, PlayerItemSlotID.Bank1_0);
				TrySendingItemArray(num91, player9.bank2.item, PlayerItemSlotID.Bank2_0);
				NetMessage.TrySendData(5, -1, -1, null, num91, PlayerItemSlotID.TrashItem);
				TrySendingItemArray(num91, player9.bank3.item, PlayerItemSlotID.Bank3_0);
				TrySendingItemArray(num91, player9.bank4.item, PlayerItemSlotID.Bank4_0);
				TrySendingItemArray(num91, player9.Loadouts[0].Armor, PlayerItemSlotID.Loadout1_Armor_0);
				TrySendingItemArray(num91, player9.Loadouts[0].Dye, PlayerItemSlotID.Loadout1_Dye_0);
				TrySendingItemArray(num91, player9.Loadouts[1].Armor, PlayerItemSlotID.Loadout2_Armor_0);
				TrySendingItemArray(num91, player9.Loadouts[1].Dye, PlayerItemSlotID.Loadout2_Dye_0);
				TrySendingItemArray(num91, player9.Loadouts[2].Armor, PlayerItemSlotID.Loadout3_Armor_0);
				TrySendingItemArray(num91, player9.Loadouts[2].Dye, PlayerItemSlotID.Loadout3_Dye_0);
				if (!string.IsNullOrWhiteSpace(Netplay.HostToken))
				{
					NetMessage.TrySendData(161, -1, -1, NetworkText.FromLiteral(Netplay.HostToken));
				}
				NetMessage.TrySendData(6);
				if (Netplay.Connection.State == 2)
				{
					Netplay.Connection.State = 3;
				}
			}
			break;
		case 4:
		{
			int num199 = reader.ReadByte();
			if (Main.netMode == 2)
			{
				num199 = whoAmI;
			}
			if (num199 == Main.myPlayer && !Main.ServerSideCharacter)
			{
				break;
			}
			Player player18 = Main.player[num199];
			player18.whoAmI = num199;
			player18.skinVariant = reader.ReadByte();
			player18.skinVariant = (int)MathHelper.Clamp(player18.skinVariant, 0f, PlayerVariantID.Count - 1);
			player18.voiceVariant = reader.ReadByte();
			player18.voiceVariant = Utils.Clamp(player18.voiceVariant, 1, 4);
			player18.voicePitchOffset = reader.ReadSingle();
			if (float.IsNaN(player18.voicePitchOffset))
			{
				player18.voicePitchOffset = 0f;
			}
			player18.voicePitchOffset = Utils.Clamp(player18.voicePitchOffset, -1f, 1f);
			player18.hair = reader.ReadByte();
			if (player18.hair >= 228)
			{
				player18.hair = 0;
			}
			player18.name = reader.ReadString().Trim().Trim();
			player18.hairDye = reader.ReadByte();
			ReadAccessoryVisibility(reader, player18.hideVisibleAccessory);
			player18.hideMisc = reader.ReadByte();
			player18.hairColor = reader.ReadRGB();
			player18.skinColor = reader.ReadRGB();
			player18.eyeColor = reader.ReadRGB();
			player18.shirtColor = reader.ReadRGB();
			player18.underShirtColor = reader.ReadRGB();
			player18.pantsColor = reader.ReadRGB();
			player18.shoeColor = reader.ReadRGB();
			BitsByte bitsByte12 = reader.ReadByte();
			player18.difficulty = 0;
			if (bitsByte12[0])
			{
				player18.difficulty = 1;
			}
			if (bitsByte12[1])
			{
				player18.difficulty = 2;
			}
			if (bitsByte12[3])
			{
				player18.difficulty = 3;
			}
			if (player18.difficulty > 3)
			{
				player18.difficulty = 3;
			}
			player18.extraAccessory = bitsByte12[2];
			BitsByte bitsByte13 = reader.ReadByte();
			player18.UsingBiomeTorches = bitsByte13[0];
			player18.happyFunTorchTime = bitsByte13[1];
			player18.unlockedBiomeTorches = bitsByte13[2];
			player18.unlockedSuperCart = bitsByte13[3];
			player18.enabledSuperCart = bitsByte13[4];
			BitsByte bitsByte14 = reader.ReadByte();
			player18.usedAegisCrystal = bitsByte14[0];
			player18.usedAegisFruit = bitsByte14[1];
			player18.usedArcaneCrystal = bitsByte14[2];
			player18.usedGalaxyPearl = bitsByte14[3];
			player18.usedGummyWorm = bitsByte14[4];
			player18.usedAmbrosia = bitsByte14[5];
			player18.ateArtisanBread = bitsByte14[6];
			if (Main.netMode != 2)
			{
				break;
			}
			bool flag16 = false;
			if (Netplay.Clients[whoAmI].State < 10)
			{
				for (int num200 = 0; num200 < 255; num200++)
				{
					if (num200 != num199 && player18.name == Main.player[num200].name && Netplay.Clients[num200].IsActive)
					{
						flag16 = true;
					}
				}
			}
			if (flag16)
			{
				NetMessage.TrySendData(2, whoAmI, -1, NetworkText.FromKey(Lang.mp[5].Key, player18.name));
			}
			else if (player18.name.Length > Player.nameLen)
			{
				NetMessage.TrySendData(2, whoAmI, -1, NetworkText.FromKey("Net.NameTooLong"));
			}
			else if (player18.name == "")
			{
				NetMessage.TrySendData(2, whoAmI, -1, NetworkText.FromKey("Net.EmptyName"));
			}
			else if (player18.difficulty == 3 && !Main.IsJourneyMode)
			{
				NetMessage.TrySendData(2, whoAmI, -1, NetworkText.FromKey("Net.PlayerIsCreativeAndWorldIsNotCreative"));
			}
			else if (player18.difficulty != 3 && Main.IsJourneyMode)
			{
				NetMessage.TrySendData(2, whoAmI, -1, NetworkText.FromKey("Net.PlayerIsNotCreativeAndWorldIsCreative"));
			}
			else
			{
				Netplay.Clients[whoAmI].Name = player18.name;
				Netplay.Clients[whoAmI].Name = player18.name;
				NetMessage.TrySendData(4, -1, whoAmI, null, num199);
			}
			break;
		}
		case 5:
		{
			int num35 = reader.ReadByte();
			int num36 = reader.ReadInt16();
			int stack3 = reader.ReadInt16();
			int prefixWeWant2 = reader.ReadByte();
			int type4 = reader.ReadInt16();
			BitsByte bitsByte3 = reader.ReadByte();
			bool favorited = bitsByte3[0];
			bool flag2 = bitsByte3[1];
			if (Main.netMode == 2)
			{
				num35 = whoAmI;
			}
			if (num35 == Main.myPlayer && !Main.ServerSideCharacter && !Main.player[num35].HasLockedInventory())
			{
				break;
			}
			Player player2 = Main.player[num35];
			lock (player2)
			{
				PlayerItemSlotID.SlotReference slot = new PlayerItemSlotID.SlotReference(player2, num36);
				PlayerItemSlotID.SlotReference slotReference = new PlayerItemSlotID.SlotReference(Main.clientPlayer, num36);
				Item item = new Item();
				item.SetDefaults(type4);
				item.stack = stack3;
				item.Prefix(prefixWeWant2);
				item.favorited = favorited;
				slot.Item = item;
				if (num35 == Main.myPlayer && !Main.ServerSideCharacter)
				{
					slotReference.Item = item.Clone();
				}
				if (num36 >= PlayerItemSlotID.Bank4_0 && num36 < PlayerItemSlotID.Loadout1_Armor_0)
				{
					if (Main.netMode == 1 && player2.disableVoidBag == num36 - PlayerItemSlotID.Bank4_0)
					{
						player2.disableVoidBag = -1;
					}
				}
				else if (num36 <= 58)
				{
					if (num35 == Main.myPlayer && num36 == 58)
					{
						Main.mouseItem = item.Clone();
					}
					if (num35 == Main.myPlayer && Main.netMode == 1)
					{
						Main.player[num35].inventoryChestStack[num36] = false;
					}
				}
				if (Main.netMode == 1 && num35 == Main.myPlayer && flag2)
				{
					ItemSlot.IndicateBlockedSlot(slot);
				}
				bool[] canRelay = PlayerItemSlotID.CanRelay;
				if (Main.netMode == 2 && num35 == whoAmI && canRelay.IndexInRange(num36) && canRelay[num36])
				{
					NetMessage.TrySendData(5, -1, whoAmI, null, num35, num36);
				}
				break;
			}
		}
		case 6:
			if (Main.netMode == 2)
			{
				if (Netplay.Clients[whoAmI].State == 1)
				{
					Netplay.Clients[whoAmI].State = 2;
				}
				NetMessage.TrySendData(7, whoAmI);
				Main.SyncAnInvasion(whoAmI);
			}
			break;
		case 7:
			if (Main.netMode == 1)
			{
				Main.time = reader.ReadInt32();
				BitsByte bitsByte24 = reader.ReadByte();
				Main.dayTime = bitsByte24[0];
				Main.bloodMoon = bitsByte24[1];
				Main.eclipse = bitsByte24[2];
				Main.moonPhase = reader.ReadByte();
				Main.maxTilesX = reader.ReadInt16();
				Main.maxTilesY = reader.ReadInt16();
				Main.spawnTileX = reader.ReadInt16();
				Main.spawnTileY = reader.ReadInt16();
				Main.worldSurface = reader.ReadInt16();
				Main.rockLayer = reader.ReadInt16();
				Main.ActiveWorldFileData.WorldId = reader.ReadInt32();
				Main.worldName = reader.ReadString();
				Main.GameMode = reader.ReadByte();
				Main.ActiveWorldFileData.UniqueId = new Guid(reader.ReadBytes(16));
				Main.ActiveWorldFileData.WorldGeneratorVersion = reader.ReadUInt64();
				Main.moonType = reader.ReadByte();
				WorldGen.setBG(0, reader.ReadByte());
				WorldGen.setBG(10, reader.ReadByte());
				WorldGen.setBG(11, reader.ReadByte());
				WorldGen.setBG(12, reader.ReadByte());
				WorldGen.setBG(1, reader.ReadByte());
				WorldGen.setBG(2, reader.ReadByte());
				WorldGen.setBG(3, reader.ReadByte());
				WorldGen.setBG(4, reader.ReadByte());
				WorldGen.setBG(5, reader.ReadByte());
				WorldGen.setBG(6, reader.ReadByte());
				WorldGen.setBG(7, reader.ReadByte());
				WorldGen.setBG(8, reader.ReadByte());
				WorldGen.setBG(9, reader.ReadByte());
				Main.iceBackStyle = reader.ReadByte();
				Main.jungleBackStyle = reader.ReadByte();
				Main.hellBackStyle = reader.ReadByte();
				Main.windSpeedTarget = reader.ReadSingle();
				Main.numClouds = reader.ReadByte();
				for (int num245 = 0; num245 < 3; num245++)
				{
					Main.treeX[num245] = reader.ReadInt32();
				}
				for (int num246 = 0; num246 < 4; num246++)
				{
					Main.treeStyle[num246] = reader.ReadByte();
				}
				for (int num247 = 0; num247 < 3; num247++)
				{
					Main.caveBackX[num247] = reader.ReadInt32();
				}
				for (int num248 = 0; num248 < 4; num248++)
				{
					Main.caveBackStyle[num248] = reader.ReadByte();
				}
				WorldGen.TreeTops.SyncReceive(reader);
				WorldGen.BackgroundsCache.UpdateCache();
				Main.maxRaining = reader.ReadSingle();
				Main.raining = Main.maxRaining > 0f;
				BitsByte bitsByte25 = reader.ReadByte();
				WorldGen.shadowOrbSmashed = bitsByte25[0];
				NPC.downedBoss1 = bitsByte25[1];
				NPC.downedBoss2 = bitsByte25[2];
				NPC.downedBoss3 = bitsByte25[3];
				Main.hardMode = bitsByte25[4];
				NPC.downedClown = bitsByte25[5];
				Main.ServerSideCharacter = bitsByte25[6];
				NPC.downedPlantBoss = bitsByte25[7];
				if (Main.ServerSideCharacter)
				{
					Main.ActivePlayerFileData.MarkAsServerSide();
				}
				BitsByte bitsByte26 = reader.ReadByte();
				NPC.downedMechBoss1 = bitsByte26[0];
				NPC.downedMechBoss2 = bitsByte26[1];
				NPC.downedMechBoss3 = bitsByte26[2];
				NPC.downedMechBossAny = bitsByte26[3];
				Main.cloudBGActive = (bitsByte26[4] ? 1 : 0);
				WorldGen.crimson = bitsByte26[5];
				Main.pumpkinMoon = bitsByte26[6];
				Main.snowMoon = bitsByte26[7];
				BitsByte bitsByte27 = reader.ReadByte();
				Main.fastForwardTimeToDawn = bitsByte27[1];
				Main.UpdateTimeRate();
				bool num249 = bitsByte27[2];
				NPC.downedSlimeKing = bitsByte27[3];
				NPC.downedQueenBee = bitsByte27[4];
				NPC.downedFishron = bitsByte27[5];
				NPC.downedMartians = bitsByte27[6];
				NPC.downedAncientCultist = bitsByte27[7];
				BitsByte bitsByte28 = reader.ReadByte();
				NPC.downedMoonlord = bitsByte28[0];
				NPC.downedHalloweenKing = bitsByte28[1];
				NPC.downedHalloweenTree = bitsByte28[2];
				NPC.downedChristmasIceQueen = bitsByte28[3];
				NPC.downedChristmasSantank = bitsByte28[4];
				NPC.downedChristmasTree = bitsByte28[5];
				NPC.downedGolemBoss = bitsByte28[6];
				BirthdayParty.ManualParty = bitsByte28[7];
				BitsByte bitsByte29 = reader.ReadByte();
				NPC.downedPirates = bitsByte29[0];
				NPC.downedFrost = bitsByte29[1];
				NPC.downedGoblins = bitsByte29[2];
				Sandstorm.Happening = bitsByte29[3];
				DD2Event.Ongoing = bitsByte29[4];
				DD2Event.DownedInvasionT1 = bitsByte29[5];
				DD2Event.DownedInvasionT2 = bitsByte29[6];
				DD2Event.DownedInvasionT3 = bitsByte29[7];
				BitsByte bitsByte30 = reader.ReadByte();
				NPC.combatBookWasUsed = bitsByte30[0];
				LanternNight.ManualLanterns = bitsByte30[1];
				NPC.downedTowerSolar = bitsByte30[2];
				NPC.downedTowerVortex = bitsByte30[3];
				NPC.downedTowerNebula = bitsByte30[4];
				NPC.downedTowerStardust = bitsByte30[5];
				Main.forceHalloweenForToday = bitsByte30[6];
				Main.forceXMasForToday = bitsByte30[7];
				BitsByte bitsByte31 = reader.ReadByte();
				NPC.boughtCat = bitsByte31[0];
				NPC.boughtDog = bitsByte31[1];
				NPC.boughtBunny = bitsByte31[2];
				NPC.freeCake = bitsByte31[3];
				Main.drunkWorld = bitsByte31[4];
				NPC.downedEmpressOfLight = bitsByte31[5];
				NPC.downedQueenSlime = bitsByte31[6];
				Main.getGoodWorld = bitsByte31[7];
				BitsByte bitsByte32 = reader.ReadByte();
				Main.tenthAnniversaryWorld = bitsByte32[0];
				Main.dontStarveWorld = bitsByte32[1];
				NPC.downedDeerclops = bitsByte32[2];
				Main.notTheBeesWorld = bitsByte32[3];
				Main.remixWorld = bitsByte32[4];
				NPC.unlockedSlimeBlueSpawn = bitsByte32[5];
				NPC.combatBookVolumeTwoWasUsed = bitsByte32[6];
				NPC.peddlersSatchelWasUsed = bitsByte32[7];
				BitsByte bitsByte33 = reader.ReadByte();
				NPC.unlockedSlimeGreenSpawn = bitsByte33[0];
				NPC.unlockedSlimeOldSpawn = bitsByte33[1];
				NPC.unlockedSlimePurpleSpawn = bitsByte33[2];
				NPC.unlockedSlimeRainbowSpawn = bitsByte33[3];
				NPC.unlockedSlimeRedSpawn = bitsByte33[4];
				NPC.unlockedSlimeYellowSpawn = bitsByte33[5];
				NPC.unlockedSlimeCopperSpawn = bitsByte33[6];
				Main.fastForwardTimeToDusk = bitsByte33[7];
				BitsByte bitsByte34 = reader.ReadByte();
				Main.noTrapsWorld = bitsByte34[0];
				Main.zenithWorld = bitsByte34[1];
				NPC.unlockedTruffleSpawn = bitsByte34[2];
				Main.vampireSeed = bitsByte34[3];
				Main.infectedSeed = bitsByte34[4];
				Main.teamBasedSpawnsSeed = bitsByte34[5];
				Main.skyblockWorld = bitsByte34[6];
				Main.dualDungeonsSeed = bitsByte34[7];
				WorldGen.Skyblock.lowTiles = ((BitsByte)reader.ReadByte())[0];
				Main.sundialCooldown = reader.ReadByte();
				Main.moondialCooldown = reader.ReadByte();
				WorldGen.SavedOreTiers.Copper = reader.ReadInt16();
				WorldGen.SavedOreTiers.Iron = reader.ReadInt16();
				WorldGen.SavedOreTiers.Silver = reader.ReadInt16();
				WorldGen.SavedOreTiers.Gold = reader.ReadInt16();
				WorldGen.SavedOreTiers.Cobalt = reader.ReadInt16();
				WorldGen.SavedOreTiers.Mythril = reader.ReadInt16();
				WorldGen.SavedOreTiers.Adamantite = reader.ReadInt16();
				if (num249)
				{
					Main.StartSlimeRain(announce: false);
				}
				else
				{
					Main.StopSlimeRain();
				}
				Main.invasionType = reader.ReadSByte();
				Main.LobbyId = reader.ReadUInt64();
				Sandstorm.IntendedSeverity = reader.ReadSingle();
				ExtraSpawnPointManager.Read(reader, networking: true);
				if (Netplay.Connection.State == 3)
				{
					Main.windSpeedCurrent = Main.windSpeedTarget;
					Netplay.Connection.State = 4;
				}
				Main.checkHalloween();
				Main.checkXMas();
			}
			break;
		case 8:
		{
			if (Main.netMode != 2)
			{
				break;
			}
			NetMessage.TrySendData(7, whoAmI);
			int num95 = reader.ReadInt32();
			int num96 = reader.ReadInt32();
			int num97 = reader.ReadByte();
			bool flag9 = true;
			if (num95 == -1 || num96 == -1)
			{
				flag9 = false;
			}
			else if (num95 < 10 || num95 > Main.maxTilesX - 10)
			{
				flag9 = false;
			}
			else if (num96 < 10 || num96 > Main.maxTilesY - 10)
			{
				flag9 = false;
			}
			bool flag10 = false;
			if (Main.teamBasedSpawnsSeed && num97 != 0)
			{
				flag10 = true;
			}
			int num98 = Netplay.GetSectionX(Main.spawnTileX) - 2;
			int num99 = Netplay.GetSectionY(Main.spawnTileY) - 1;
			int num100 = num98 + 5;
			int num101 = num99 + 3;
			if (num98 < 0)
			{
				num98 = 0;
			}
			if (num100 >= Main.maxSectionsX)
			{
				num100 = Main.maxSectionsX;
			}
			if (num99 < 0)
			{
				num99 = 0;
			}
			if (num101 >= Main.maxSectionsY)
			{
				num101 = Main.maxSectionsY;
			}
			int num102 = (num100 - num98) * (num101 - num99);
			List<Point> list = new List<Point>();
			for (int num103 = num98; num103 < num100; num103++)
			{
				for (int num104 = num99; num104 < num101; num104++)
				{
					list.Add(new Point(num103, num104));
				}
			}
			int num105 = -1;
			int num106 = -1;
			if (flag9)
			{
				num95 = Netplay.GetSectionX(num95) - 2;
				num96 = Netplay.GetSectionY(num96) - 1;
				num105 = num95 + 5;
				num106 = num96 + 3;
				if (num95 < 0)
				{
					num95 = 0;
				}
				if (num105 >= Main.maxSectionsX)
				{
					num105 = Main.maxSectionsX - 1;
				}
				if (num96 < 0)
				{
					num96 = 0;
				}
				if (num106 >= Main.maxSectionsY)
				{
					num106 = Main.maxSectionsY - 1;
				}
				for (int num107 = num95; num107 <= num105; num107++)
				{
					for (int num108 = num96; num108 <= num106; num108++)
					{
						if (num107 < num98 || num107 >= num100 || num108 < num99 || num108 >= num101)
						{
							list.Add(new Point(num107, num108));
							num102++;
						}
					}
				}
			}
			int num109 = -1;
			int num110 = -1;
			int num111 = -1;
			int num112 = -1;
			if (flag10)
			{
				Point spawnPoint = Point.Zero;
				if (ExtraSpawnPointManager.TryGetExtraSpawnPointForTeam(num97, out spawnPoint))
				{
					num109 = spawnPoint.X;
					num110 = spawnPoint.Y;
					num109 = Netplay.GetSectionX(num109) - 2;
					num110 = Netplay.GetSectionY(num110) - 1;
					num111 = num109 + 5;
					num112 = num110 + 3;
					if (num109 < 0)
					{
						num109 = 0;
					}
					if (num111 >= Main.maxSectionsX)
					{
						num111 = Main.maxSectionsX - 1;
					}
					if (num110 < 0)
					{
						num110 = 0;
					}
					if (num112 >= Main.maxSectionsY)
					{
						num112 = Main.maxSectionsY - 1;
					}
					for (int num113 = num109; num113 <= num111; num113++)
					{
						for (int num114 = num110; num114 <= num112; num114++)
						{
							if ((num113 < num98 || num113 >= num100 || num114 < num99 || num114 >= num101) && (num113 < num95 || num113 >= num105 || num114 < num96 || num114 >= num106))
							{
								list.Add(new Point(num113, num114));
								num102++;
							}
						}
					}
				}
				else
				{
					flag10 = false;
				}
			}
			PortalHelper.SyncPortalsOnPlayerJoin(whoAmI, 1, list, out var portalSections);
			num102 += portalSections.Count;
			if (Netplay.Clients[whoAmI].State == 2)
			{
				Netplay.Clients[whoAmI].State = 3;
			}
			NetMessage.TrySendData(9, whoAmI, -1, Lang.inter[44].ToNetworkText(), num102);
			Netplay.Clients[whoAmI].StatusText2 = Language.GetTextValue("Net.IsReceivingTileData");
			Netplay.Clients[whoAmI].StatusMax += num102;
			for (int num115 = num98; num115 < num100; num115++)
			{
				for (int num116 = num99; num116 < num101; num116++)
				{
					NetMessage.SendSection(whoAmI, num115, num116);
				}
			}
			if (flag9)
			{
				for (int num117 = num95; num117 <= num105; num117++)
				{
					for (int num118 = num96; num118 <= num106; num118++)
					{
						NetMessage.SendSection(whoAmI, num117, num118);
					}
				}
			}
			if (flag10)
			{
				for (int num119 = num109; num119 <= num111; num119++)
				{
					for (int num120 = num110; num120 <= num112; num120++)
					{
						NetMessage.SendSection(whoAmI, num119, num120);
					}
				}
			}
			for (int num121 = 0; num121 < portalSections.Count; num121++)
			{
				NetMessage.SendSection(whoAmI, portalSections[num121].X, portalSections[num121].Y);
			}
			for (int num122 = 0; num122 < 400; num122++)
			{
				if (Main.item[num122].active)
				{
					NetMessage.TrySendData(21, whoAmI, -1, null, num122);
					NetMessage.TrySendData(22, whoAmI, -1, null, num122);
				}
			}
			for (int num123 = 0; num123 < Main.maxNPCs; num123++)
			{
				if (Main.npc[num123].active)
				{
					NetMessage.TrySendData(23, whoAmI, -1, null, num123);
					NetMessage.TrySendData(54, whoAmI, -1, null, num123);
				}
			}
			for (int num124 = 0; num124 < 1000; num124++)
			{
				if (Main.projectile[num124].active && (Main.projPet[Main.projectile[num124].type] || Main.projectile[num124].netImportant))
				{
					NetMessage.TrySendData(27, whoAmI, -1, null, num124);
				}
			}
			NetManager.Instance.SendToClient(BannerSystem.NetBannersModule.WriteFullState(), whoAmI);
			NetMessage.TrySendData(57, whoAmI);
			NetMessage.TrySendData(103);
			NetMessage.TrySendData(101, whoAmI);
			NetMessage.TrySendData(136, whoAmI);
			Main.BestiaryTracker.OnPlayerJoining(whoAmI);
			CreativePowerManager.Instance.SyncThingsToJoiningPlayer(whoAmI);
			Main.PylonSystem.OnPlayerJoining(whoAmI);
			NetMessage.TrySendData(49, whoAmI);
			break;
		}
		case 9:
			if (Main.netMode == 1)
			{
				Netplay.Connection.StatusMax += reader.ReadInt32();
				Netplay.Connection.StatusText = NetworkText.Deserialize(reader).ToString();
				BitsByte bitsByte4 = reader.ReadByte();
				BitsByte serverSpecialFlags = Netplay.Connection.ServerSpecialFlags;
				serverSpecialFlags[0] = bitsByte4[0];
				serverSpecialFlags[1] = bitsByte4[1];
				Netplay.Connection.ServerSpecialFlags = serverSpecialFlags;
			}
			break;
		case 10:
			if (Main.netMode == 1)
			{
				NetMessage.DecompressTileBlock(reader.BaseStream);
			}
			break;
		case 11:
			if (Main.netMode == 1)
			{
				WorldGen.SectionTileFrame(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
			}
			break;
		case 12:
		{
			int num144 = reader.ReadByte();
			if (Main.netMode == 2)
			{
				num144 = whoAmI;
			}
			Player player12 = Main.player[num144];
			player12.SpawnX = reader.ReadInt16();
			player12.SpawnY = reader.ReadInt16();
			player12.respawnTimer = reader.ReadInt32();
			player12.numberOfDeathsPVE = reader.ReadInt16();
			player12.numberOfDeathsPVP = reader.ReadInt16();
			player12.team = reader.ReadByte();
			if (player12.respawnTimer > 0)
			{
				player12.dead = true;
			}
			PlayerSpawnContext playerSpawnContext = (PlayerSpawnContext)reader.ReadByte();
			player12.Spawn(playerSpawnContext);
			if (Main.netMode != 2 || Netplay.Clients[whoAmI].State < 3)
			{
				break;
			}
			if (Netplay.Clients[whoAmI].State == 3)
			{
				Netplay.Clients[whoAmI].State = 10;
				NetMessage.buffer[whoAmI].broadcast = true;
				NetMessage.SyncConnectedPlayer(whoAmI);
				bool flag12 = NetMessage.DoesPlayerSlotCountAsAHost(whoAmI);
				Main.countsAsHostForGameplay[whoAmI] = flag12;
				if (NetMessage.DoesPlayerSlotCountAsAHost(whoAmI))
				{
					NetMessage.TrySendData(139, whoAmI, -1, null, whoAmI, flag12.ToInt());
				}
				NetMessage.TrySendData(12, -1, whoAmI, null, whoAmI, (int)(byte)playerSpawnContext);
				NetMessage.TrySendData(129, whoAmI);
				NetMessage.greetPlayer(whoAmI);
				if (Main.player[num144].unlockedBiomeTorches)
				{
					NPC nPC = new NPC();
					nPC.SetDefaults(664);
					Main.BestiaryTracker.Kills.RegisterKill(nPC);
				}
			}
			else
			{
				NetMessage.TrySendData(12, -1, whoAmI, null, whoAmI, (int)(byte)playerSpawnContext);
			}
			break;
		}
		case 13:
		{
			int num210 = reader.ReadByte();
			if (num210 == Main.myPlayer && !Main.ServerSideCharacter)
			{
				break;
			}
			if (Main.netMode == 2)
			{
				num210 = whoAmI;
			}
			Player player19 = Main.player[num210];
			BitsByte bitsByte16 = reader.ReadByte();
			BitsByte bitsByte17 = reader.ReadByte();
			BitsByte bitsByte18 = reader.ReadByte();
			BitsByte bitsByte19 = reader.ReadByte();
			player19.controlUp = bitsByte16[0];
			player19.controlDown = bitsByte16[1];
			player19.controlLeft = bitsByte16[2];
			player19.controlRight = bitsByte16[3];
			player19.controlJump = bitsByte16[4];
			player19.controlUseItem = bitsByte16[5];
			player19.direction = (bitsByte16[6] ? 1 : (-1));
			if (bitsByte17[0])
			{
				player19.pulley = true;
				player19.pulleyDir = (byte)((!bitsByte17[1]) ? 1u : 2u);
			}
			else
			{
				player19.pulley = false;
			}
			player19.vortexStealthActive = bitsByte17[3];
			player19.gravDir = (bitsByte17[4] ? 1 : (-1));
			player19.TryTogglingShield(bitsByte17[5]);
			player19.ghost = bitsByte17[6];
			player19.selectedItemState.Select(reader.ReadByte());
			Vector2 vector5 = reader.ReadVector2();
			Vector2 velocity5 = Vector2.Zero;
			if (bitsByte17[2])
			{
				velocity5 = reader.ReadVector2();
			}
			if (player19.unacknowledgedTeleports > 0)
			{
				vector5 = player19.position;
				velocity5 = player19.velocity;
			}
			if (Main.netMode == 1 && player19.position != Vector2.Zero)
			{
				player19.netOffset += player19.position - vector5;
				if (player19.netOffset.Length() > (float)Main.multiplayerNPCSmoothingRange)
				{
					player19.netOffset = Vector2.Zero;
				}
				if (player19.netOffset != Vector2.Zero && DebugOptions.ShowNetOffsetDust && Vector2.Distance(vector5, player19.position) > 4f)
				{
					Dust.QuickDustLine(vector5, player19.position, 20f, Color.Red);
				}
			}
			player19.position = vector5;
			player19.velocity = velocity5;
			Vector2 t = player19.position;
			if (bitsByte17[7])
			{
				player19.mount.SetMount(reader.ReadUInt16(), player19);
			}
			else
			{
				player19.mount.Dismount(player19);
			}
			if (bitsByte18[6])
			{
				player19.PotionOfReturnOriginalUsePosition = reader.ReadVector2();
				player19.PotionOfReturnHomePosition = reader.ReadVector2();
			}
			else
			{
				player19.PotionOfReturnOriginalUsePosition = null;
				player19.PotionOfReturnHomePosition = null;
			}
			player19.tryKeepingHoveringUp = bitsByte18[0];
			player19.IsVoidVaultEnabled = bitsByte18[1];
			player19.sitting.isSitting = bitsByte18[2];
			player19.downedDD2EventAnyDifficulty = bitsByte18[3];
			player19.petting.isPetting = bitsByte18[4];
			player19.petting.isPetSmall = bitsByte18[5];
			player19.tryKeepingHoveringDown = bitsByte18[7];
			player19.sleeping.SetIsSleepingAndAdjustPlayerRotation(player19, bitsByte19[0]);
			player19.autoReuseAllWeapons = bitsByte19[1];
			player19.controlDownHold = bitsByte19[2];
			player19.isOperatingAnotherEntity = bitsByte19[3];
			player19.controlUseTile = bitsByte19[4];
			player19.netCameraTarget = (bitsByte19[5] ? new Vector2?(reader.ReadVector2()) : ((Vector2?)null));
			player19.lastItemUseAttemptSuccess = bitsByte19[6];
			Utils.Swap(ref t, ref player19.position);
			if (Main.netMode == 2 && Netplay.Clients[whoAmI].State == 10)
			{
				NetMessage.TrySendData(13, -1, whoAmI, null, num210);
			}
			Utils.Swap(ref t, ref player19.position);
			break;
		}
		case 14:
		{
			int num48 = reader.ReadByte();
			int num49 = reader.ReadByte();
			if (Main.netMode != 1)
			{
				break;
			}
			bool active = Main.player[num48].active;
			if (num49 == 1)
			{
				if (!Main.player[num48].active)
				{
					Main.player[num48] = new Player();
				}
				Main.player[num48].active = true;
			}
			else
			{
				Main.player[num48].active = false;
			}
			if (active != Main.player[num48].active)
			{
				if (Main.player[num48].active)
				{
					Player.Hooks.PlayerConnect(num48);
				}
				else
				{
					Player.Hooks.PlayerDisconnect(num48);
				}
			}
			break;
		}
		case 16:
		{
			int num164 = reader.ReadByte();
			if (num164 != Main.myPlayer || Main.ServerSideCharacter)
			{
				if (Main.netMode == 2)
				{
					num164 = whoAmI;
				}
				Player player15 = Main.player[num164];
				player15.statLife = reader.ReadInt16();
				player15.statLifeMax = reader.ReadInt16();
				if (player15.statLifeMax < 20)
				{
					player15.statLifeMax = 20;
				}
				player15.dead = player15.statLife <= 0;
				if (Main.netMode == 2)
				{
					NetMessage.TrySendData(16, -1, whoAmI, null, num164);
				}
			}
			break;
		}
		case 17:
		{
			byte b8 = reader.ReadByte();
			int num145 = reader.ReadInt16();
			int num146 = reader.ReadInt16();
			short num147 = reader.ReadInt16();
			int num148 = reader.ReadByte();
			bool flag13 = num147 == 1;
			if (!WorldGen.InWorld(num145, num146, 3))
			{
				break;
			}
			if (Main.tile[num145, num146] == null)
			{
				Main.tile[num145, num146] = new Tile();
			}
			if (Main.netMode == 2)
			{
				if (!flag13)
				{
					if (b8 == 0 || b8 == 2 || b8 == 4)
					{
						Netplay.Clients[whoAmI].SpamDeleteBlock += 1f;
					}
					if (b8 == 1 || b8 == 3)
					{
						Netplay.Clients[whoAmI].SpamAddBlock += 1f;
					}
				}
				if (!Netplay.Clients[whoAmI].TileSections[Netplay.GetSectionX(num145), Netplay.GetSectionY(num146)])
				{
					flag13 = true;
				}
			}
			MapUpdateQueue.Add(num145, num146);
			if (b8 == 0)
			{
				WorldGen.KillTile(num145, num146, flag13);
				if (Main.netMode == 1 && !flag13)
				{
					HitTile.ClearAllTilesAtThisLocation(num145, num146);
				}
			}
			bool flag14 = false;
			if (b8 == 1)
			{
				bool forced = true;
				if (WorldGen.CheckTileBreakability2_ShouldTileSurvive(num145, num146))
				{
					flag14 = true;
					forced = false;
				}
				WorldGen.PlaceTile(num145, num146, num147, mute: false, forced, -1, num148);
			}
			if (b8 == 2)
			{
				WorldGen.KillWall(num145, num146, flag13);
			}
			if (b8 == 3)
			{
				WorldGen.PlaceWall(num145, num146, num147);
			}
			if (b8 == 4)
			{
				WorldGen.KillTile(num145, num146, flag13, effectOnly: false, noItem: true);
			}
			if (b8 == 5)
			{
				WorldGen.PlaceWire(num145, num146);
			}
			if (b8 == 6)
			{
				WorldGen.KillWire(num145, num146);
			}
			if (b8 == 7)
			{
				WorldGen.PoundTile(num145, num146);
			}
			if (b8 == 8)
			{
				WorldGen.PlaceActuator(num145, num146);
			}
			if (b8 == 9)
			{
				WorldGen.KillActuator(num145, num146);
			}
			if (b8 == 10)
			{
				WorldGen.PlaceWire2(num145, num146);
			}
			if (b8 == 11)
			{
				WorldGen.KillWire2(num145, num146);
			}
			if (b8 == 12)
			{
				WorldGen.PlaceWire3(num145, num146);
			}
			if (b8 == 13)
			{
				WorldGen.KillWire3(num145, num146);
			}
			if (b8 == 14)
			{
				WorldGen.SlopeTile(num145, num146, num147);
			}
			if (b8 == 15)
			{
				Minecart.FrameTrack(num145, num146, pound: true);
			}
			if (b8 == 16)
			{
				WorldGen.PlaceWire4(num145, num146);
			}
			if (b8 == 17)
			{
				WorldGen.KillWire4(num145, num146);
			}
			switch (b8)
			{
			case 18:
				Wiring.SetCurrentUser(whoAmI);
				Wiring.PokeLogicGate(num145, num146);
				Wiring.SetCurrentUser();
				return;
			case 19:
				Wiring.SetCurrentUser(whoAmI);
				Wiring.Actuate(num145, num146);
				Wiring.SetCurrentUser();
				return;
			case 20:
				if (WorldGen.InWorld(num145, num146, 2))
				{
					int type15 = Main.tile[num145, num146].type;
					WorldGen.KillTile(num145, num146, flag13);
					num147 = (short)((Main.tile[num145, num146].active() && Main.tile[num145, num146].type == type15) ? 1 : 0);
					if (Main.netMode == 2)
					{
						NetMessage.TrySendData(17, -1, -1, null, b8, num145, num146, num147, num148);
					}
				}
				return;
			case 21:
				WorldGen.ReplaceTile(num145, num146, (ushort)num147, num148);
				break;
			}
			if (b8 == 22)
			{
				WorldGen.ReplaceWall(num145, num146, (ushort)num147);
			}
			if (b8 == 23 && WorldGen.CanPoundTile(num145, num146))
			{
				Main.tile[num145, num146].slope((byte)num147);
				WorldGen.PoundTile(num145, num146);
			}
			if (Main.netMode == 2)
			{
				if (flag14)
				{
					NetMessage.SendTileSquare(-1, num145, num146, 5);
				}
				else if ((b8 != 1 && b8 != 21) || !TileID.Sets.Falling[num147] || Main.tile[num145, num146].active())
				{
					NetMessage.TrySendData(17, -1, whoAmI, null, b8, num145, num146, num147, num148);
				}
			}
			break;
		}
		case 18:
			if (Main.netMode == 1)
			{
				Main.dayTime = reader.ReadByte() == 1;
				Main.time = reader.ReadInt32();
				Main.sunModY = reader.ReadInt16();
				Main.moonModY = reader.ReadInt16();
			}
			break;
		case 19:
		{
			byte b9 = reader.ReadByte();
			int num170 = reader.ReadInt16();
			int num171 = reader.ReadInt16();
			if (WorldGen.InWorld(num170, num171, 3))
			{
				int num172 = ((reader.ReadByte() != 0) ? 1 : (-1));
				switch (b9)
				{
				case 0:
					WorldGen.OpenDoor(num170, num171, num172);
					break;
				case 1:
					WorldGen.CloseDoor(num170, num171, forced: true);
					break;
				case 2:
					WorldGen.ShiftTrapdoor(num170, num171, num172 == 1, 1);
					break;
				case 3:
					WorldGen.ShiftTrapdoor(num170, num171, num172 == 1, 0);
					break;
				case 4:
					WorldGen.ShiftTallGate(num170, num171, closing: false, forced: true);
					break;
				case 5:
					WorldGen.ShiftTallGate(num170, num171, closing: true, forced: true);
					break;
				}
				if (Main.netMode == 2)
				{
					NetMessage.TrySendData(19, -1, whoAmI, null, b9, num170, num171, (num172 == 1) ? 1 : 0);
				}
			}
			break;
		}
		case 20:
		{
			int num62 = reader.ReadInt16();
			int num63 = reader.ReadInt16();
			ushort num64 = reader.ReadByte();
			ushort num65 = reader.ReadByte();
			byte b5 = reader.ReadByte();
			if (!WorldGen.InWorld(num62, num63, 3))
			{
				break;
			}
			TileChangeType type8 = TileChangeType.None;
			if (Enum.IsDefined(typeof(TileChangeType), b5))
			{
				type8 = (TileChangeType)b5;
			}
			if (MessageBuffer.OnTileChangeReceived != null)
			{
				MessageBuffer.OnTileChangeReceived(num62, num63, Math.Max(num64, num65), type8);
			}
			BitsByte bitsByte5 = (byte)0;
			BitsByte bitsByte6 = (byte)0;
			BitsByte bitsByte7 = (byte)0;
			Tile tile4 = null;
			for (int l = num62; l < num62 + num64; l++)
			{
				for (int m = num63; m < num63 + num65; m++)
				{
					if (Main.tile[l, m] == null)
					{
						Main.tile[l, m] = new Tile();
					}
					tile4 = Main.tile[l, m];
					bool flag4 = tile4.active();
					bitsByte5 = reader.ReadByte();
					bitsByte6 = reader.ReadByte();
					bitsByte7 = reader.ReadByte();
					tile4.active(bitsByte5[0]);
					tile4.wall = (byte)(bitsByte5[2] ? 1u : 0u);
					bool flag5 = bitsByte5[3];
					if (Main.netMode != 2)
					{
						tile4.liquid = (byte)(flag5 ? 1u : 0u);
					}
					tile4.wire(bitsByte5[4]);
					tile4.halfBrick(bitsByte5[5]);
					tile4.actuator(bitsByte5[6]);
					tile4.inActive(bitsByte5[7]);
					tile4.wire2(bitsByte6[0]);
					tile4.wire3(bitsByte6[1]);
					if (bitsByte6[2])
					{
						tile4.color(reader.ReadByte());
					}
					if (bitsByte6[3])
					{
						tile4.wallColor(reader.ReadByte());
					}
					if (tile4.active())
					{
						int type9 = tile4.type;
						tile4.type = reader.ReadUInt16();
						if (Main.tileFrameImportant[tile4.type])
						{
							tile4.frameX = reader.ReadInt16();
							tile4.frameY = reader.ReadInt16();
						}
						else if (!flag4 || tile4.type != type9)
						{
							tile4.frameX = -1;
							tile4.frameY = -1;
						}
						byte b6 = 0;
						if (bitsByte6[4])
						{
							b6++;
						}
						if (bitsByte6[5])
						{
							b6 += 2;
						}
						if (bitsByte6[6])
						{
							b6 += 4;
						}
						tile4.slope(b6);
					}
					tile4.wire4(bitsByte6[7]);
					tile4.fullbrightBlock(bitsByte7[0]);
					tile4.fullbrightWall(bitsByte7[1]);
					tile4.invisibleBlock(bitsByte7[2]);
					tile4.invisibleWall(bitsByte7[3]);
					if (tile4.wall > 0)
					{
						tile4.wall = reader.ReadUInt16();
					}
					if (flag5)
					{
						tile4.liquid = reader.ReadByte();
						tile4.liquidType(reader.ReadByte());
					}
				}
			}
			WorldGen.RangeFrame(num62, num63, num62 + num64, num63 + num65);
			if (Main.netMode == 2)
			{
				NetMessage.TrySendData(b, -1, whoAmI, null, num62, num63, (int)num64, (int)num65, b5);
			}
			break;
		}
		case 21:
		case 90:
		case 145:
		case 148:
		{
			int num201 = reader.ReadInt16();
			Vector2 position4 = reader.ReadVector2();
			Vector2 velocity3 = reader.ReadVector2();
			int stack7 = reader.ReadInt16();
			int prefix4 = reader.ReadByte();
			BitsByte bitsByte15 = reader.ReadByte();
			bool flag17 = bitsByte15[0];
			bool flag18 = bitsByte15[1];
			int num202 = reader.ReadInt16();
			bool shimmered = false;
			float shimmerTime = 0f;
			int timeLeftInWhichTheItemCannotBeTakenByEnemies = 0;
			if (b == 145)
			{
				shimmered = reader.ReadBoolean();
				shimmerTime = reader.ReadSingle();
			}
			if (b == 148)
			{
				timeLeftInWhichTheItemCannotBeTakenByEnemies = reader.ReadByte();
			}
			WorldItem worldItem4 = Main.item[num201];
			if (Main.netMode == 1)
			{
				ItemSyncPersistentStats itemSyncPersistentStats = default(ItemSyncPersistentStats);
				itemSyncPersistentStats.CopyFrom(worldItem4);
				bool newAndShiny = (worldItem4.newAndShiny || worldItem4.type != num202) && ItemSlot.Options.HighlightNewItems && (num202 < 0 || num202 >= ItemID.Count || !ItemID.Sets.NeverAppearsAsNewInInventory[num202]);
				worldItem4.SetDefaults(num202);
				worldItem4.newAndShiny = newAndShiny;
				worldItem4.Prefix(prefix4);
				worldItem4.stack = stack7;
				worldItem4.position = position4;
				worldItem4.velocity = velocity3;
				worldItem4.shimmered = shimmered;
				worldItem4.shimmerTime = shimmerTime;
				if (b == 90)
				{
					worldItem4.instanced = true;
					worldItem4.playerIndexTheItemIsReservedFor = Main.myPlayer;
					worldItem4.keepTime = 600;
				}
				else if (flag18)
				{
					worldItem4.keepTime = 100;
				}
				worldItem4.timeLeftInWhichTheItemCannotBeTakenByEnemies = timeLeftInWhichTheItemCannotBeTakenByEnemies;
				worldItem4.wet = Collision.WetCollision(worldItem4.position, worldItem4.width, worldItem4.height);
				itemSyncPersistentStats.PasteInto(worldItem4);
			}
			else
			{
				if (Main.timeItemSlotCannotBeReusedFor[num201] > 0)
				{
					break;
				}
				bool num203 = num201 == 400;
				if (num203)
				{
					Item item4 = new Item();
					item4.SetDefaults(num202);
					num201 = Item.NewItem(new EntitySource_Sync(), (int)position4.X, (int)position4.Y, item4.width, item4.height, item4.type, stack7, noBroadcast: true);
					worldItem4 = Main.item[num201];
					flag18 = (bitsByte15[1] = !flag17);
				}
				else
				{
					int timeSinceTheItemHasBeenReservedForSomeone = worldItem4.timeSinceTheItemHasBeenReservedForSomeone;
					if (worldItem4.playerIndexTheItemIsReservedFor != whoAmI)
					{
						timeSinceTheItemHasBeenReservedForSomeone = 0;
					}
					worldItem4.playerIndexTheItemIsReservedFor = 255;
					worldItem4.SetDefaults(num202);
					worldItem4.playerIndexTheItemIsReservedFor = whoAmI;
					worldItem4.timeSinceTheItemHasBeenReservedForSomeone = timeSinceTheItemHasBeenReservedForSomeone;
				}
				worldItem4.Prefix(prefix4);
				worldItem4.stack = stack7;
				worldItem4.position = position4;
				worldItem4.velocity = velocity3;
				worldItem4.timeLeftInWhichTheItemCannotBeTakenByEnemies = timeLeftInWhichTheItemCannotBeTakenByEnemies;
				if (b == 145)
				{
					worldItem4.shimmered = shimmered;
					worldItem4.shimmerTime = shimmerTime;
				}
				if (flag18)
				{
					worldItem4.ownIgnore = whoAmI;
					worldItem4.ownTime = 100;
				}
				if (num203)
				{
					NetMessage.TrySendData(b, -1, -1, null, num201, (int)(byte)bitsByte15);
					Main.item[num201].FindOwner();
				}
				else
				{
					NetMessage.TrySendData(b, -1, whoAmI, null, num201);
				}
			}
			break;
		}
		case 151:
		{
			int num52 = reader.ReadInt16();
			WorldItem worldItem = Main.item[num52];
			if ((Main.netMode != 2 || Main.timeItemSlotCannotBeReusedFor[num52] <= 0) && (Main.netMode != 2 || worldItem.playerIndexTheItemIsReservedFor == whoAmI))
			{
				worldItem.playerIndexTheItemIsReservedFor = 255;
				worldItem.TurnToAir();
				if (Main.netMode == 2)
				{
					NetMessage.TrySendData(151, -1, whoAmI, null, num52);
				}
			}
			break;
		}
		case 22:
		{
			int num179 = reader.ReadInt16();
			int num180 = reader.ReadByte();
			Vector2 position3 = reader.ReadVector2();
			WorldItem worldItem3 = Main.item[num179];
			if (Main.netMode != 2)
			{
				worldItem3.playerIndexTheItemIsReservedFor = num180;
				worldItem3.position = position3;
				if (num180 == Main.myPlayer)
				{
					worldItem3.keepTime = Math.Max(worldItem3.keepTime, 15);
				}
				else
				{
					worldItem3.keepTime = 0;
				}
			}
			break;
		}
		case 23:
		{
			if (Main.netMode != 1)
			{
				break;
			}
			int num215 = reader.ReadInt16();
			Vector2 vector6 = reader.ReadVector2();
			Vector2 velocity6 = reader.ReadVector2();
			int num216 = reader.ReadUInt16();
			if (num216 == 65535)
			{
				num216 = 0;
			}
			BitsByte bitsByte20 = reader.ReadByte();
			BitsByte bitsByte21 = reader.ReadByte();
			float[] array = ReUseTemporaryNPCAI();
			for (int num217 = 0; num217 < NPC.maxAI; num217++)
			{
				if (bitsByte20[num217 + 2])
				{
					array[num217] = reader.ReadSingle();
				}
				else
				{
					array[num217] = 0f;
				}
			}
			int num218 = reader.ReadInt16();
			int? playerCountForMultiplayerDifficultyOverride = 1;
			if (bitsByte21[0])
			{
				playerCountForMultiplayerDifficultyOverride = reader.ReadByte();
			}
			float value4 = 1f;
			if (bitsByte21[2])
			{
				value4 = reader.ReadSingle();
			}
			int num219 = 0;
			if (!bitsByte20[7])
			{
				num219 = reader.ReadByte() switch
				{
					2 => reader.ReadInt16(), 
					4 => reader.ReadInt32(), 
					_ => reader.ReadSByte(), 
				};
			}
			NPC nPC5 = Main.npc[num215];
			bool flag21 = bitsByte21[3] || !nPC5.active;
			int num220 = -1;
			if (flag21 || nPC5.netID != num218)
			{
				if (flag21)
				{
					nPC5.ResetForNewNPC();
				}
				else
				{
					num220 = nPC5.type;
				}
				nPC5.active = true;
				nPC5.SetDefaults(num218, new NPCSpawnParams
				{
					playerCountForMultiplayerDifficultyOverride = playerCountForMultiplayerDifficultyOverride,
					difficultyOverride = value4
				});
			}
			if (!flag21 && Vector2.DistanceSquared(nPC5.position, vector6) <= (float)(Main.multiplayerNPCSmoothingRange * Main.multiplayerNPCSmoothingRange))
			{
				nPC5.netOffset += nPC5.position - vector6;
				if (nPC5.netOffset != Vector2.Zero && DebugOptions.ShowNetOffsetDust && Vector2.Distance(vector6, nPC5.position) > 4f)
				{
					Dust.QuickDustLine(vector6, nPC5.position, 20f, Color.Red);
				}
			}
			nPC5.position = vector6;
			nPC5.velocity = velocity6;
			nPC5.target = num216;
			nPC5.direction = (bitsByte20[0] ? 1 : (-1));
			nPC5.directionY = (bitsByte20[1] ? 1 : (-1));
			nPC5.spriteDirection = (bitsByte20[6] ? 1 : (-1));
			if (bitsByte20[7])
			{
				num219 = (nPC5.life = nPC5.lifeMax);
			}
			else
			{
				nPC5.life = num219;
			}
			if (num219 <= 0)
			{
				nPC5.active = false;
			}
			nPC5.SpawnedFromStatue = bitsByte21[1];
			if (nPC5.SpawnedFromStatue)
			{
				nPC5.value = 0f;
			}
			if (bitsByte21[4])
			{
				nPC5.shimmerTransparency = 1f;
			}
			for (int num221 = 0; num221 < NPC.maxAI; num221++)
			{
				nPC5.ai[num221] = array[num221];
			}
			if (num220 > -1)
			{
				nPC5.TransformVisuals(num220, nPC5.type);
			}
			if (num218 == 262)
			{
				NPC.plantBoss = num215;
			}
			if (num218 == 245)
			{
				NPC.golemBoss = num215;
			}
			if (num218 == 668)
			{
				NPC.deerclopsBoss = num215;
			}
			if (nPC5.type >= 0 && nPC5.type < NPCID.Count && Main.npcCatchable[nPC5.type])
			{
				nPC5.releaseOwner = reader.ReadByte();
			}
			break;
		}
		case 24:
		{
			int num154 = reader.ReadInt16();
			int num155 = reader.ReadByte();
			if (Main.netMode == 2)
			{
				num155 = whoAmI;
			}
			Player player13 = Main.player[num155];
			Main.npc[num154].StrikeNPC(player13.inventory[player13.selectedItem].damage, player13.inventory[player13.selectedItem].knockBack, player13.direction);
			if (Main.netMode == 2)
			{
				NetMessage.TrySendData(24, -1, whoAmI, null, num154, num155);
				NetMessage.TrySendData(23, -1, -1, null, num154);
			}
			break;
		}
		case 27:
		{
			int num226 = reader.ReadInt16();
			Vector2 position5 = reader.ReadVector2();
			Vector2 velocity7 = reader.ReadVector2();
			int num227 = reader.ReadByte();
			int num228 = reader.ReadInt16();
			BitsByte bitsByte22 = reader.ReadByte();
			BitsByte bitsByte23 = (byte)(bitsByte22[2] ? reader.ReadByte() : 0);
			float[] array2 = ReUseTemporaryProjectileAI();
			array2[0] = (bitsByte22[0] ? reader.ReadSingle() : 0f);
			array2[1] = (bitsByte22[1] ? reader.ReadSingle() : 0f);
			int bannerIdToRespondTo = (bitsByte22[3] ? reader.ReadUInt16() : 0);
			int damage3 = (bitsByte22[4] ? reader.ReadInt16() : 0);
			float knockBack2 = (bitsByte22[5] ? reader.ReadSingle() : 0f);
			int originalDamage = (bitsByte22[6] ? reader.ReadInt16() : 0);
			int num229 = (bitsByte22[7] ? reader.ReadInt16() : (-1));
			if (num229 >= 1000)
			{
				num229 = -1;
			}
			array2[2] = (bitsByte23[0] ? reader.ReadSingle() : 0f);
			if (Main.netMode == 2)
			{
				if (num228 == 949)
				{
					num227 = 255;
				}
				else
				{
					num227 = whoAmI;
					if (Main.projHostile[num228])
					{
						break;
					}
				}
			}
			int num230 = 1000;
			for (int num231 = 0; num231 < 1000; num231++)
			{
				if (Main.projectile[num231].owner == num227 && Main.projectile[num231].identity == num226 && Main.projectile[num231].active)
				{
					num230 = num231;
					break;
				}
			}
			if (num230 == 1000)
			{
				for (int num232 = 0; num232 < 1000; num232++)
				{
					if (!Main.projectile[num232].active)
					{
						num230 = num232;
						break;
					}
				}
			}
			if (num230 == 1000)
			{
				num230 = Projectile.FindOldestProjectile();
			}
			Projectile projectile = Main.projectile[num230];
			if (!projectile.active || projectile.type != num228)
			{
				projectile.SetDefaults(num228);
				if (Main.netMode == 2)
				{
					Netplay.Clients[whoAmI].SpamProjectile += 1f;
				}
			}
			projectile.identity = num226;
			projectile.position = position5;
			projectile.velocity = velocity7;
			projectile.type = num228;
			projectile.damage = damage3;
			projectile.bannerIdToRespondTo = bannerIdToRespondTo;
			projectile.originalDamage = originalDamage;
			projectile.knockBack = knockBack2;
			projectile.owner = num227;
			for (int num233 = 0; num233 < Projectile.maxAI; num233++)
			{
				projectile.ai[num233] = array2[num233];
			}
			if (num229 >= 0)
			{
				projectile.projUUID = num229;
				Main.projectileIdentity[num227, num229] = num230;
			}
			projectile.ProjectileFixDesperation();
			if (Main.netMode == 2)
			{
				NetMessage.TrySendData(27, -1, whoAmI, null, num230);
			}
			break;
		}
		case 28:
		{
			int num211 = reader.ReadInt16();
			int num212 = reader.ReadInt16();
			float num213 = reader.ReadSingle();
			int num214 = reader.ReadByte() - 1;
			byte b14 = reader.ReadByte();
			if (Main.netMode == 2)
			{
				if (num212 < 0)
				{
					num212 = 0;
				}
				Main.npc[num211].PlayerInteraction(whoAmI);
			}
			if (num212 >= 0)
			{
				Main.npc[num211].StrikeNPC(num212, num213, num214, b14 == 1, noEffect: false, fromNet: true, (Main.netMode == 2) ? whoAmI : 255);
			}
			else
			{
				Main.npc[num211].life = 0;
				Main.npc[num211].HitEffect();
				Main.npc[num211].active = false;
			}
			if (Main.netMode == 2)
			{
				NetMessage.TrySendData(28, -1, whoAmI, null, num211, num212, num213, num214, b14);
				if (Main.npc[num211].life <= 0)
				{
					NetMessage.TrySendData(23, -1, -1, null, num211);
				}
				if (Main.npc[num211].realLife >= 0 && Main.npc[Main.npc[num211].realLife].life <= 0)
				{
					NetMessage.TrySendData(23, -1, -1, null, Main.npc[num211].realLife);
				}
			}
			break;
		}
		case 29:
		{
			int num161 = reader.ReadInt16();
			int num162 = reader.ReadByte();
			if (Main.netMode == 2)
			{
				num162 = whoAmI;
			}
			for (int num163 = 0; num163 < 1000; num163++)
			{
				if (Main.projectile[num163].owner == num162 && Main.projectile[num163].identity == num161 && Main.projectile[num163].active)
				{
					Main.projectile[num163].Kill();
					break;
				}
			}
			if (Main.netMode == 2)
			{
				NetMessage.TrySendData(29, -1, whoAmI, null, num161, num162);
			}
			break;
		}
		case 30:
		{
			int num76 = reader.ReadByte();
			if (Main.netMode == 2)
			{
				num76 = whoAmI;
			}
			bool flag6 = reader.ReadBoolean();
			Main.player[num76].hostile = flag6;
			if (Main.netMode == 2)
			{
				NetMessage.TrySendData(30, -1, whoAmI, null, num76);
				LocalizedText obj2 = (flag6 ? Lang.mp[11] : Lang.mp[12]);
				ChatHelper.BroadcastChatMessage(color: Main.teamColor[Main.player[num76].team], text: NetworkText.FromKey(obj2.Key, Main.player[num76].name));
			}
			break;
		}
		case 31:
		{
			if (Main.netMode != 2)
			{
				break;
			}
			int num29 = reader.ReadInt16();
			int num30 = reader.ReadInt16();
			int num31 = Chest.FindChest(num29, num30);
			if (num31 > -1 && Chest.UsingChest(num31) == -1)
			{
				NetMessage.SendChestContentsTo(num31, whoAmI);
				NetMessage.TrySendData(33, whoAmI, -1, null, num31);
				Main.player[whoAmI].chest = num31;
				if (Main.myPlayer == whoAmI)
				{
					Main.PipsUseGrid = false;
				}
				NetMessage.TrySendData(80, -1, whoAmI, null, whoAmI, num31);
				if (Main.netMode == 2 && WorldGen.IsChestRigged(num29, num30))
				{
					Wiring.SetCurrentUser(whoAmI);
					Wiring.HitSwitch(num29, num30);
					Wiring.SetCurrentUser();
					NetMessage.TrySendData(59, -1, whoAmI, null, num29, num30);
				}
			}
			break;
		}
		case 32:
		{
			int num27 = reader.ReadInt16();
			int num28 = reader.ReadByte();
			int stack2 = reader.ReadInt16();
			int prefixWeWant = reader.ReadByte();
			int type3 = reader.ReadInt16();
			if (num27 >= 0 && num27 < 8000 && Main.chest[num27] != null)
			{
				if (Main.chest[num27].item[num28] == null)
				{
					Main.chest[num27].item[num28] = new Item();
				}
				Main.chest[num27].item[num28].SetDefaults(type3);
				Main.chest[num27].item[num28].Prefix(prefixWeWant);
				Main.chest[num27].item[num28].stack = stack2;
				if (Main.netMode == 2)
				{
					NetMessage.TrySendData(32, -1, whoAmI, null, num27, num28);
				}
			}
			break;
		}
		case 33:
		{
			int num40 = reader.ReadInt16();
			int num41 = reader.ReadInt16();
			int num42 = reader.ReadInt16();
			int num43 = reader.ReadByte();
			string name = string.Empty;
			if (num43 != 0)
			{
				if (num43 <= 20)
				{
					name = reader.ReadString();
				}
				else if (num43 != 255)
				{
					num43 = 0;
				}
			}
			if (Main.netMode == 1)
			{
				Player player4 = Main.player[Main.myPlayer];
				if (player4.chest == -1)
				{
					Main.playerInventory = true;
					SoundEngine.PlaySound(10);
					if (num40 != -1)
					{
						ItemSlot.SetGlowForChest(Main.chest[num40]);
					}
				}
				else if (player4.chest != num40 && num40 != -1)
				{
					Main.playerInventory = true;
					SoundEngine.PlaySound(12);
					Main.PipsUseGrid = false;
					ItemSlot.SetGlowForChest(Main.chest[num40]);
				}
				else if (player4.chest != -1 && num40 == -1)
				{
					SoundEngine.PlaySound(11);
					Main.PipsUseGrid = false;
				}
				player4.chest = num40;
				player4.chestX = num41;
				player4.chestY = num42;
				if (Main.tile[num41, num42].frameX >= 36 && Main.tile[num41, num42].frameX < 72)
				{
					AchievementsHelper.HandleSpecialEvent(Main.player[Main.myPlayer], 16);
				}
			}
			else
			{
				if (num43 != 0)
				{
					int chest = Main.player[whoAmI].chest;
					Chest chest2 = Main.chest[chest];
					chest2.name = name;
					NetMessage.TrySendData(69, -1, whoAmI, null, chest, chest2.x, chest2.y);
				}
				Main.player[whoAmI].chest = num40;
				NetMessage.TrySendData(80, -1, whoAmI, null, whoAmI, num40);
			}
			break;
		}
		case 34:
		{
			byte b2 = reader.ReadByte();
			int num2 = reader.ReadInt16();
			int num3 = reader.ReadInt16();
			int num4 = reader.ReadInt16();
			int num5 = reader.ReadInt16();
			if (Main.netMode == 2)
			{
				num5 = 0;
			}
			if (Main.netMode == 2)
			{
				switch (b2)
				{
				case 0:
				{
					int num8 = WorldGen.PlaceChest(num2, num3, 21, notNearOtherChests: false, num4);
					if (num8 == -1)
					{
						NetMessage.TrySendData(34, whoAmI, -1, null, b2, num2, num3, num4, num8);
						int itemDrop_Chests2 = WorldGen.GetItemDrop_Chests(num4, secondType: false);
						if (itemDrop_Chests2 > 0)
						{
							Item.NewItem(new EntitySource_TileBreak(num2, num3), num2 * 16, num3 * 16, 32, 32, itemDrop_Chests2, 1, noBroadcast: true);
						}
					}
					else
					{
						NetMessage.TrySendData(34, -1, -1, null, b2, num2, num3, num4, num8);
					}
					break;
				}
				case 1:
					if (Main.tile[num2, num3].type == 21)
					{
						Tile tile = Main.tile[num2, num3];
						if (tile.frameX % 36 != 0)
						{
							num2--;
						}
						if (tile.frameY % 36 != 0)
						{
							num3--;
						}
						int number = Chest.FindChest(num2, num3);
						WorldGen.KillTile(num2, num3);
						if (!tile.active())
						{
							NetMessage.TrySendData(34, -1, -1, null, b2, num2, num3, 0f, number);
						}
						break;
					}
					goto default;
				default:
					switch (b2)
					{
					case 2:
					{
						int num6 = WorldGen.PlaceChest(num2, num3, 88, notNearOtherChests: false, num4);
						if (num6 == -1)
						{
							NetMessage.TrySendData(34, whoAmI, -1, null, b2, num2, num3, num4, num6);
							Item.NewItem(new EntitySource_TileBreak(num2, num3), num2 * 16, num3 * 16, 32, 32, WorldGen.GetItemDrop_Dressers(num4), 1, noBroadcast: true);
						}
						else
						{
							NetMessage.TrySendData(34, -1, -1, null, b2, num2, num3, num4, num6);
						}
						break;
					}
					case 3:
						if (Main.tile[num2, num3].type == 88)
						{
							Tile tile2 = Main.tile[num2, num3];
							num2 -= tile2.frameX % 54 / 18;
							if (tile2.frameY % 36 != 0)
							{
								num3--;
							}
							int number2 = Chest.FindChest(num2, num3);
							WorldGen.KillTile(num2, num3);
							if (!tile2.active())
							{
								NetMessage.TrySendData(34, -1, -1, null, b2, num2, num3, 0f, number2);
							}
							break;
						}
						goto default;
					default:
						switch (b2)
						{
						case 4:
						{
							int num7 = WorldGen.PlaceChest(num2, num3, 467, notNearOtherChests: false, num4);
							if (num7 == -1)
							{
								NetMessage.TrySendData(34, whoAmI, -1, null, b2, num2, num3, num4, num7);
								int itemDrop_Chests = WorldGen.GetItemDrop_Chests(num4, secondType: true);
								if (itemDrop_Chests > 0)
								{
									Item.NewItem(new EntitySource_TileBreak(num2, num3), num2 * 16, num3 * 16, 32, 32, itemDrop_Chests, 1, noBroadcast: true);
								}
							}
							else
							{
								NetMessage.TrySendData(34, -1, -1, null, b2, num2, num3, num4, num7);
							}
							break;
						}
						case 5:
							if (Main.tile[num2, num3].type == 467)
							{
								Tile tile3 = Main.tile[num2, num3];
								if (tile3.frameX % 36 != 0)
								{
									num2--;
								}
								if (tile3.frameY % 36 != 0)
								{
									num3--;
								}
								int number3 = Chest.FindChest(num2, num3);
								WorldGen.KillTile(num2, num3);
								if (!tile3.active())
								{
									NetMessage.TrySendData(34, -1, -1, null, b2, num2, num3, 0f, number3);
								}
							}
							break;
						}
						break;
					}
					break;
				}
				break;
			}
			switch (b2)
			{
			case 0:
				if (num5 == -1)
				{
					WorldGen.KillTile(num2, num3);
					break;
				}
				SoundEngine.PlaySound(0, num2 * 16, num3 * 16);
				WorldGen.PlaceChestDirect(num2, num3, 21, num4, num5);
				break;
			case 2:
				if (num5 == -1)
				{
					WorldGen.KillTile(num2, num3);
					break;
				}
				SoundEngine.PlaySound(0, num2 * 16, num3 * 16);
				WorldGen.PlaceDresserDirect(num2, num3, 88, num4, num5);
				break;
			case 4:
				if (num5 == -1)
				{
					WorldGen.KillTile(num2, num3);
					break;
				}
				SoundEngine.PlaySound(0, num2 * 16, num3 * 16);
				WorldGen.PlaceChestDirect(num2, num3, 467, num4, num5);
				break;
			default:
				Chest.DestroyChestDirect(num2, num3, num5);
				WorldGen.KillTile(num2, num3);
				break;
			}
			break;
		}
		case 35:
		{
			int num224 = reader.ReadByte();
			if (Main.netMode == 2)
			{
				num224 = whoAmI;
			}
			int num225 = reader.ReadInt16();
			if (num224 != Main.myPlayer || Main.ServerSideCharacter)
			{
				Main.player[num224].HealEffect(num225);
			}
			if (Main.netMode == 2)
			{
				NetMessage.TrySendData(35, -1, whoAmI, null, num224, num225);
			}
			break;
		}
		case 36:
		{
			int num175 = reader.ReadByte();
			if (Main.netMode == 2)
			{
				num175 = whoAmI;
			}
			Player player17 = Main.player[num175];
			bool flag15 = player17.zone5[0];
			player17.zone1 = reader.ReadByte();
			player17.zone2 = reader.ReadByte();
			player17.zone3 = reader.ReadByte();
			player17.zone4 = reader.ReadByte();
			player17.zone5 = reader.ReadByte();
			player17.townNPCs = reader.ReadByte();
			if (Main.netMode == 2)
			{
				if (!flag15 && player17.zone5[0])
				{
					NPC.Spawner.SpawnFaelings(player17);
				}
				NetMessage.TrySendData(36, -1, whoAmI, null, num175);
			}
			break;
		}
		case 37:
			if (Main.netMode == 1)
			{
				if (Main.autoPass)
				{
					NetMessage.TrySendData(38);
					Main.autoPass = false;
				}
				else
				{
					Netplay.ServerPassword = "";
					Main.menuMode = 31;
				}
			}
			break;
		case 38:
			if (Main.netMode == 2)
			{
				if (reader.ReadString() == Netplay.ServerPassword)
				{
					Netplay.Clients[whoAmI].State = 1;
					NetMessage.TrySendData(3, whoAmI);
				}
				else
				{
					NetMessage.TrySendData(2, whoAmI, -1, Lang.mp[1].ToNetworkText());
				}
			}
			break;
		case 39:
		{
			int num77 = reader.ReadInt16();
			WorldItem worldItem2 = Main.item[num77];
			if (Main.netMode == 1)
			{
				if (worldItem2.playerIndexTheItemIsReservedFor == Main.myPlayer)
				{
					worldItem2.playerIndexTheItemIsReservedFor = 255;
					NetMessage.TrySendData(39, -1, -1, null, num77);
				}
			}
			else if (worldItem2.playerIndexTheItemIsReservedFor == whoAmI)
			{
				worldItem2.playerIndexTheItemIsReservedFor = 255;
				worldItem2.FindOwner();
				if (worldItem2.playerIndexTheItemIsReservedFor == 255)
				{
					NetMessage.TrySendData(22, -1, whoAmI, null, num77);
				}
			}
			break;
		}
		case 40:
		{
			int num47 = reader.ReadByte();
			if (Main.netMode == 2)
			{
				num47 = whoAmI;
			}
			int talkNPC = reader.ReadInt16();
			Main.player[num47].SetTalkNPC(talkNPC);
			if (Main.netMode == 2)
			{
				NetMessage.TrySendData(40, -1, whoAmI, null, num47);
			}
			break;
		}
		case 41:
		{
			int num33 = reader.ReadByte();
			if (Main.netMode == 2)
			{
				num33 = whoAmI;
			}
			Player player = Main.player[num33];
			float itemRotation = reader.ReadSingle();
			int itemAnimation = reader.ReadInt16();
			player.itemRotation = itemRotation;
			player.itemAnimation = itemAnimation;
			player.channel = player.inventory[player.selectedItem].channel;
			if (Main.netMode == 2)
			{
				NetMessage.TrySendData(41, -1, whoAmI, null, num33);
			}
			break;
		}
		case 42:
		{
			int num267 = reader.ReadByte();
			if (Main.netMode == 2)
			{
				num267 = whoAmI;
			}
			else if (Main.myPlayer == num267 && !Main.ServerSideCharacter)
			{
				break;
			}
			int statMana = reader.ReadInt16();
			int statManaMax = reader.ReadInt16();
			Main.player[num267].statMana = statMana;
			Main.player[num267].statManaMax = statManaMax;
			break;
		}
		case 43:
		{
			int num239 = reader.ReadByte();
			if (Main.netMode == 2)
			{
				num239 = whoAmI;
			}
			int num240 = reader.ReadInt16();
			if (num239 != Main.myPlayer)
			{
				Main.player[num239].ManaEffect(num240);
			}
			if (Main.netMode == 2)
			{
				NetMessage.TrySendData(43, -1, whoAmI, null, num239, num240);
			}
			break;
		}
		case 45:
		case 157:
		{
			int num158 = reader.ReadByte();
			if (Main.netMode == 2)
			{
				num158 = whoAmI;
			}
			int num159 = reader.ReadByte();
			Player player14 = Main.player[num158];
			int team = player14.team;
			player14.team = num159;
			Color color = Main.teamColor[num159];
			if (Main.netMode != 2)
			{
				break;
			}
			NetMessage.TrySendData(45, -1, whoAmI, null, num158);
			LocalizedText localizedText = Lang.mp[13 + num159];
			if (num159 == 5)
			{
				localizedText = Lang.mp[22];
			}
			for (int num160 = 0; num160 < 255; num160++)
			{
				if (num160 == whoAmI || (team > 0 && Main.player[num160].team == team) || (num159 > 0 && Main.player[num160].team == num159))
				{
					ChatHelper.SendChatMessageToClient(NetworkText.FromKey(localizedText.Key, player14.name), color, num160);
				}
			}
			if (b == 157 && Main.teamBasedSpawnsSeed)
			{
				Point spawnPoint2 = Point.Zero;
				if (ExtraSpawnPointManager.TryGetExtraSpawnPointForTeam(num159, out spawnPoint2))
				{
					RemoteClient.CheckSection(whoAmI, spawnPoint2.ToWorldCoordinates());
					NetMessage.SendData(158, num158, -1, null, num158);
				}
			}
			break;
		}
		case 46:
			if (Main.netMode == 2)
			{
				short i3 = reader.ReadInt16();
				int j3 = reader.ReadInt16();
				int num149 = Sign.ReadSign(i3, j3);
				if (num149 >= 0)
				{
					NetMessage.TrySendData(47, whoAmI, -1, null, num149, whoAmI);
				}
			}
			break;
		case 47:
		{
			int num71 = reader.ReadInt16();
			int x7 = reader.ReadInt16();
			int y7 = reader.ReadInt16();
			string text2 = reader.ReadString();
			int num72 = reader.ReadByte();
			BitsByte bitsByte8 = reader.ReadByte();
			if (num71 >= 0 && num71 < 32000)
			{
				string text3 = null;
				if (Main.sign[num71] != null)
				{
					text3 = Main.sign[num71].text;
				}
				Main.sign[num71] = new Sign();
				Main.sign[num71].x = x7;
				Main.sign[num71].y = y7;
				Sign.TextSign(num71, text2);
				if (Main.netMode == 2 && text3 != text2)
				{
					num72 = whoAmI;
					NetMessage.TrySendData(47, -1, whoAmI, null, num71, num72);
				}
				if (Main.netMode == 1 && num72 == Main.myPlayer && Main.sign[num71] != null && !bitsByte8[0])
				{
					Main.LocalPlayer.OpenSign(num71);
				}
			}
			break;
		}
		case 48:
		{
			int num14 = reader.ReadInt16();
			int num15 = reader.ReadInt16();
			byte b3 = reader.ReadByte();
			byte liquidType = reader.ReadByte();
			if (Main.netMode == 2 && Netplay.SpamCheck)
			{
				int num16 = whoAmI;
				int num17 = (int)(Main.player[num16].position.X + (float)(Main.player[num16].width / 2));
				int num18 = (int)(Main.player[num16].position.Y + (float)(Main.player[num16].height / 2));
				int num19 = 10;
				int num20 = num17 - num19;
				int num21 = num17 + num19;
				int num22 = num18 - num19;
				int num23 = num18 + num19;
				if (num14 < num20 || num14 > num21 || num15 < num22 || num15 > num23)
				{
					Netplay.Clients[whoAmI].SpamWater += 1f;
				}
			}
			if (Main.tile[num14, num15] == null)
			{
				Main.tile[num14, num15] = new Tile();
			}
			lock (Main.tile[num14, num15])
			{
				Main.tile[num14, num15].liquid = b3;
				Main.tile[num14, num15].liquidType(liquidType);
				if (Main.netMode == 2)
				{
					WorldGen.SquareTileFrame(num14, num15);
					if (b3 == 0)
					{
						NetMessage.SendData(48, -1, whoAmI, null, num14, num15);
					}
				}
				break;
			}
		}
		case 49:
			if (Netplay.Connection.State == 6)
			{
				Netplay.Connection.State = 10;
				Main.player[Main.myPlayer].Spawn(PlayerSpawnContext.SpawningIntoWorld);
			}
			break;
		case 50:
		{
			int num242 = reader.ReadByte();
			if (Main.netMode == 2)
			{
				num242 = whoAmI;
			}
			else if (num242 == Main.myPlayer && !Main.ServerSideCharacter)
			{
				break;
			}
			Player player20 = Main.player[num242];
			int num243 = 0;
			int num244;
			while ((num244 = reader.ReadUInt16()) > 0)
			{
				player20.buffType[num243] = num244;
				player20.buffTime[num243] = 60;
				num243++;
			}
			Array.Clear(player20.buffType, num243, player20.buffType.Length - num243);
			Array.Clear(player20.buffTime, num243, player20.buffTime.Length - num243);
			if (Main.netMode == 2)
			{
				NetMessage.TrySendData(50, -1, whoAmI, null, num242);
			}
			break;
		}
		case 51:
		{
			byte b15 = reader.ReadByte();
			if (Main.netMode == 2)
			{
				b15 = (byte)whoAmI;
			}
			byte b16 = reader.ReadByte();
			switch (b16)
			{
			case 1:
				NPC.SpawnSkeletron(b15);
				break;
			case 2:
				if (Main.netMode == 2)
				{
					NetMessage.TrySendData(51, -1, whoAmI, null, b15, (int)b16);
				}
				else
				{
					SoundEngine.PlaySound(SoundID.Item1, (int)Main.player[b15].position.X, (int)Main.player[b15].position.Y);
				}
				break;
			case 3:
				if (Main.netMode == 2)
				{
					Main.Sundialing();
				}
				break;
			case 4:
				Main.npc[b15].BigMimicSpawnSmoke();
				break;
			case 5:
				if (Main.netMode == 2)
				{
					NPC nPC6 = new NPC();
					nPC6.SetDefaults(664);
					Main.BestiaryTracker.Kills.RegisterKill(nPC6);
				}
				break;
			case 6:
				if (Main.netMode == 2)
				{
					Main.Moondialing();
				}
				break;
			}
			break;
		}
		case 52:
		{
			int num181 = reader.ReadByte();
			int num182 = reader.ReadInt16();
			int num183 = reader.ReadInt16();
			if (num181 == 1)
			{
				Chest.Unlock(num182, num183);
				if (Main.netMode == 2)
				{
					NetMessage.TrySendData(52, -1, whoAmI, null, 0, num181, num182, num183);
					NetMessage.SendTileSquare(-1, num182, num183, 2);
				}
			}
			if (num181 == 2)
			{
				WorldGen.UnlockDoor(num182, num183);
				if (Main.netMode == 2)
				{
					NetMessage.TrySendData(52, -1, whoAmI, null, 0, num181, num182, num183);
					NetMessage.SendTileSquare(-1, num182, num183, 2);
				}
			}
			if (num181 == 3)
			{
				Chest.Lock(num182, num183);
				if (Main.netMode == 2)
				{
					NetMessage.TrySendData(52, -1, whoAmI, null, 0, num181, num182, num183);
					NetMessage.SendTileSquare(-1, num182, num183, 2);
				}
			}
			break;
		}
		case 53:
		{
			int num176 = reader.ReadInt16();
			int type17 = reader.ReadUInt16();
			int time2 = reader.ReadInt16();
			Main.npc[num176].AddBuff(type17, time2, quiet: true);
			if (Main.netMode == 2)
			{
				NetMessage.TrySendData(54, -1, -1, null, num176);
			}
			break;
		}
		case 54:
			if (Main.netMode == 1)
			{
				int num151 = reader.ReadInt16();
				NPC nPC2 = Main.npc[num151];
				int num152 = 0;
				int num153;
				while ((num153 = reader.ReadUInt16()) > 0)
				{
					nPC2.buffType[num152] = num153;
					nPC2.buffTime[num152] = reader.ReadUInt16();
					num152++;
				}
				Array.Clear(nPC2.buffType, num152, nPC2.buffType.Length - num152);
				Array.Clear(nPC2.buffTime, num152, nPC2.buffTime.Length - num152);
			}
			break;
		case 55:
		{
			int num87 = reader.ReadByte();
			int num88 = reader.ReadUInt16();
			int num89 = reader.ReadInt32();
			if ((Main.netMode != 2 || Main.pvpBuff[num88]) && (Main.netMode != 1 || num87 == Main.myPlayer))
			{
				if (Main.netMode == 2)
				{
					NetMessage.TrySendData(55, num87, -1, null, num87, num88, num89);
				}
				else
				{
					Main.player[num87].AddBuff(num88, num89, fromNetPvP: true);
				}
			}
			break;
		}
		case 56:
		{
			int num74 = reader.ReadInt16();
			if (num74 >= 0 && num74 < Main.maxNPCs)
			{
				if (Main.netMode == 1)
				{
					string givenName = reader.ReadString();
					Main.npc[num74].GivenName = givenName;
					int townNpcVariationIndex = reader.ReadInt32();
					Main.npc[num74].townNpcVariationIndex = townNpcVariationIndex;
				}
				else if (Main.netMode == 2)
				{
					NetMessage.TrySendData(56, whoAmI, -1, null, num74);
				}
			}
			break;
		}
		case 57:
			if (Main.netMode == 1)
			{
				WorldGen.tGood = reader.ReadByte();
				WorldGen.tEvil = reader.ReadByte();
				WorldGen.tBlood = reader.ReadByte();
			}
			break;
		case 58:
		{
			int num58 = reader.ReadByte();
			if (Main.netMode == 2)
			{
				num58 = whoAmI;
			}
			float num59 = reader.ReadSingle();
			if (Main.netMode == 2)
			{
				NetMessage.TrySendData(58, -1, whoAmI, null, whoAmI, num59);
				break;
			}
			Player player6 = Main.player[num58];
			int type5 = player6.inventory[player6.selectedItem].type;
			switch (type5)
			{
			case 4057:
			case 4372:
			case 4715:
				player6.PlayGuitarChord(num59);
				break;
			case 4673:
				player6.PlayDrums(num59);
				break;
			default:
			{
				Main.musicPitch = num59;
				LegacySoundStyle type6 = SoundID.Item26;
				if (type5 == 507)
				{
					type6 = SoundID.Item35;
				}
				if (type5 == 1305)
				{
					type6 = SoundID.Item47;
				}
				SoundEngine.PlaySound(type6, player6.position);
				break;
			}
			}
			break;
		}
		case 59:
		{
			int num66 = reader.ReadInt16();
			int num67 = reader.ReadInt16();
			Wiring.SetCurrentUser(whoAmI);
			Wiring.HitSwitch(num66, num67);
			Wiring.SetCurrentUser();
			if (Main.netMode == 2)
			{
				NetMessage.TrySendData(59, -1, whoAmI, null, num66, num67);
			}
			break;
		}
		case 60:
		{
			int num268 = reader.ReadInt16();
			int num269 = reader.ReadInt16();
			int num270 = reader.ReadInt16();
			byte b17 = reader.ReadByte();
			if (num268 >= Main.maxNPCs)
			{
				NetMessage.BootPlayer(whoAmI, NetworkText.FromKey("Net.CheatingInvalid"));
				break;
			}
			NPC nPC7 = Main.npc[num268];
			bool isLikeATownNPC = nPC7.isLikeATownNPC;
			if (Main.netMode == 1)
			{
				nPC7.homeless = b17 == 1;
				nPC7.homeTileX = num269;
				nPC7.homeTileY = num270;
			}
			if (!isLikeATownNPC)
			{
				break;
			}
			if (Main.netMode == 1)
			{
				switch (b17)
				{
				case 1:
					WorldGen.TownManager.KickOut(nPC7.type);
					break;
				case 2:
					WorldGen.TownManager.SetRoom(nPC7.type, num269, num270);
					break;
				}
			}
			else if (b17 == 1)
			{
				WorldGen.kickOut(num268);
			}
			else
			{
				WorldGen.moveRoom(num269, num270, num268);
			}
			break;
		}
		case 61:
		{
			int num259 = reader.ReadInt16();
			int num260 = reader.ReadInt16();
			if (Main.netMode != 2)
			{
				break;
			}
			if (num260 >= 0 && num260 < NPCID.Count && NPCID.Sets.MPAllowedEnemies[num260])
			{
				if (!NPC.AnyNPCs(num260))
				{
					NPC.SpawnOnPlayer(num259, num260);
				}
			}
			else if (num260 == -4)
			{
				if (!Main.dayTime && !DD2Event.Ongoing)
				{
					ChatHelper.BroadcastChatMessage(NetworkText.FromKey(Lang.misc[31].Key), new Color(50, 255, 130));
					Main.startPumpkinMoon();
					NetMessage.TrySendData(7);
					NetMessage.TrySendData(78, -1, -1, null, 0, 1f, 2f, 1f);
				}
			}
			else if (num260 == -5)
			{
				if (!Main.dayTime && !DD2Event.Ongoing)
				{
					ChatHelper.BroadcastChatMessage(NetworkText.FromKey(Lang.misc[34].Key), new Color(50, 255, 130));
					Main.startSnowMoon();
					NetMessage.TrySendData(7);
					NetMessage.TrySendData(78, -1, -1, null, 0, 1f, 1f, 1f);
				}
			}
			else if (num260 == -6)
			{
				if (Main.dayTime && !Main.eclipse)
				{
					if (Main.remixWorld)
					{
						ChatHelper.BroadcastChatMessage(NetworkText.FromKey(Lang.misc[106].Key), new Color(50, 255, 130));
					}
					else
					{
						ChatHelper.BroadcastChatMessage(NetworkText.FromKey(Lang.misc[20].Key), new Color(50, 255, 130));
					}
					Main.eclipse = true;
					NetMessage.TrySendData(7);
				}
			}
			else if (num260 == -7)
			{
				Main.invasionDelay = 0;
				Main.StartInvasion(4);
				NetMessage.TrySendData(7);
				NetMessage.TrySendData(78, -1, -1, null, 0, 1f, Main.invasionType + 3);
			}
			else if (num260 == -8)
			{
				if (NPC.downedGolemBoss && Main.hardMode && !NPC.AnyDanger() && !NPC.AnyoneNearCultists())
				{
					WorldGen.StartImpendingDoom(720);
					NetMessage.TrySendData(7);
				}
			}
			else if (num260 == -10)
			{
				if (!Main.dayTime && !Main.bloodMoon)
				{
					ChatHelper.BroadcastChatMessage(NetworkText.FromKey(Lang.misc[8].Key), new Color(50, 255, 130));
					Main.bloodMoon = true;
					if (Main.GetMoonPhase() == MoonPhase.Empty)
					{
						Main.moonPhase = 5;
					}
					AchievementsHelper.NotifyProgressionEvent(4);
					NetMessage.TrySendData(7);
				}
			}
			else if (num260 == -11)
			{
				ChatHelper.BroadcastChatMessage(NetworkText.FromKey("Misc.CombatBookUsed"), new Color(50, 255, 130));
				NPC.combatBookWasUsed = true;
				NetMessage.TrySendData(7);
			}
			else if (num260 == -12)
			{
				NPC.UnlockOrExchangePet(ref NPC.boughtCat, 637, "Misc.LicenseCatUsed", num260);
			}
			else if (num260 == -13)
			{
				NPC.UnlockOrExchangePet(ref NPC.boughtDog, 638, "Misc.LicenseDogUsed", num260);
			}
			else if (num260 == -14)
			{
				NPC.UnlockOrExchangePet(ref NPC.boughtBunny, 656, "Misc.LicenseBunnyUsed", num260);
			}
			else if (num260 == -15)
			{
				NPC.UnlockOrExchangePet(ref NPC.unlockedSlimeBlueSpawn, 670, "Misc.LicenseSlimeUsed", num260);
			}
			else if (num260 == -16)
			{
				NPC.SpawnMechQueen(num259);
			}
			else if (num260 == -17)
			{
				ChatHelper.BroadcastChatMessage(NetworkText.FromKey("Misc.CombatBookVolumeTwoUsed"), new Color(50, 255, 130));
				NPC.combatBookVolumeTwoWasUsed = true;
				NetMessage.TrySendData(7);
			}
			else if (num260 == -18)
			{
				ChatHelper.BroadcastChatMessage(NetworkText.FromKey("Misc.PeddlersSatchelUsed"), new Color(50, 255, 130));
				NPC.peddlersSatchelWasUsed = true;
				NetMessage.TrySendData(7);
			}
			else if (num260 == -19)
			{
				Main.StartSlimeRain();
			}
			else if (num260 < 0)
			{
				int num261 = 1;
				if (num260 > -InvasionID.Count)
				{
					num261 = -num260;
				}
				if (num261 > 0 && Main.invasionType == 0)
				{
					Main.invasionDelay = 0;
					Main.StartInvasion(num261);
				}
				NetMessage.TrySendData(7);
				NetMessage.TrySendData(78, -1, -1, null, 0, 1f, Main.invasionType + 3);
			}
			break;
		}
		case 62:
		{
			int num222 = reader.ReadByte();
			int num223 = reader.ReadByte();
			if (Main.netMode == 2)
			{
				num222 = whoAmI;
			}
			if (num223 == 1)
			{
				Main.player[num222].NinjaDodge();
			}
			if (num223 == 2)
			{
				Main.player[num222].ShadowDodge();
			}
			if (num223 == 4)
			{
				Main.player[num222].BrainOfConfusionDodge();
			}
			if (Main.netMode == 2)
			{
				NetMessage.TrySendData(62, -1, whoAmI, null, num222, num223);
			}
			break;
		}
		case 63:
		{
			int num193 = reader.ReadInt16();
			int num194 = reader.ReadInt16();
			byte b12 = reader.ReadByte();
			byte b13 = reader.ReadByte();
			if (b13 == 0)
			{
				WorldGen.paintTile(num193, num194, b12);
			}
			else
			{
				WorldGen.paintCoatTile(num193, num194, b12);
			}
			if (Main.netMode == 2)
			{
				NetMessage.TrySendData(63, -1, whoAmI, null, num193, num194, (int)b12, (int)b13);
			}
			break;
		}
		case 64:
		{
			int num173 = reader.ReadInt16();
			int num174 = reader.ReadInt16();
			byte b10 = reader.ReadByte();
			byte b11 = reader.ReadByte();
			if (b11 == 0)
			{
				WorldGen.paintWall(num173, num174, b10);
			}
			else
			{
				WorldGen.paintCoatWall(num173, num174, b10);
			}
			if (Main.netMode == 2)
			{
				NetMessage.TrySendData(64, -1, whoAmI, null, num173, num174, (int)b10, (int)b11);
			}
			break;
		}
		case 65:
		{
			BitsByte bitsByte10 = reader.ReadByte();
			int num134 = reader.ReadInt16();
			if (Main.netMode == 2)
			{
				num134 = whoAmI;
			}
			Vector2 vector3 = reader.ReadVector2();
			int num135 = 0;
			num135 = reader.ReadByte();
			int num136 = 0;
			if (bitsByte10[0])
			{
				num136++;
			}
			if (bitsByte10[1])
			{
				num136 += 2;
			}
			bool flag11 = false;
			if (bitsByte10[2])
			{
				flag11 = true;
			}
			int num137 = 0;
			if (bitsByte10[3])
			{
				num137 = reader.ReadInt32();
			}
			if (flag11)
			{
				vector3 = Main.player[num134].position;
			}
			switch (num136)
			{
			case 0:
				Main.player[num134].Teleport(vector3, num135, num137);
				if (Main.netMode == 2)
				{
					NetMessage.TrySendData(65, -1, whoAmI, null, 0, num134, vector3.X, vector3.Y, num135, flag11.ToInt(), num137);
				}
				if (Main.netMode == 1 && num134 == Main.myPlayer)
				{
					NetMessage.TrySendData(65, -1, -1, null, 3, num134);
				}
				break;
			case 1:
				Main.npc[num134].Teleport(vector3, num135, num137);
				Main.npc[num134].netOffset *= 0f;
				break;
			case 2:
			{
				Main.player[num134].Teleport(vector3, num135, num137);
				if (Main.netMode != 2)
				{
					break;
				}
				RemoteClient.CheckSection(whoAmI, vector3);
				NetMessage.TrySendData(65, -1, -1, null, 0, num134, vector3.X, vector3.Y, num135, flag11.ToInt(), num137);
				int num138 = -1;
				float num139 = 9999f;
				for (int num140 = 0; num140 < 255; num140++)
				{
					if (Main.player[num140].active && num140 != whoAmI)
					{
						Vector2 vector4 = Main.player[num140].position - Main.player[whoAmI].position;
						if (vector4.Length() < num139)
						{
							num139 = vector4.Length();
							num138 = num140;
						}
					}
				}
				if (num138 >= 0)
				{
					ChatHelper.BroadcastChatMessage(NetworkText.FromKey("Game.HasTeleportedTo", Main.player[whoAmI].name, Main.player[num138].name), new Color(250, 250, 0));
				}
				break;
			}
			case 3:
				Main.player[num134].unacknowledgedTeleports--;
				break;
			}
			break;
		}
		case 66:
		{
			int num80 = reader.ReadByte();
			int num81 = reader.ReadInt16();
			if (num81 > 0)
			{
				Player player8 = Main.player[num80];
				player8.statLife += num81;
				if (player8.statLife > player8.statLifeMax2)
				{
					player8.statLife = player8.statLifeMax2;
				}
				player8.HealEffect(num81, broadcast: false);
				if (Main.netMode == 2)
				{
					NetMessage.TrySendData(66, -1, whoAmI, null, num80, num81);
				}
			}
			break;
		}
		case 68:
			reader.ReadString();
			break;
		case 69:
		{
			int num44 = reader.ReadInt16();
			int num45 = reader.ReadInt16();
			int num46 = reader.ReadInt16();
			if (Main.netMode == 1)
			{
				if (num44 >= 0 && num44 < 8000)
				{
					Chest chest3 = Main.chest[num44];
					if (chest3 == null)
					{
						chest3 = Chest.CreateWorldChest(num44, num45, num46);
					}
					else if (chest3.x != num45 || chest3.y != num46)
					{
						break;
					}
					chest3.name = reader.ReadString();
				}
			}
			else
			{
				if (num44 < -1 || num44 >= 8000)
				{
					break;
				}
				if (num44 == -1)
				{
					num44 = Chest.FindChest(num45, num46);
					if (num44 == -1)
					{
						break;
					}
				}
				Chest chest4 = Main.chest[num44];
				if (chest4.x == num45 && chest4.y == num46)
				{
					NetMessage.TrySendData(69, whoAmI, -1, null, num44, num45, num46);
				}
			}
			break;
		}
		case 70:
			if (Main.netMode == 2)
			{
				int num34 = reader.ReadInt16();
				int who = reader.ReadByte();
				if (Main.netMode == 2)
				{
					who = whoAmI;
				}
				if (num34 < Main.maxNPCs && num34 >= 0)
				{
					NPC.CatchNPC(num34, who);
				}
			}
			break;
		case 71:
			if (Main.netMode == 2)
			{
				int x2 = reader.ReadInt32();
				int y2 = reader.ReadInt32();
				int type2 = reader.ReadInt16();
				byte style = reader.ReadByte();
				NPC.ReleaseNPC(x2, y2, type2, style, whoAmI);
			}
			break;
		case 72:
			if (Main.netMode == 1)
			{
				for (int num273 = 0; num273 < Main.TravelShopMaxSlots; num273++)
				{
					Main.travelShop[num273] = reader.ReadInt16();
				}
			}
			break;
		case 73:
			switch (reader.ReadByte())
			{
			case 0:
				Main.player[whoAmI].TeleportationPotion();
				break;
			case 1:
				Main.player[whoAmI].MagicConch();
				break;
			case 2:
				Main.player[whoAmI].DemonConch();
				break;
			case 3:
				Main.player[whoAmI].Shellphone_Spawn();
				break;
			case 4:
				Main.player[whoAmI].PlayerNoSpaceTeleport();
				break;
			}
			break;
		case 74:
			if (Main.netMode == 1)
			{
				Main.anglerQuest = reader.ReadByte();
				Main.anglerQuestFinished = reader.ReadBoolean();
			}
			break;
		case 75:
			if (Main.netMode == 2)
			{
				string name2 = Main.player[whoAmI].name;
				if (!Main.anglerWhoFinishedToday.Contains(name2))
				{
					Main.anglerWhoFinishedToday.Add(name2);
				}
			}
			break;
		case 76:
		{
			int num241 = reader.ReadByte();
			if (num241 != Main.myPlayer || Main.ServerSideCharacter)
			{
				if (Main.netMode == 2)
				{
					num241 = whoAmI;
				}
				Player obj6 = Main.player[num241];
				obj6.anglerQuestsFinished = reader.ReadInt32();
				obj6.golferScoreAccumulated = reader.ReadInt32();
				if (Main.netMode == 2)
				{
					NetMessage.TrySendData(76, -1, whoAmI, null, num241);
				}
			}
			break;
		}
		case 77:
		{
			short type20 = reader.ReadInt16();
			ushort tileType = reader.ReadUInt16();
			short x15 = reader.ReadInt16();
			short y15 = reader.ReadInt16();
			Animation.NewTemporaryAnimation(type20, tileType, x15, y15);
			break;
		}
		case 78:
			if (Main.netMode == 1)
			{
				Main.ReportInvasionProgress(reader.ReadInt32(), reader.ReadInt32(), reader.ReadSByte(), reader.ReadSByte());
			}
			break;
		case 79:
		{
			int x13 = reader.ReadInt16();
			int y13 = reader.ReadInt16();
			short type18 = reader.ReadInt16();
			int style2 = reader.ReadInt16();
			int num204 = reader.ReadByte();
			int random = reader.ReadSByte();
			int direction = (reader.ReadBoolean() ? 1 : (-1));
			if (Main.netMode == 2)
			{
				Netplay.Clients[whoAmI].SpamAddBlock += 1f;
				if (!WorldGen.InWorld(x13, y13, 10) || !Netplay.Clients[whoAmI].TileSections[Netplay.GetSectionX(x13), Netplay.GetSectionY(y13)])
				{
					break;
				}
			}
			WorldGen.PlaceObject(x13, y13, type18, mute: false, style2, num204, random, direction);
			if (Main.netMode == 2)
			{
				NetMessage.SendObjectPlacement(whoAmI, x13, y13, type18, style2, num204, random, direction);
			}
			break;
		}
		case 80:
			if (Main.netMode == 1)
			{
				int num190 = reader.ReadByte();
				int num191 = reader.ReadInt16();
				if (num191 >= -3 && num191 < 8000)
				{
					Main.player[num190].chest = num191;
				}
			}
			break;
		case 81:
			if (Main.netMode == 1)
			{
				int x12 = (int)reader.ReadSingle();
				int y12 = (int)reader.ReadSingle();
				CombatText.NewText(color: reader.ReadRGB(), amount: reader.ReadInt32(), location: new Rectangle(x12, y12, 0, 0));
			}
			break;
		case 119:
			if (Main.netMode == 1)
			{
				int x11 = (int)reader.ReadSingle();
				int y11 = (int)reader.ReadSingle();
				CombatText.NewText(color: reader.ReadRGB(), text: NetworkText.Deserialize(reader).ToString(), location: new Rectangle(x11, y11, 0, 0));
			}
			break;
		case 82:
			NetManager.Instance.Read(reader, whoAmI, length);
			break;
		case 84:
		{
			int num165 = reader.ReadByte();
			if (Main.netMode == 2)
			{
				num165 = whoAmI;
			}
			float stealth = reader.ReadSingle();
			Main.player[num165].stealth = stealth;
			if (Main.netMode == 2)
			{
				NetMessage.TrySendData(84, -1, whoAmI, null, num165);
			}
			break;
		}
		case 85:
			if (Main.netMode == 2 && whoAmI < 255)
			{
				Player player16 = Main.player[whoAmI];
				QuickStacking.SourceInventory inventory = QuickStacking.ReadNetInventory(player16, reader);
				bool smartStack = reader.ReadBoolean();
				QuickStacking.QuickStackToNearbyChests(player16, inventory, smartStack);
			}
			else if (Main.netMode == 1)
			{
				QuickStacking.IndicateBlockedChests(Main.LocalPlayer, QuickStacking.ReadBlockedChestList(reader));
			}
			break;
		case 86:
		{
			if (Main.netMode != 1)
			{
				break;
			}
			int num150 = reader.ReadInt32();
			if (!reader.ReadBoolean())
			{
				if (TileEntity.TryGet<TileEntity>(num150, out var result3))
				{
					TileEntity.Remove(result3);
				}
			}
			else
			{
				TileEntity tileEntity = TileEntity.Read(reader, 318, networkSend: true);
				tileEntity.ID = num150;
				TileEntity.Add(tileEntity);
			}
			break;
		}
		case 87:
			if (Main.netMode == 2)
			{
				int x10 = reader.ReadInt16();
				int y10 = reader.ReadInt16();
				int type14 = reader.ReadByte();
				if (WorldGen.InWorld(x10, y10) && !TileEntity.TryGetAt<TileEntity>(x10, y10, out var _))
				{
					TileEntity.PlaceEntityNet(x10, y10, type14);
				}
			}
			break;
		case 88:
		{
			if (Main.netMode != 1)
			{
				break;
			}
			int num12 = reader.ReadInt16();
			if (num12 < 0 || num12 > 400)
			{
				break;
			}
			Item inner = Main.item[num12].inner;
			BitsByte bitsByte = reader.ReadByte();
			if (bitsByte[0])
			{
				inner.color.PackedValue = reader.ReadUInt32();
			}
			if (bitsByte[1])
			{
				inner.damage = reader.ReadUInt16();
			}
			if (bitsByte[2])
			{
				inner.knockBack = reader.ReadSingle();
			}
			if (bitsByte[3])
			{
				inner.useAnimation = reader.ReadUInt16();
			}
			if (bitsByte[4])
			{
				inner.useTime = reader.ReadUInt16();
			}
			if (bitsByte[5])
			{
				inner.shoot = reader.ReadInt16();
			}
			if (bitsByte[6])
			{
				inner.shootSpeed = reader.ReadSingle();
			}
			if (bitsByte[7])
			{
				bitsByte = reader.ReadByte();
				if (bitsByte[0])
				{
					inner.width = reader.ReadInt16();
				}
				if (bitsByte[1])
				{
					inner.height = reader.ReadInt16();
				}
				if (bitsByte[2])
				{
					inner.scale = reader.ReadSingle();
				}
				if (bitsByte[3])
				{
					inner.ammo = reader.ReadInt16();
				}
				if (bitsByte[4])
				{
					inner.useAmmo = reader.ReadInt16();
				}
				if (bitsByte[5])
				{
					inner.notAmmo = reader.ReadBoolean();
				}
			}
			break;
		}
		case 89:
			if (Main.netMode == 2)
			{
				short x = reader.ReadInt16();
				int y = reader.ReadInt16();
				int type = reader.ReadInt16();
				int prefix = reader.ReadByte();
				int stack = reader.ReadInt16();
				TEItemFrame.TryPlacing(x, y, type, prefix, stack);
			}
			break;
		case 91:
		{
			if (Main.netMode != 1)
			{
				break;
			}
			int num262 = reader.ReadInt32();
			int num263 = reader.ReadByte();
			if (num263 == 255)
			{
				if (EmoteBubble.byID.ContainsKey(num262))
				{
					EmoteBubble.byID.Remove(num262);
				}
				break;
			}
			int num264 = reader.ReadUInt16();
			int num265 = reader.ReadUInt16();
			int num266 = reader.ReadByte();
			int metadata = 0;
			if (num266 < 0)
			{
				metadata = reader.ReadInt16();
			}
			WorldUIAnchor worldUIAnchor = EmoteBubble.DeserializeNetAnchor(num263, num264);
			if (num263 == 1)
			{
				Main.player[num264].emoteTime = 360;
			}
			lock (EmoteBubble.byID)
			{
				if (!EmoteBubble.byID.ContainsKey(num262))
				{
					EmoteBubble.byID[num262] = new EmoteBubble(num266, worldUIAnchor, num265);
				}
				else
				{
					EmoteBubble.byID[num262].lifeTime = num265;
					EmoteBubble.byID[num262].lifeTimeStart = num265;
					EmoteBubble.byID[num262].emote = num266;
					EmoteBubble.byID[num262].anchor = worldUIAnchor;
				}
				EmoteBubble.byID[num262].ID = num262;
				EmoteBubble.byID[num262].metadata = metadata;
				EmoteBubble.OnBubbleChange(num262);
				break;
			}
		}
		case 92:
		{
			int num250 = reader.ReadInt16();
			int num251 = reader.ReadInt32();
			float num252 = reader.ReadSingle();
			float num253 = reader.ReadSingle();
			if (num250 >= 0 && num250 <= Main.maxNPCs)
			{
				if (Main.netMode == 1)
				{
					Main.npc[num250].moneyPing(new Vector2(num252, num253));
					Main.npc[num250].extraValue = num251;
				}
				else
				{
					Main.npc[num250].extraValue += num251;
					NetMessage.TrySendData(92, -1, -1, null, num250, Main.npc[num250].extraValue, num252, num253);
				}
			}
			break;
		}
		case 94:
		{
			string text5 = reader.ReadString();
			reader.ReadInt32();
			int num254 = (int)reader.ReadSingle();
			reader.ReadSingle();
			if (DebugOptions.enableDebugCommands)
			{
				if (text5 == "/showdebug")
				{
					DebugOptions.Shared_ReportCommandUsage = num254 == 1;
				}
				else if (text5 == "/setserverping")
				{
					DebugOptions.Shared_ServerPing = num254;
					DebugNetworkStream.Latency = (uint)(num254 / 2);
				}
			}
			break;
		}
		case 95:
		{
			ushort num234 = reader.ReadUInt16();
			int num235 = reader.ReadByte();
			if (Main.netMode != 2)
			{
				break;
			}
			for (int num236 = 0; num236 < 1000; num236++)
			{
				if (Main.projectile[num236].owner == num234 && Main.projectile[num236].active && Main.projectile[num236].type == 602 && Main.projectile[num236].ai[1] == (float)num235)
				{
					Main.projectile[num236].Kill();
					NetMessage.TrySendData(29, -1, -1, null, Main.projectile[num236].identity, (int)num234);
					break;
				}
			}
			break;
		}
		case 96:
		{
			int num208 = reader.ReadByte();
			if (Main.netMode == 2)
			{
				num208 = whoAmI;
			}
			Player obj5 = Main.player[num208];
			int num209 = reader.ReadInt16();
			Vector2 newPos2 = reader.ReadVector2();
			Vector2 velocity4 = reader.ReadVector2();
			int lastPortalColorIndex2 = num209 + ((num209 % 2 == 0) ? 1 : (-1));
			obj5.lastPortalColorIndex = lastPortalColorIndex2;
			obj5.Teleport(newPos2, 4, num209);
			obj5.velocity = velocity4;
			if (Main.netMode == 2)
			{
				NetMessage.SendData(96, -1, num208, null, num208, newPos2.X, newPos2.Y, num209);
			}
			break;
		}
		case 97:
			if (Main.netMode == 1)
			{
				AchievementsHelper.NotifyNPCKilledDirect(Main.player[Main.myPlayer], reader.ReadInt16());
			}
			break;
		case 98:
			if (Main.netMode == 1)
			{
				AchievementsHelper.NotifyProgressionEvent(reader.ReadInt16());
			}
			break;
		case 99:
		{
			int num192 = reader.ReadByte();
			if (Main.netMode == 2)
			{
				num192 = whoAmI;
			}
			Main.player[num192].MinionRestTargetPoint = reader.ReadVector2();
			if (Main.netMode == 2)
			{
				NetMessage.TrySendData(99, -1, whoAmI, null, num192);
			}
			break;
		}
		case 115:
		{
			int num184 = reader.ReadByte();
			if (Main.netMode == 2)
			{
				num184 = whoAmI;
			}
			Main.player[num184].MinionAttackTargetNPC = reader.ReadInt16();
			if (Main.netMode == 2)
			{
				NetMessage.TrySendData(115, -1, whoAmI, null, num184);
			}
			break;
		}
		case 100:
		{
			int num177 = reader.ReadUInt16();
			NPC obj4 = Main.npc[num177];
			int num178 = reader.ReadInt16();
			Vector2 newPos = reader.ReadVector2();
			Vector2 velocity2 = reader.ReadVector2();
			int lastPortalColorIndex = num178 + ((num178 % 2 == 0) ? 1 : (-1));
			obj4.lastPortalColorIndex = lastPortalColorIndex;
			obj4.Teleport(newPos, 4, num178);
			obj4.velocity = velocity2;
			obj4.netOffset *= 0f;
			break;
		}
		case 101:
			if (Main.netMode != 2)
			{
				NPC.ShieldStrengthTowerSolar = reader.ReadUInt16();
				NPC.ShieldStrengthTowerVortex = reader.ReadUInt16();
				NPC.ShieldStrengthTowerNebula = reader.ReadUInt16();
				NPC.ShieldStrengthTowerStardust = reader.ReadUInt16();
				if (NPC.ShieldStrengthTowerSolar < 0)
				{
					NPC.ShieldStrengthTowerSolar = 0;
				}
				if (NPC.ShieldStrengthTowerVortex < 0)
				{
					NPC.ShieldStrengthTowerVortex = 0;
				}
				if (NPC.ShieldStrengthTowerNebula < 0)
				{
					NPC.ShieldStrengthTowerNebula = 0;
				}
				if (NPC.ShieldStrengthTowerStardust < 0)
				{
					NPC.ShieldStrengthTowerStardust = 0;
				}
				if (NPC.ShieldStrengthTowerSolar > NPC.LunarShieldPowerMax)
				{
					NPC.ShieldStrengthTowerSolar = NPC.LunarShieldPowerMax;
				}
				if (NPC.ShieldStrengthTowerVortex > NPC.LunarShieldPowerMax)
				{
					NPC.ShieldStrengthTowerVortex = NPC.LunarShieldPowerMax;
				}
				if (NPC.ShieldStrengthTowerNebula > NPC.LunarShieldPowerMax)
				{
					NPC.ShieldStrengthTowerNebula = NPC.LunarShieldPowerMax;
				}
				if (NPC.ShieldStrengthTowerStardust > NPC.LunarShieldPowerMax)
				{
					NPC.ShieldStrengthTowerStardust = NPC.LunarShieldPowerMax;
				}
			}
			break;
		case 102:
		{
			int num125 = reader.ReadByte();
			ushort num126 = reader.ReadUInt16();
			Vector2 other = reader.ReadVector2();
			if (Main.netMode == 2)
			{
				num125 = whoAmI;
				NetMessage.TrySendData(102, -1, -1, null, num125, (int)num126, other.X, other.Y);
				break;
			}
			Player player10 = Main.player[num125];
			for (int num127 = 0; num127 < 255; num127++)
			{
				Player player11 = Main.player[num127];
				if (!player11.active || player11.dead || (player10.team != 0 && player10.team != player11.team) || !(player11.Distance(other) < 700f))
				{
					continue;
				}
				Vector2 value3 = player10.Center - player11.Center;
				Vector2 vector = Vector2.Normalize(value3);
				if (!vector.HasNaNs())
				{
					int type12 = 90;
					float num128 = 0f;
					float num129 = (float)Math.PI / 15f;
					Vector2 spinningpoint = new Vector2(0f, -8f);
					Vector2 vector2 = new Vector2(-3f);
					float num130 = 0f;
					float num131 = 0.005f;
					switch (num126)
					{
					case 179:
						type12 = 86;
						break;
					case 173:
						type12 = 90;
						break;
					case 176:
						type12 = 88;
						break;
					}
					for (int num132 = 0; (float)num132 < value3.Length() / 6f; num132++)
					{
						Vector2 position2 = player11.Center + 6f * (float)num132 * vector + spinningpoint.RotatedBy(num128) + vector2;
						num128 += num129;
						int num133 = Dust.NewDust(position2, 6, 6, type12, 0f, 0f, 100, default(Color), 1.5f);
						Main.dust[num133].noGravity = true;
						Main.dust[num133].velocity = Vector2.Zero;
						num130 = (Main.dust[num133].fadeIn = num130 + num131);
						Main.dust[num133].velocity += vector * 1.5f;
					}
				}
				player11.NebulaLevelup(num126);
			}
			break;
		}
		case 103:
			if (Main.netMode == 1)
			{
				NPC.MaxMoonLordCountdown = reader.ReadInt32();
				NPC.MoonLordCountdown = reader.ReadInt32();
			}
			break;
		case 104:
			if (Main.netMode == 1 && Main.npcShop > 0)
			{
				Item[] item3 = Main.instance.shop[Main.npcShop].item;
				int num86 = reader.ReadByte();
				int type11 = reader.ReadInt16();
				int stack5 = reader.ReadInt16();
				int prefixWeWant3 = reader.ReadByte();
				int value = reader.ReadInt32();
				BitsByte bitsByte9 = reader.ReadByte();
				if (num86 < item3.Length)
				{
					item3[num86] = new Item();
					item3[num86].SetDefaults(type11);
					item3[num86].stack = stack5;
					item3[num86].Prefix(prefixWeWant3);
					item3[num86].value = value;
					item3[num86].buyOnce = bitsByte9[0];
				}
			}
			break;
		case 105:
			if (Main.netMode != 1)
			{
				short i2 = reader.ReadInt16();
				int j2 = reader.ReadInt16();
				bool flag8 = reader.ReadBoolean();
				WorldGen.ToggleGemLock(i2, j2, flag8);
			}
			break;
		case 106:
			if (Main.netMode == 1)
			{
				HalfVector2 halfVector = new HalfVector2
				{
					PackedValue = reader.ReadUInt32()
				};
				Utils.PoofOfSmoke(halfVector.ToVector2());
			}
			break;
		case 107:
			if (Main.netMode == 1)
			{
				Color c = reader.ReadRGB();
				string text4 = NetworkText.Deserialize(reader).ToString();
				int widthLimit = reader.ReadInt16();
				Main.NewTextMultiline(text4, force: false, c, widthLimit);
			}
			break;
		case 108:
			if (Main.netMode == 1)
			{
				int damage2 = reader.ReadInt16();
				float knockBack = reader.ReadSingle();
				int x8 = reader.ReadInt16();
				int y8 = reader.ReadInt16();
				int angle = reader.ReadInt16();
				int ammo = reader.ReadInt16();
				int num73 = reader.ReadByte();
				if (num73 == Main.myPlayer)
				{
					WorldGen.ShootFromCannon(x8, y8, angle, ammo, damage2, knockBack, num73, fromWire: true);
				}
			}
			break;
		case 109:
			if (Main.netMode == 2)
			{
				short x5 = reader.ReadInt16();
				int y5 = reader.ReadInt16();
				int x6 = reader.ReadInt16();
				int y6 = reader.ReadInt16();
				byte toolMode = reader.ReadByte();
				int num70 = whoAmI;
				WiresUI.Settings.MultiToolMode toolMode2 = WiresUI.Settings.ToolMode;
				WiresUI.Settings.ToolMode = (WiresUI.Settings.MultiToolMode)toolMode;
				Wiring.MassWireOperation(new Point(x5, y5), new Point(x6, y6), Main.player[num70]);
				WiresUI.Settings.ToolMode = toolMode2;
			}
			break;
		case 110:
		{
			if (Main.netMode != 1)
			{
				break;
			}
			int type7 = reader.ReadInt16();
			int num60 = reader.ReadInt16();
			int num61 = reader.ReadByte();
			if (num61 == Main.myPlayer)
			{
				Player player7 = Main.player[num61];
				for (int k = 0; k < num60; k++)
				{
					player7.ConsumeItem(type7);
				}
				player7.wireOperationsCooldown = 0;
			}
			break;
		}
		case 111:
			if (Main.netMode == 2)
			{
				BirthdayParty.ToggleManualParty();
			}
			break;
		case 112:
		{
			int num53 = reader.ReadByte();
			int num54 = reader.ReadInt32();
			int num55 = reader.ReadInt32();
			int num56 = reader.ReadByte();
			int num57 = reader.ReadInt16();
			bool flag3 = reader.ReadByte() == 1;
			switch (num53)
			{
			case 1:
				if (Main.netMode == 1)
				{
					WorldGen.TreeGrowFX(num54, num55, num56, num57, flag3);
				}
				if (Main.netMode == 2)
				{
					NetMessage.TrySendData(b, -1, -1, null, num53, num54, num55, num56, num57, flag3 ? 1 : 0);
				}
				break;
			case 2:
				NPC.FairyEffects(new Vector2(num54, num55), num57);
				break;
			}
			break;
		}
		case 113:
		{
			int x3 = reader.ReadInt16();
			int y3 = reader.ReadInt16();
			if (Main.netMode == 2 && !Main.snowMoon && !Main.pumpkinMoon)
			{
				if (DD2Event.WouldFailSpawningHere(x3, y3))
				{
					DD2Event.FailureMessage(whoAmI);
				}
				DD2Event.SummonCrystal(x3, y3, whoAmI);
			}
			break;
		}
		case 114:
			if (Main.netMode == 1)
			{
				DD2Event.WipeEntities();
			}
			break;
		case 116:
			if (Main.netMode == 1)
			{
				DD2Event.TimeLeftBetweenWaves = reader.ReadInt32();
			}
			break;
		case 117:
		{
			int num24 = reader.ReadByte();
			if (Main.netMode != 2 || whoAmI == num24 || (Main.player[num24].hostile && Main.player[whoAmI].hostile))
			{
				PlayerDeathReason playerDeathReason2 = PlayerDeathReason.FromReader(reader);
				int damage = reader.ReadInt16();
				int num25 = reader.ReadByte() - 1;
				BitsByte bitsByte2 = reader.ReadByte();
				bool flag = bitsByte2[0];
				bool pvp2 = bitsByte2[1];
				int num26 = reader.ReadSByte();
				Main.player[num24].Hurt(playerDeathReason2, damage, num25, pvp2, quiet: true, flag, num26);
				if (Main.netMode == 2)
				{
					NetMessage.SendPlayerHurt(num24, playerDeathReason2, damage, num25, flag, pvp2, num26, -1, whoAmI);
				}
			}
			break;
		}
		case 118:
		{
			int num9 = reader.ReadByte();
			if (Main.netMode == 2)
			{
				num9 = whoAmI;
			}
			PlayerDeathReason playerDeathReason = PlayerDeathReason.FromReader(reader);
			int num10 = reader.ReadInt16();
			int num11 = reader.ReadByte() - 1;
			bool pvp = ((BitsByte)reader.ReadByte())[0];
			Main.player[num9].KillMe(playerDeathReason, num10, num11, pvp);
			if (Main.netMode == 2)
			{
				NetMessage.SendPlayerDeath(num9, playerDeathReason, num10, num11, pvp, -1, whoAmI);
			}
			break;
		}
		case 120:
		{
			int num271 = reader.ReadByte();
			if (Main.netMode == 2)
			{
				num271 = whoAmI;
			}
			int num272 = reader.ReadByte();
			if (num272 >= 0 && num272 < EmoteID.Count && Main.netMode == 2)
			{
				EmoteBubble.NewBubble(num272, new WorldUIAnchor(Main.player[num271]), 360);
				EmoteBubble.CheckForNPCsToReactToEmoteBubble(num272, Main.player[num271]);
			}
			break;
		}
		case 121:
		{
			int num255 = reader.ReadByte();
			if (Main.netMode == 2)
			{
				num255 = whoAmI;
			}
			int num256 = reader.ReadInt32();
			int num257 = reader.ReadByte();
			int num258 = reader.ReadByte();
			if (!TileEntity.TryGet<TEDisplayDoll>(num256, out var result7))
			{
				TEDisplayDoll.ReadDummySync(num257, num258, reader);
				break;
			}
			result7.ReadData(num257, num258, reader);
			if (Main.netMode == 2)
			{
				NetMessage.TrySendData(b, -1, num255, null, num255, num256, num257, num258);
			}
			break;
		}
		case 122:
		{
			int num237 = reader.ReadInt32();
			int num238 = reader.ReadByte();
			if (Main.netMode == 2)
			{
				num238 = whoAmI;
			}
			if (Main.netMode == 2)
			{
				if (num237 == -1)
				{
					Main.player[num238].tileEntityAnchor.Clear();
					NetMessage.TrySendData(b, -1, -1, null, num237, num238);
					break;
				}
				if (!TileEntity.IsOccupied(num237, out var _) && TileEntity.TryGet<TileEntity>(num237, out var result5))
				{
					Main.player[num238].tileEntityAnchor.Set(num237, result5.Position.X, result5.Position.Y);
					NetMessage.TrySendData(b, -1, -1, null, num237, num238);
				}
			}
			if (Main.netMode == 1)
			{
				TileEntity result6;
				if (num237 == -1)
				{
					Main.player[num238].tileEntityAnchor.Clear();
				}
				else if (TileEntity.TryGet<TileEntity>(num237, out result6))
				{
					TileEntity.SetInteractionAnchor(Main.player[num238], result6.Position.X, result6.Position.Y, num237);
				}
			}
			break;
		}
		case 123:
			if (Main.netMode == 2)
			{
				short x14 = reader.ReadInt16();
				int y14 = reader.ReadInt16();
				int type19 = reader.ReadInt16();
				int prefix5 = reader.ReadByte();
				int stack8 = reader.ReadInt16();
				TEWeaponsRack.TryPlacing(x14, y14, type19, prefix5, stack8);
			}
			break;
		case 124:
		{
			int num205 = reader.ReadByte();
			if (Main.netMode == 2)
			{
				num205 = whoAmI;
			}
			int num206 = reader.ReadInt32();
			int num207 = reader.ReadByte();
			bool flag20 = false;
			if (num207 >= 2)
			{
				flag20 = true;
				num207 -= 2;
			}
			if (!TileEntity.TryGet<TEHatRack>(num206, out var result4) || num207 >= 2)
			{
				reader.ReadInt32();
				reader.ReadByte();
				break;
			}
			result4.ReadItem(num207, reader, flag20);
			if (Main.netMode == 2)
			{
				NetMessage.TrySendData(b, -1, num205, null, num205, num206, num207, flag20.ToInt());
			}
			break;
		}
		case 125:
		{
			int num195 = reader.ReadByte();
			int num196 = reader.ReadInt16();
			int num197 = reader.ReadInt16();
			int num198 = reader.ReadByte();
			if (Main.netMode == 2)
			{
				num195 = whoAmI;
			}
			if (Main.netMode == 1)
			{
				Main.player[Main.myPlayer].GetOtherPlayersPickTile(num196, num197, num198);
			}
			if (Main.netMode == 2)
			{
				NetMessage.TrySendData(125, -1, num195, null, num195, num196, num197, num198);
			}
			break;
		}
		case 126:
			if (Main.netMode == 1)
			{
				NPC.RevengeManager.AddMarkerFromReader(reader);
			}
			break;
		case 127:
		{
			int markerUniqueID = reader.ReadInt32();
			if (Main.netMode == 1)
			{
				NPC.RevengeManager.DestroyMarker(markerUniqueID);
			}
			break;
		}
		case 128:
		{
			int num185 = reader.ReadByte();
			int num186 = reader.ReadUInt16();
			int num187 = reader.ReadUInt16();
			int num188 = reader.ReadUInt16();
			int num189 = reader.ReadUInt16();
			if (Main.netMode == 2)
			{
				NetMessage.SendData(128, -1, num185, null, num185, num188, num189, 0f, num186, num187);
			}
			else
			{
				GolfHelper.ContactListener.PutBallInCup_TextAndEffects(new Point(num186, num187), num185, num188, num189);
			}
			break;
		}
		case 129:
			if (Main.netMode == 1)
			{
				if (Main.LocalPlayer.team > 0)
				{
					NetMessage.SendData(45, -1, -1, null, Main.myPlayer);
				}
				Main.FixUIScale();
				Main.TrySetPreparationState(Main.WorldPreparationState.ProcessingData);
			}
			break;
		case 130:
		{
			if (Main.netMode != 2)
			{
				break;
			}
			int num166 = reader.ReadUInt16();
			int num167 = reader.ReadUInt16();
			int num168 = reader.ReadInt16();
			if (num168 == 682)
			{
				if (NPC.unlockedSlimeRedSpawn)
				{
					break;
				}
				NPC.unlockedSlimeRedSpawn = true;
				NetMessage.TrySendData(7);
			}
			num166 *= 16;
			num167 *= 16;
			NPC nPC4 = new NPC();
			nPC4.SetDefaults(num168);
			int type16 = nPC4.type;
			int netID = nPC4.netID;
			int num169 = NPC.NewNPC(new EntitySource_FishedOut(Main.player[whoAmI]), num166, num167, num168);
			if (netID != type16)
			{
				Main.npc[num169].SetDefaults(netID);
				NetMessage.TrySendData(23, -1, -1, null, num169);
			}
			if (num168 == 682)
			{
				WorldGen.CheckAchievement_RealEstateAndTownSlimes();
			}
			break;
		}
		case 131:
			if (Main.netMode == 1)
			{
				int num156 = reader.ReadUInt16();
				NPC nPC3 = null;
				nPC3 = ((num156 >= Main.maxNPCs) ? new NPC() : Main.npc[num156]);
				int num157 = reader.ReadByte();
				if (num157 == 1)
				{
					int time = reader.ReadInt32();
					int fromWho = reader.ReadInt16();
					nPC3.GetImmuneTime(fromWho, time);
				}
			}
			break;
		case 132:
			if (Main.netMode == 1)
			{
				Point point2 = reader.ReadVector2().ToPoint();
				ushort key = reader.ReadUInt16();
				LegacySoundStyle legacySoundStyle = SoundID.SoundByIndex[key];
				BitsByte bitsByte11 = reader.ReadByte();
				int num141 = -1;
				float num142 = 1f;
				float num143 = 0f;
				SoundEngine.PlaySound(Style: (!bitsByte11[0]) ? legacySoundStyle.Style : reader.ReadInt32(), volumeScale: (!bitsByte11[1]) ? legacySoundStyle.Volume : MathHelper.Clamp(reader.ReadSingle(), 0f, 1f), pitchOffset: (!bitsByte11[2]) ? legacySoundStyle.GetRandomPitch() : MathHelper.Clamp(reader.ReadSingle(), -1f, 1f), type: legacySoundStyle.SoundId, x: point2.X, y: point2.Y);
			}
			break;
		case 133:
			if (Main.netMode == 2)
			{
				short x9 = reader.ReadInt16();
				int y9 = reader.ReadInt16();
				int type13 = reader.ReadInt16();
				int prefix3 = reader.ReadByte();
				int stack6 = reader.ReadInt16();
				TEFoodPlatter.TryPlacing(x9, y9, type13, prefix3, stack6);
			}
			break;
		case 134:
		{
			int num94 = reader.ReadByte();
			int ladyBugLuckTimeLeft = reader.ReadInt32();
			float torchLuck = reader.ReadSingle();
			byte luckPotion = reader.ReadByte();
			bool hasGardenGnomeNearby = reader.ReadBoolean();
			bool brokenMirrorBadLuck = reader.ReadBoolean();
			float equipmentBasedLuckBonus = reader.ReadSingle();
			float coinLuck = reader.ReadSingle();
			byte kiteLuckLevel = reader.ReadByte();
			if (Main.netMode == 2)
			{
				num94 = whoAmI;
			}
			Player obj3 = Main.player[num94];
			obj3.ladyBugLuckTimeLeft = ladyBugLuckTimeLeft;
			obj3.torchLuck = torchLuck;
			obj3.luckPotion = luckPotion;
			obj3.HasGardenGnomeNearby = hasGardenGnomeNearby;
			obj3.brokenMirrorBadLuck = brokenMirrorBadLuck;
			obj3.equipmentBasedLuckBonus = equipmentBasedLuckBonus;
			obj3.coinLuck = coinLuck;
			obj3.kiteLuckLevel = kiteLuckLevel;
			obj3.RecalculateLuck();
			if (Main.netMode == 2)
			{
				NetMessage.SendData(134, -1, num94, null, num94);
			}
			break;
		}
		case 135:
		{
			int num93 = reader.ReadByte();
			if (Main.netMode == 1)
			{
				Main.player[num93].immuneAlpha = 255;
			}
			break;
		}
		case 136:
		{
			for (int n = 0; n < 2; n++)
			{
				for (int num90 = 0; num90 < 3; num90++)
				{
					NPC.cavernMonsterType[n, num90] = reader.ReadUInt16();
				}
			}
			break;
		}
		case 137:
			if (Main.netMode == 2)
			{
				int num85 = reader.ReadInt16();
				int buffTypeToRemove = reader.ReadUInt16();
				if (num85 >= 0 && num85 < Main.maxNPCs)
				{
					Main.npc[num85].RequestBuffRemoval(buffTypeToRemove);
				}
			}
			break;
		case 139:
			if (Main.netMode != 2)
			{
				int num84 = reader.ReadByte();
				bool flag7 = reader.ReadBoolean();
				Main.countsAsHostForGameplay[num84] = flag7;
			}
			break;
		case 140:
		{
			int num82 = reader.ReadByte();
			int num83 = reader.ReadInt32();
			switch (num82)
			{
			case 0:
				if (Main.netMode == 1)
				{
					CreditsRollEvent.SetRemainingTimeDirect(num83);
				}
				break;
			case 1:
				if (Main.netMode == 2)
				{
					NPC.TransformCopperSlime(num83);
				}
				break;
			case 2:
				if (Main.netMode == 2)
				{
					NPC.TransformElderSlime(num83);
				}
				break;
			}
			break;
		}
		case 141:
		{
			LucyAxeMessage.MessageSource messageSource = (LucyAxeMessage.MessageSource)reader.ReadByte();
			byte b7 = reader.ReadByte();
			Vector2 velocity = reader.ReadVector2();
			int num78 = reader.ReadInt32();
			int num79 = reader.ReadInt32();
			if (Main.netMode == 2)
			{
				NetMessage.SendData(141, -1, whoAmI, null, (int)messageSource, (int)b7, velocity.X, velocity.Y, num78, num79);
			}
			else
			{
				LucyAxeMessage.CreateFromNet(messageSource, b7, new Vector2(num78, num79), velocity);
			}
			break;
		}
		case 142:
		{
			int num75 = reader.ReadByte();
			if (Main.netMode == 2)
			{
				num75 = whoAmI;
			}
			Player obj = Main.player[num75];
			obj.piggyBankProjTracker.TryReading(reader);
			obj.voidLensChest.TryReading(reader);
			if (Main.netMode == 2)
			{
				NetMessage.TrySendData(142, -1, whoAmI, null, num75);
			}
			break;
		}
		case 143:
			if (Main.netMode == 2)
			{
				DD2Event.AttemptToSkipWaitTime();
			}
			break;
		case 144:
			if (Main.netMode == 2)
			{
				NPC.HaveDryadDoStardewAnimation();
			}
			break;
		case 146:
			switch (reader.ReadByte())
			{
			case 0:
				WorldItem.ShimmerEffect(reader.ReadVector2());
				break;
			case 1:
			{
				Vector2 coinPosition = reader.ReadVector2();
				int coinAmount = reader.ReadInt32();
				Main.player[Main.myPlayer].AddCoinLuck(coinPosition, coinAmount);
				break;
			}
			}
			break;
		case 147:
		{
			int num68 = reader.ReadByte();
			if (Main.netMode == 2)
			{
				num68 = whoAmI;
			}
			int num69 = reader.ReadByte();
			Main.player[num68].TrySwitchingLoadout(num69);
			ReadAccessoryVisibility(reader, Main.player[num68].hideVisibleAccessory);
			if (Main.netMode == 2)
			{
				NetMessage.TrySendData(b, -1, num68, null, num68, num69);
			}
			break;
		}
		case 149:
			if (Main.netMode == 2)
			{
				short x4 = reader.ReadInt16();
				int y4 = reader.ReadInt16();
				int type10 = reader.ReadInt16();
				int prefix2 = reader.ReadByte();
				int stack4 = reader.ReadInt16();
				TEDeadCellsDisplayJar.TryPlacing(x4, y4, type10, prefix2, stack4);
			}
			break;
		case 150:
		{
			int num50 = reader.ReadByte();
			if (Main.netMode == 2)
			{
				num50 = whoAmI;
			}
			int num51 = reader.ReadInt16();
			Player player5 = Main.player[num50];
			if (Main.netMode == 2)
			{
				if (num51 >= 0)
				{
					player5.SetOrRequestSpectating(num51);
					break;
				}
				player5.spectating = -1;
				NetMessage.SendData(150, -1, whoAmI, null, whoAmI, num51);
			}
			else if (player5 != Main.LocalPlayer || player5.spectating >= 0)
			{
				player5.spectating = num51;
			}
			break;
		}
		case 152:
		{
			int num39 = reader.ReadByte();
			if (Main.netMode == 2)
			{
				num39 = whoAmI;
			}
			if (Main.netMode == 2)
			{
				NetMessage.TrySendData(152, -1, whoAmI, null, num39);
			}
			if (Main.netMode == 1)
			{
				Player player3 = Main.player[num39];
				Item item2 = player3.inventory[player3.selectedItem];
				if (item2.UseSound != null)
				{
					SoundEngine.PlaySound(item2.UseSound, player3.Center, item2.useSoundPitch);
				}
			}
			break;
		}
		case 153:
		{
			int num37 = reader.ReadByte();
			int num38 = reader.ReadInt16();
			Main.npc[num37].GetHurtByDebuff(num38);
			if (Main.netMode == 2)
			{
				NetMessage.TrySendData(153, -1, whoAmI, null, num37, num38);
			}
			break;
		}
		case 154:
			if (Main.netMode == 2)
			{
				NetMessage.TrySendData(154, whoAmI);
			}
			else
			{
				Ping.PingRecieved();
			}
			break;
		case 155:
		{
			short num32 = reader.ReadInt16();
			short newSize = reader.ReadInt16();
			if (num32 >= 0 && num32 < 8000)
			{
				Main.chest[num32].Resize(newSize);
			}
			break;
		}
		case 156:
			if (Main.netMode == 2)
			{
				Point16 point = new Point16(reader.ReadInt16(), reader.ReadInt16());
				int itemType = reader.ReadInt16();
				if (TileEntity.TryGetAt<TELeashedEntityAnchorWithItem>(point.X, point.Y, out var result))
				{
					result.InsertItem(itemType);
				}
			}
			break;
		case 158:
			if (Main.netMode != 2)
			{
				byte b4 = reader.ReadByte();
				Main.player[b4].Spawn(PlayerSpawnContext.TeamSwap);
			}
			break;
		case 159:
			if (Main.netMode == 2)
			{
				int sectionX = reader.ReadUInt16();
				int sectionY = reader.ReadUInt16();
				NetMessage.SendSection(whoAmI, sectionX, sectionY);
			}
			break;
		case 160:
			if (Main.netMode != 2)
			{
				int num13 = reader.ReadInt16();
				Vector2 position = reader.ReadVector2();
				Main.item[num13].position = position;
			}
			break;
		case 161:
		{
			string text = reader.ReadString();
			Main.player[whoAmI].host = !string.IsNullOrWhiteSpace(Netplay.HostToken) && Netplay.HostToken == text;
			break;
		}
		default:
			if (Main.netMode == 2 && Netplay.Clients[whoAmI].State == 0)
			{
				NetMessage.BootPlayer(whoAmI, Lang.mp[2].ToNetworkText());
			}
			break;
		case 15:
		case 25:
		case 26:
		case 44:
		case 67:
		case 83:
		case 93:
			break;
		}
	}

	private static void ReadAccessoryVisibility(BinaryReader reader, bool[] hideVisibleAccessory)
	{
		ushort num = reader.ReadUInt16();
		for (int i = 0; i < hideVisibleAccessory.Length; i++)
		{
			hideVisibleAccessory[i] = (num & (1 << i)) != 0;
		}
	}

	private static void TrySendingItemArray(int plr, Item[] array, int slotStartIndex)
	{
		for (int i = 0; i < array.Length; i++)
		{
			NetMessage.TrySendData(5, -1, -1, null, plr, slotStartIndex + i);
		}
	}
}
