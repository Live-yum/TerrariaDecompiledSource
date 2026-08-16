using System;
using System.IO;
using Ionic.Zlib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Events;
using Terraria.GameContent.Items;
using Terraria.GameContent.Tile_Entities;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Map;
using Terraria.Social;
using Terraria.Testing;

namespace Terraria;

public class NetMessage
{
	public struct NetSoundInfo
	{
		public Vector2 position;

		public ushort soundIndex;

		public int style;

		public float volume;

		public float pitchOffset;

		public NetSoundInfo(Vector2 position, ushort soundIndex, int style = -1, float volume = -1f, float pitchOffset = -1f)
		{
			this.position = position;
			this.soundIndex = soundIndex;
			this.style = style;
			this.volume = volume;
			this.pitchOffset = pitchOffset;
		}

		public void WriteSelfTo(BinaryWriter writer)
		{
			writer.WriteVector2(position);
			writer.Write(soundIndex);
			BitsByte bitsByte = new BitsByte(style != -1, volume != -1f, pitchOffset != -1f);
			writer.Write(bitsByte);
			if (bitsByte[0])
			{
				writer.Write(style);
			}
			if (bitsByte[1])
			{
				writer.Write(volume);
			}
			if (bitsByte[2])
			{
				writer.Write(pitchOffset);
			}
		}
	}

	public static MessageBuffer[] buffer = new MessageBuffer[257];

	private static short[] _compressChestList = new short[8000];

	private static short[] _compressSignList = new short[32000];

	private static short[] _compressEntities = new short[1000];

	private static PlayerDeathReason _currentPlayerDeathReason;

	private static NetSoundInfo _currentNetSoundInfo;

	private static CoinLossRevengeSystem.RevengeMarker _currentRevengeMarker;

	public static bool TrySendData(int msgType, int remoteClient = -1, int ignoreClient = -1, NetworkText text = null, int number = 0, float number2 = 0f, float number3 = 0f, float number4 = 0f, int number5 = 0, int number6 = 0, int number7 = 0)
	{
		try
		{
			SendData(msgType, remoteClient, ignoreClient, text, number, number2, number3, number4, number5, number6, number7);
		}
		catch (Exception)
		{
			return false;
		}
		return true;
	}

	public static void SendData(int msgType, int remoteClient = -1, int ignoreClient = -1, NetworkText text = null, int number = 0, float number2 = 0f, float number3 = 0f, float number4 = 0f, int number5 = 0, int number6 = 0, int number7 = 0)
	{
		if (Main.netMode == 0)
		{
			return;
		}
		if (msgType == 21 && (Main.item[number].shimmerTime > 0f || Main.item[number].shimmered))
		{
			msgType = 145;
		}
		if (msgType == 21 && Main.item[number].type == 0)
		{
			msgType = 151;
		}
		int num = 256;
		if (text == null)
		{
			text = NetworkText.Empty;
		}
		if (Main.netMode == 2 && remoteClient >= 0)
		{
			num = remoteClient;
		}
		lock (buffer[num])
		{
			BinaryWriter writer = buffer[num].writer;
			if (writer == null)
			{
				buffer[num].ResetWriter();
				writer = buffer[num].writer;
			}
			writer.BaseStream.Position = 0L;
			long position = writer.BaseStream.Position;
			writer.BaseStream.Position += 2L;
			writer.Write((byte)msgType);
			switch (msgType)
			{
			case 1:
				writer.Write("Terraria" + 318);
				break;
			case 2:
				text.Serialize(writer);
				if (Main.dedServ)
				{
					Console.WriteLine(Language.GetTextValue("CLI.ClientWasBooted", Netplay.Clients[num].Socket.GetRemoteAddress().ToString(), text));
				}
				break;
			case 3:
				writer.Write((byte)remoteClient);
				writer.Write(value: false);
				break;
			case 4:
			{
				Player player5 = Main.player[number];
				writer.Write((byte)number);
				writer.Write((byte)player5.skinVariant);
				writer.Write((byte)player5.voiceVariant);
				writer.Write(player5.voicePitchOffset);
				writer.Write((byte)player5.hair);
				writer.Write(player5.name);
				writer.Write(player5.hairDye);
				WriteAccessoryVisibility(writer, player5.hideVisibleAccessory);
				writer.Write(player5.hideMisc);
				writer.WriteRGB(player5.hairColor);
				writer.WriteRGB(player5.skinColor);
				writer.WriteRGB(player5.eyeColor);
				writer.WriteRGB(player5.shirtColor);
				writer.WriteRGB(player5.underShirtColor);
				writer.WriteRGB(player5.pantsColor);
				writer.WriteRGB(player5.shoeColor);
				BitsByte bitsByte20 = (byte)0;
				if (player5.difficulty == 1)
				{
					bitsByte20[0] = true;
				}
				else if (player5.difficulty == 2)
				{
					bitsByte20[1] = true;
				}
				else if (player5.difficulty == 3)
				{
					bitsByte20[3] = true;
				}
				bitsByte20[2] = player5.extraAccessory;
				writer.Write(bitsByte20);
				BitsByte bitsByte21 = (byte)0;
				bitsByte21[0] = player5.UsingBiomeTorches;
				bitsByte21[1] = player5.happyFunTorchTime;
				bitsByte21[2] = player5.unlockedBiomeTorches;
				bitsByte21[3] = player5.unlockedSuperCart;
				bitsByte21[4] = player5.enabledSuperCart;
				writer.Write(bitsByte21);
				BitsByte bitsByte22 = (byte)0;
				bitsByte22[0] = player5.usedAegisCrystal;
				bitsByte22[1] = player5.usedAegisFruit;
				bitsByte22[2] = player5.usedArcaneCrystal;
				bitsByte22[3] = player5.usedGalaxyPearl;
				bitsByte22[4] = player5.usedGummyWorm;
				bitsByte22[5] = player5.usedAmbrosia;
				bitsByte22[6] = player5.ateArtisanBread;
				writer.Write(bitsByte22);
				break;
			}
			case 5:
			{
				writer.Write((byte)number);
				writer.Write((short)number2);
				Item item5 = new PlayerItemSlotID.SlotReference(Main.player[number], (int)number2).Item;
				if (item5.Name == "" || item5.stack == 0 || item5.type == 0)
				{
					item5.SetDefaults(0);
				}
				int num7 = item5.stack;
				int type = item5.type;
				if (num7 < 0)
				{
					num7 = 0;
				}
				writer.Write((short)num7);
				writer.Write(item5.prefix);
				writer.Write((short)type);
				writer.Write(new BitsByte
				{
					[0] = item5.favorited,
					[1] = number3 != 0f
				});
				break;
			}
			case 7:
			{
				writer.Write((int)Main.time);
				BitsByte bitsByte5 = (byte)0;
				bitsByte5[0] = Main.dayTime;
				bitsByte5[1] = Main.bloodMoon;
				bitsByte5[2] = Main.eclipse;
				writer.Write(bitsByte5);
				writer.Write((byte)Main.moonPhase);
				writer.Write((short)Main.maxTilesX);
				writer.Write((short)Main.maxTilesY);
				writer.Write((short)Main.spawnTileX);
				writer.Write((short)Main.spawnTileY);
				writer.Write((short)Main.worldSurface);
				writer.Write((short)Main.rockLayer);
				writer.Write(Main.ActiveWorldFileData.WorldId);
				writer.Write(Main.worldName);
				writer.Write((byte)Main.GameMode);
				writer.Write(Main.ActiveWorldFileData.UniqueId.ToByteArray());
				writer.Write(Main.ActiveWorldFileData.WorldGeneratorVersion);
				writer.Write((byte)Main.moonType);
				writer.Write((byte)WorldGen.treeBG1);
				writer.Write((byte)WorldGen.treeBG2);
				writer.Write((byte)WorldGen.treeBG3);
				writer.Write((byte)WorldGen.treeBG4);
				writer.Write((byte)WorldGen.corruptBG);
				writer.Write((byte)WorldGen.jungleBG);
				writer.Write((byte)WorldGen.snowBG);
				writer.Write((byte)WorldGen.hallowBG);
				writer.Write((byte)WorldGen.crimsonBG);
				writer.Write((byte)WorldGen.desertBG);
				writer.Write((byte)WorldGen.oceanBG);
				writer.Write((byte)WorldGen.mushroomBG);
				writer.Write((byte)WorldGen.underworldBG);
				writer.Write((byte)Main.iceBackStyle);
				writer.Write((byte)Main.jungleBackStyle);
				writer.Write((byte)Main.hellBackStyle);
				writer.Write(Main.windSpeedTarget);
				writer.Write((byte)Main.numClouds);
				for (int n = 0; n < 3; n++)
				{
					writer.Write(Main.treeX[n]);
				}
				for (int num11 = 0; num11 < 4; num11++)
				{
					writer.Write((byte)Main.treeStyle[num11]);
				}
				for (int num12 = 0; num12 < 3; num12++)
				{
					writer.Write(Main.caveBackX[num12]);
				}
				for (int num13 = 0; num13 < 4; num13++)
				{
					writer.Write((byte)Main.caveBackStyle[num13]);
				}
				WorldGen.TreeTops.SyncSend(writer);
				if (!Main.raining)
				{
					Main.maxRaining = 0f;
				}
				writer.Write(Main.maxRaining);
				BitsByte bitsByte6 = (byte)0;
				bitsByte6[0] = WorldGen.shadowOrbSmashed;
				bitsByte6[1] = NPC.downedBoss1;
				bitsByte6[2] = NPC.downedBoss2;
				bitsByte6[3] = NPC.downedBoss3;
				bitsByte6[4] = Main.hardMode;
				bitsByte6[5] = NPC.downedClown;
				bitsByte6[7] = NPC.downedPlantBoss;
				writer.Write(bitsByte6);
				BitsByte bitsByte7 = (byte)0;
				bitsByte7[0] = NPC.downedMechBoss1;
				bitsByte7[1] = NPC.downedMechBoss2;
				bitsByte7[2] = NPC.downedMechBoss3;
				bitsByte7[3] = NPC.downedMechBossAny;
				bitsByte7[4] = Main.cloudBGActive >= 1f;
				bitsByte7[5] = WorldGen.crimson;
				bitsByte7[6] = Main.pumpkinMoon;
				bitsByte7[7] = Main.snowMoon;
				writer.Write(bitsByte7);
				BitsByte bitsByte8 = (byte)0;
				bitsByte8[1] = Main.fastForwardTimeToDawn;
				bitsByte8[2] = Main.slimeRain;
				bitsByte8[3] = NPC.downedSlimeKing;
				bitsByte8[4] = NPC.downedQueenBee;
				bitsByte8[5] = NPC.downedFishron;
				bitsByte8[6] = NPC.downedMartians;
				bitsByte8[7] = NPC.downedAncientCultist;
				writer.Write(bitsByte8);
				BitsByte bitsByte9 = (byte)0;
				bitsByte9[0] = NPC.downedMoonlord;
				bitsByte9[1] = NPC.downedHalloweenKing;
				bitsByte9[2] = NPC.downedHalloweenTree;
				bitsByte9[3] = NPC.downedChristmasIceQueen;
				bitsByte9[4] = NPC.downedChristmasSantank;
				bitsByte9[5] = NPC.downedChristmasTree;
				bitsByte9[6] = NPC.downedGolemBoss;
				bitsByte9[7] = BirthdayParty.PartyIsUp;
				writer.Write(bitsByte9);
				BitsByte bitsByte10 = (byte)0;
				bitsByte10[0] = NPC.downedPirates;
				bitsByte10[1] = NPC.downedFrost;
				bitsByte10[2] = NPC.downedGoblins;
				bitsByte10[3] = Sandstorm.Happening;
				bitsByte10[4] = DD2Event.Ongoing;
				bitsByte10[5] = DD2Event.DownedInvasionT1;
				bitsByte10[6] = DD2Event.DownedInvasionT2;
				bitsByte10[7] = DD2Event.DownedInvasionT3;
				writer.Write(bitsByte10);
				BitsByte bitsByte11 = (byte)0;
				bitsByte11[0] = NPC.combatBookWasUsed;
				bitsByte11[1] = LanternNight.LanternsUp;
				bitsByte11[2] = NPC.downedTowerSolar;
				bitsByte11[3] = NPC.downedTowerVortex;
				bitsByte11[4] = NPC.downedTowerNebula;
				bitsByte11[5] = NPC.downedTowerStardust;
				bitsByte11[6] = Main.forceHalloweenForToday;
				bitsByte11[7] = Main.forceXMasForToday;
				writer.Write(bitsByte11);
				BitsByte bitsByte12 = (byte)0;
				bitsByte12[0] = NPC.boughtCat;
				bitsByte12[1] = NPC.boughtDog;
				bitsByte12[2] = NPC.boughtBunny;
				bitsByte12[3] = NPC.freeCake;
				bitsByte12[4] = Main.drunkWorld;
				bitsByte12[5] = NPC.downedEmpressOfLight;
				bitsByte12[6] = NPC.downedQueenSlime;
				bitsByte12[7] = Main.getGoodWorld;
				writer.Write(bitsByte12);
				BitsByte bitsByte13 = (byte)0;
				bitsByte13[0] = Main.tenthAnniversaryWorld;
				bitsByte13[1] = Main.dontStarveWorld;
				bitsByte13[2] = NPC.downedDeerclops;
				bitsByte13[3] = Main.notTheBeesWorld;
				bitsByte13[4] = Main.remixWorld;
				bitsByte13[5] = NPC.unlockedSlimeBlueSpawn;
				bitsByte13[6] = NPC.combatBookVolumeTwoWasUsed;
				bitsByte13[7] = NPC.peddlersSatchelWasUsed;
				writer.Write(bitsByte13);
				BitsByte bitsByte14 = (byte)0;
				bitsByte14[0] = NPC.unlockedSlimeGreenSpawn;
				bitsByte14[1] = NPC.unlockedSlimeOldSpawn;
				bitsByte14[2] = NPC.unlockedSlimePurpleSpawn;
				bitsByte14[3] = NPC.unlockedSlimeRainbowSpawn;
				bitsByte14[4] = NPC.unlockedSlimeRedSpawn;
				bitsByte14[5] = NPC.unlockedSlimeYellowSpawn;
				bitsByte14[6] = NPC.unlockedSlimeCopperSpawn;
				bitsByte14[7] = Main.fastForwardTimeToDusk;
				writer.Write(bitsByte14);
				BitsByte bitsByte15 = (byte)0;
				bitsByte15[0] = Main.noTrapsWorld;
				bitsByte15[1] = Main.zenithWorld;
				bitsByte15[2] = NPC.unlockedTruffleSpawn;
				bitsByte15[3] = Main.vampireSeed;
				bitsByte15[4] = Main.infectedSeed;
				bitsByte15[5] = Main.teamBasedSpawnsSeed;
				bitsByte15[6] = Main.skyblockWorld;
				bitsByte15[7] = Main.dualDungeonsSeed;
				writer.Write(bitsByte15);
				BitsByte bitsByte16 = (byte)0;
				bitsByte16[0] = WorldGen.Skyblock.lowTiles;
				writer.Write(bitsByte16);
				writer.Write((byte)Main.sundialCooldown);
				writer.Write((byte)Main.moondialCooldown);
				writer.Write((short)WorldGen.SavedOreTiers.Copper);
				writer.Write((short)WorldGen.SavedOreTiers.Iron);
				writer.Write((short)WorldGen.SavedOreTiers.Silver);
				writer.Write((short)WorldGen.SavedOreTiers.Gold);
				writer.Write((short)WorldGen.SavedOreTiers.Cobalt);
				writer.Write((short)WorldGen.SavedOreTiers.Mythril);
				writer.Write((short)WorldGen.SavedOreTiers.Adamantite);
				writer.Write((sbyte)Main.invasionType);
				if (SocialAPI.Network != null)
				{
					writer.Write(SocialAPI.Network.GetLobbyId());
				}
				else
				{
					writer.Write(0uL);
				}
				writer.Write(Sandstorm.IntendedSeverity);
				ExtraSpawnPointManager.Write(writer, networking: true);
				break;
			}
			case 8:
				writer.Write(number);
				writer.Write((int)number2);
				writer.Write((byte)number3);
				break;
			case 9:
			{
				writer.Write(number);
				text.Serialize(writer);
				BitsByte bitsByte30 = (byte)number2;
				writer.Write(bitsByte30);
				break;
			}
			case 10:
				CompressTileBlock(number, (int)number2, (short)number3, (short)number4, writer.BaseStream);
				break;
			case 11:
				writer.Write((short)number);
				writer.Write((short)number2);
				writer.Write((short)number3);
				writer.Write((short)number4);
				break;
			case 12:
			{
				Player player7 = Main.player[number];
				writer.Write((byte)number);
				writer.Write((short)player7.SpawnX);
				writer.Write((short)player7.SpawnY);
				writer.Write(player7.respawnTimer);
				writer.Write((short)player7.numberOfDeathsPVE);
				writer.Write((short)player7.numberOfDeathsPVP);
				writer.Write((byte)player7.team);
				writer.Write((byte)number2);
				break;
			}
			case 13:
			{
				Player player6 = Main.player[number];
				writer.Write((byte)number);
				BitsByte bitsByte23 = (byte)0;
				bitsByte23[0] = player6.controlUp;
				bitsByte23[1] = player6.controlDown;
				bitsByte23[2] = player6.controlLeft;
				bitsByte23[3] = player6.controlRight;
				bitsByte23[4] = player6.controlJump;
				bitsByte23[5] = player6.controlUseItem;
				bitsByte23[6] = player6.direction == 1;
				writer.Write(bitsByte23);
				BitsByte bitsByte24 = (byte)0;
				bitsByte24[0] = player6.pulley;
				bitsByte24[1] = player6.pulley && player6.pulleyDir == 2;
				bitsByte24[2] = player6.velocity != Vector2.Zero;
				bitsByte24[3] = player6.vortexStealthActive;
				bitsByte24[4] = player6.gravDir == 1f;
				bitsByte24[5] = player6.shieldRaised;
				bitsByte24[6] = player6.ghost;
				bitsByte24[7] = player6.mount.Active;
				writer.Write(bitsByte24);
				BitsByte bitsByte25 = (byte)0;
				bitsByte25[0] = player6.tryKeepingHoveringUp;
				bitsByte25[1] = player6.IsVoidVaultEnabled;
				bitsByte25[2] = player6.sitting.isSitting;
				bitsByte25[3] = player6.downedDD2EventAnyDifficulty;
				bitsByte25[4] = player6.petting.isPetting;
				bitsByte25[5] = player6.petting.isPetSmall;
				bitsByte25[6] = player6.PotionOfReturnOriginalUsePosition.HasValue;
				bitsByte25[7] = player6.tryKeepingHoveringDown;
				writer.Write(bitsByte25);
				BitsByte bitsByte26 = (byte)0;
				bitsByte26[0] = player6.sleeping.isSleeping;
				bitsByte26[1] = player6.autoReuseAllWeapons;
				bitsByte26[2] = player6.controlDownHold;
				bitsByte26[3] = player6.isOperatingAnotherEntity;
				bitsByte26[4] = player6.controlUseTile;
				bitsByte26[5] = player6.netCameraTarget.HasValue;
				bitsByte26[6] = player6.lastItemUseAttemptSuccess;
				writer.Write(bitsByte26);
				writer.Write((byte)player6.selectedItem);
				writer.WriteVector2(player6.position);
				if (bitsByte24[2])
				{
					writer.WriteVector2(player6.velocity);
				}
				if (bitsByte24[7])
				{
					writer.Write((ushort)player6.mount.Type);
				}
				if (bitsByte25[6])
				{
					writer.WriteVector2(player6.PotionOfReturnOriginalUsePosition.Value);
					writer.WriteVector2(player6.PotionOfReturnHomePosition.Value);
				}
				if (bitsByte26[5])
				{
					writer.WriteVector2(player6.netCameraTarget.Value);
				}
				if (player6 == Main.LocalPlayer)
				{
					player6.lastSyncedNetCameraTarget = player6.netCameraTarget;
				}
				break;
			}
			case 14:
				writer.Write((byte)number);
				writer.Write((byte)number2);
				break;
			case 16:
				writer.Write((byte)number);
				writer.Write((short)Main.player[number].statLife);
				writer.Write((short)Main.player[number].statLifeMax);
				break;
			case 17:
				writer.Write((byte)number);
				writer.Write((short)number2);
				writer.Write((short)number3);
				writer.Write((short)number4);
				writer.Write((byte)number5);
				break;
			case 18:
				writer.Write((byte)(Main.dayTime ? 1u : 0u));
				writer.Write((int)Main.time);
				writer.Write(Main.sunModY);
				writer.Write(Main.moonModY);
				break;
			case 19:
				writer.Write((byte)number);
				writer.Write((short)number2);
				writer.Write((short)number3);
				writer.Write((number4 == 1f) ? ((byte)1) : ((byte)0));
				break;
			case 20:
			{
				int num14 = number;
				int num15 = (int)number2;
				int num16 = (int)number3;
				if (num16 < 0)
				{
					num16 = 0;
				}
				int num17 = (int)number4;
				if (num17 < 0)
				{
					num17 = 0;
				}
				if (num14 < num16)
				{
					num14 = num16;
				}
				if (num14 >= Main.maxTilesX + num16)
				{
					num14 = Main.maxTilesX - num16 - 1;
				}
				if (num15 < num17)
				{
					num15 = num17;
				}
				if (num15 >= Main.maxTilesY + num17)
				{
					num15 = Main.maxTilesY - num17 - 1;
				}
				writer.Write((short)num14);
				writer.Write((short)num15);
				writer.Write((byte)num16);
				writer.Write((byte)num17);
				writer.Write((byte)number5);
				for (int num18 = num14; num18 < num14 + num16; num18++)
				{
					for (int num19 = num15; num19 < num15 + num17; num19++)
					{
						BitsByte bitsByte17 = (byte)0;
						BitsByte bitsByte18 = (byte)0;
						BitsByte bitsByte19 = (byte)0;
						byte b3 = 0;
						byte b4 = 0;
						Tile tile2 = Main.tile[num18, num19];
						bitsByte17[0] = tile2.active();
						bitsByte17[2] = tile2.wall > 0;
						bitsByte17[3] = tile2.liquid > 0 && Main.netMode == 2;
						bitsByte17[4] = tile2.wire();
						bitsByte17[5] = tile2.halfBrick();
						bitsByte17[6] = tile2.actuator();
						bitsByte17[7] = tile2.inActive();
						bitsByte18[0] = tile2.wire2();
						bitsByte18[1] = tile2.wire3();
						if (tile2.active() && tile2.color() > 0)
						{
							bitsByte18[2] = true;
							b3 = tile2.color();
						}
						if (tile2.wall > 0 && tile2.wallColor() > 0)
						{
							bitsByte18[3] = true;
							b4 = tile2.wallColor();
						}
						bitsByte18 = (byte)((byte)bitsByte18 + (byte)(tile2.slope() << 4));
						bitsByte18[7] = tile2.wire4();
						bitsByte19[0] = tile2.fullbrightBlock();
						bitsByte19[1] = tile2.fullbrightWall();
						bitsByte19[2] = tile2.invisibleBlock();
						bitsByte19[3] = tile2.invisibleWall();
						writer.Write(bitsByte17);
						writer.Write(bitsByte18);
						writer.Write(bitsByte19);
						if (b3 > 0)
						{
							writer.Write(b3);
						}
						if (b4 > 0)
						{
							writer.Write(b4);
						}
						if (tile2.active())
						{
							writer.Write(tile2.type);
							if (Main.tileFrameImportant[tile2.type])
							{
								writer.Write(tile2.frameX);
								writer.Write(tile2.frameY);
							}
						}
						if (tile2.wall > 0)
						{
							writer.Write(tile2.wall);
						}
						if (tile2.liquid > 0 && Main.netMode == 2)
						{
							writer.Write(tile2.liquid);
							writer.Write(tile2.liquidType());
						}
					}
				}
				break;
			}
			case 21:
			case 90:
			case 145:
			case 148:
			{
				WorldItem worldItem3 = Main.item[number];
				Item inner = worldItem3.inner;
				writer.Write((short)number);
				writer.WriteVector2(worldItem3.position);
				writer.WriteVector2(worldItem3.velocity);
				writer.Write((short)inner.stack);
				writer.Write(inner.prefix);
				writer.Write((byte)number2);
				short value3 = 0;
				if (worldItem3.active && worldItem3.stack > 0)
				{
					value3 = (short)worldItem3.type;
				}
				writer.Write(value3);
				if (msgType == 145)
				{
					writer.Write(worldItem3.shimmered);
					writer.Write(worldItem3.shimmerTime);
				}
				if (msgType == 148)
				{
					writer.Write((byte)MathHelper.Clamp(worldItem3.timeLeftInWhichTheItemCannotBeTakenByEnemies, 0f, 255f));
				}
				break;
			}
			case 151:
				Main.item[number].playerIndexTheItemIsReservedFor = 255;
				writer.Write((short)number);
				break;
			case 22:
			{
				WorldItem worldItem2 = Main.item[number];
				writer.Write((short)number);
				writer.Write((byte)worldItem2.playerIndexTheItemIsReservedFor);
				writer.WriteVector2(worldItem2.position);
				break;
			}
			case 23:
			{
				NPC nPC2 = Main.npc[number];
				writer.Write((short)number);
				writer.WriteVector2(nPC2.position);
				writer.WriteVector2(nPC2.velocity);
				writer.Write((ushort)nPC2.target);
				int num4 = nPC2.life;
				if (!nPC2.active)
				{
					num4 = 0;
				}
				short value2 = (short)nPC2.netID;
				bool[] array = new bool[4];
				BitsByte bitsByte = (byte)0;
				bitsByte[0] = nPC2.direction > 0;
				bitsByte[1] = nPC2.directionY > 0;
				bitsByte[2] = (array[0] = nPC2.ai[0] != 0f);
				bitsByte[3] = (array[1] = nPC2.ai[1] != 0f);
				bitsByte[4] = (array[2] = nPC2.ai[2] != 0f);
				bitsByte[5] = (array[3] = nPC2.ai[3] != 0f);
				bitsByte[6] = nPC2.spriteDirection > 0;
				bitsByte[7] = num4 == nPC2.lifeMax;
				writer.Write(bitsByte);
				BitsByte bitsByte2 = (byte)0;
				bitsByte2[0] = nPC2.statsAreScaledForThisManyPlayers > 1;
				bitsByte2[1] = nPC2.SpawnedFromStatue;
				bitsByte2[2] = nPC2.difficulty != 1f;
				bitsByte2[3] = nPC2.spawnNeedsSyncing;
				bitsByte2[4] = nPC2.spawnNeedsSyncing && nPC2.shimmerTransparency > 0f;
				writer.Write(bitsByte2);
				for (int l = 0; l < NPC.maxAI; l++)
				{
					if (array[l])
					{
						writer.Write(nPC2.ai[l]);
					}
				}
				writer.Write(value2);
				if (bitsByte2[0])
				{
					writer.Write((byte)nPC2.statsAreScaledForThisManyPlayers);
				}
				if (bitsByte2[2])
				{
					writer.Write(nPC2.difficulty);
				}
				if (!bitsByte[7])
				{
					byte b2 = 1;
					if (nPC2.lifeMax > 32767)
					{
						b2 = 4;
					}
					else if (nPC2.lifeMax > 127)
					{
						b2 = 2;
					}
					writer.Write(b2);
					switch (b2)
					{
					case 2:
						writer.Write((short)num4);
						break;
					case 4:
						writer.Write(num4);
						break;
					default:
						writer.Write((sbyte)num4);
						break;
					}
				}
				if (nPC2.type >= 0 && nPC2.type < NPCID.Count && Main.npcCatchable[nPC2.type])
				{
					writer.Write((byte)nPC2.releaseOwner);
				}
				break;
			}
			case 24:
				writer.Write((short)number);
				writer.Write((byte)number2);
				break;
			case 107:
				writer.Write((byte)number2);
				writer.Write((byte)number3);
				writer.Write((byte)number4);
				text.Serialize(writer);
				writer.Write((short)number5);
				break;
			case 27:
			{
				Projectile projectile = Main.projectile[number];
				writer.Write((short)projectile.identity);
				writer.WriteVector2(projectile.position);
				writer.WriteVector2(projectile.velocity);
				writer.Write((byte)projectile.owner);
				writer.Write((short)projectile.type);
				BitsByte bitsByte27 = (byte)0;
				BitsByte bitsByte28 = (byte)0;
				bitsByte27[0] = projectile.ai[0] != 0f;
				bitsByte27[1] = projectile.ai[1] != 0f;
				bitsByte28[0] = projectile.ai[2] != 0f;
				if (projectile.bannerIdToRespondTo != 0)
				{
					bitsByte27[3] = true;
				}
				if (projectile.damage != 0)
				{
					bitsByte27[4] = true;
				}
				if (projectile.knockBack != 0f)
				{
					bitsByte27[5] = true;
				}
				if (projectile.type > 0 && projectile.type < ProjectileID.Count && ProjectileID.Sets.NeedsUUID[projectile.type])
				{
					bitsByte27[7] = true;
				}
				if (projectile.originalDamage != 0)
				{
					bitsByte27[6] = true;
				}
				if ((byte)bitsByte28 != 0)
				{
					bitsByte27[2] = true;
				}
				writer.Write(bitsByte27);
				if (bitsByte27[2])
				{
					writer.Write(bitsByte28);
				}
				if (bitsByte27[0])
				{
					writer.Write(projectile.ai[0]);
				}
				if (bitsByte27[1])
				{
					writer.Write(projectile.ai[1]);
				}
				if (bitsByte27[3])
				{
					writer.Write((ushort)projectile.bannerIdToRespondTo);
				}
				if (bitsByte27[4])
				{
					writer.Write((short)projectile.damage);
				}
				if (bitsByte27[5])
				{
					writer.Write(projectile.knockBack);
				}
				if (bitsByte27[6])
				{
					writer.Write((short)projectile.originalDamage);
				}
				if (bitsByte27[7])
				{
					writer.Write((short)projectile.projUUID);
				}
				if (bitsByte28[0])
				{
					writer.Write(projectile.ai[2]);
				}
				break;
			}
			case 28:
				writer.Write((short)number);
				writer.Write((short)number2);
				writer.Write(number3);
				writer.Write((byte)(number4 + 1f));
				writer.Write((byte)number5);
				break;
			case 29:
				writer.Write((short)number);
				writer.Write((byte)number2);
				break;
			case 30:
				writer.Write((byte)number);
				writer.Write(Main.player[number].hostile);
				break;
			case 31:
				writer.Write((short)number);
				writer.Write((short)number2);
				break;
			case 32:
			{
				Item item6 = Main.chest[number].item[(byte)number2];
				writer.Write((short)number);
				writer.Write((byte)number2);
				short value4 = (short)item6.type;
				if (item6.Name == null)
				{
					value4 = 0;
				}
				writer.Write((short)item6.stack);
				writer.Write(item6.prefix);
				writer.Write(value4);
				break;
			}
			case 33:
			{
				int num8 = 0;
				int num9 = 0;
				int num10 = 0;
				string text2 = null;
				if (number > -1)
				{
					num8 = Main.chest[number].x;
					num9 = Main.chest[number].y;
				}
				if (number2 == 1f)
				{
					string text3 = text.ToString();
					num10 = (byte)text3.Length;
					if (num10 == 0 || num10 > 20)
					{
						num10 = 255;
					}
					else
					{
						text2 = text3;
					}
				}
				writer.Write((short)number);
				writer.Write((short)num8);
				writer.Write((short)num9);
				writer.Write((byte)num10);
				if (text2 != null)
				{
					writer.Write(text2);
				}
				break;
			}
			case 34:
				writer.Write((byte)number);
				writer.Write((short)number2);
				writer.Write((short)number3);
				writer.Write((short)number4);
				if (Main.netMode == 2)
				{
					Netplay.GetSectionX((int)number2);
					Netplay.GetSectionY((int)number3);
					writer.Write((short)number5);
				}
				else
				{
					writer.Write((short)0);
				}
				break;
			case 35:
				writer.Write((byte)number);
				writer.Write((short)number2);
				break;
			case 36:
			{
				Player player4 = Main.player[number];
				writer.Write((byte)number);
				writer.Write(player4.zone1);
				writer.Write(player4.zone2);
				writer.Write(player4.zone3);
				writer.Write(player4.zone4);
				writer.Write(player4.zone5);
				writer.Write((byte)player4.townNPCs);
				break;
			}
			case 38:
				writer.Write(Netplay.ServerPassword);
				break;
			case 39:
				writer.Write((short)number);
				break;
			case 40:
				writer.Write((byte)number);
				writer.Write((short)Main.player[number].talkNPC);
				break;
			case 41:
				writer.Write((byte)number);
				writer.Write(Main.player[number].itemRotation);
				writer.Write((short)Main.player[number].itemAnimation);
				break;
			case 42:
				writer.Write((byte)number);
				writer.Write((short)Main.player[number].statMana);
				writer.Write((short)Main.player[number].statManaMax);
				break;
			case 43:
				writer.Write((byte)number);
				writer.Write((short)number2);
				break;
			case 45:
			case 157:
				writer.Write((byte)number);
				writer.Write((byte)Main.player[number].team);
				break;
			case 46:
				writer.Write((short)number);
				writer.Write((short)number2);
				break;
			case 47:
				writer.Write((short)number);
				writer.Write((short)Main.sign[number].x);
				writer.Write((short)Main.sign[number].y);
				writer.Write(Main.sign[number].text);
				writer.Write((byte)number2);
				writer.Write((byte)number3);
				break;
			case 48:
			{
				Tile tile = Main.tile[number, (int)number2];
				writer.Write((short)number);
				writer.Write((short)number2);
				writer.Write(tile.liquid);
				writer.Write(tile.liquidType());
				break;
			}
			case 50:
			{
				writer.Write((byte)number);
				Player player2 = Main.player[number];
				for (int m = 0; m < Player.maxBuffs; m++)
				{
					if (player2.buffType[m] > 0)
					{
						writer.Write((ushort)player2.buffType[m]);
					}
				}
				writer.Write((ushort)0);
				break;
			}
			case 51:
				writer.Write((byte)number);
				writer.Write((byte)number2);
				break;
			case 52:
				writer.Write((byte)number2);
				writer.Write((short)number3);
				writer.Write((short)number4);
				break;
			case 53:
				writer.Write((short)number);
				writer.Write((ushort)number2);
				writer.Write((short)number3);
				break;
			case 54:
			{
				NPC nPC = Main.npc[number];
				writer.Write((short)number);
				for (int k = 0; k < NPC.maxBuffs; k++)
				{
					int num2 = nPC.buffType[k];
					int num3 = nPC.buffTime[k];
					if (num2 > 0 && num3 > 0)
					{
						writer.Write((ushort)num2);
						writer.Write((ushort)num3);
					}
				}
				writer.Write((ushort)0);
				break;
			}
			case 55:
				writer.Write((byte)number);
				writer.Write((ushort)number2);
				writer.Write((int)number3);
				break;
			case 56:
				writer.Write((short)number);
				if (Main.netMode == 2)
				{
					string givenName = Main.npc[number].GivenName;
					writer.Write(givenName);
					writer.Write(Main.npc[number].townNpcVariationIndex);
				}
				break;
			case 57:
				writer.Write(WorldGen.tGood);
				writer.Write(WorldGen.tEvil);
				writer.Write(WorldGen.tBlood);
				break;
			case 58:
				writer.Write((byte)number);
				writer.Write(number2);
				break;
			case 59:
				writer.Write((short)number);
				writer.Write((short)number2);
				break;
			case 60:
				writer.Write((short)number);
				writer.Write((short)number2);
				writer.Write((short)number3);
				writer.Write((byte)number4);
				break;
			case 61:
				writer.Write((short)number);
				writer.Write((short)number2);
				break;
			case 62:
				writer.Write((byte)number);
				writer.Write((byte)number2);
				break;
			case 63:
			case 64:
				writer.Write((short)number);
				writer.Write((short)number2);
				writer.Write((byte)number3);
				writer.Write((byte)number4);
				break;
			case 65:
			{
				BitsByte bitsByte29 = (byte)0;
				bitsByte29[0] = (number & 1) == 1;
				bitsByte29[1] = (number & 2) == 2;
				bitsByte29[2] = number6 == 1;
				bitsByte29[3] = number7 != 0;
				writer.Write(bitsByte29);
				writer.Write((short)number2);
				writer.Write(number3);
				writer.Write(number4);
				writer.Write((byte)number5);
				if (bitsByte29[3])
				{
					writer.Write(number7);
				}
				if (Main.netMode == 2 && number == 0 && number2 != (float)ignoreClient)
				{
					Main.player[(int)number2].unacknowledgedTeleports++;
				}
				break;
			}
			case 66:
				writer.Write((byte)number);
				writer.Write((short)number2);
				break;
			case 68:
				writer.Write(Main.clientUUID);
				break;
			case 69:
				Netplay.GetSectionX((int)number2);
				Netplay.GetSectionY((int)number3);
				writer.Write((short)number);
				writer.Write((short)number2);
				writer.Write((short)number3);
				writer.Write(Main.chest[(short)number].name);
				break;
			case 70:
				writer.Write((short)number);
				writer.Write((byte)number2);
				break;
			case 71:
				writer.Write(number);
				writer.Write((int)number2);
				writer.Write((short)number3);
				writer.Write((byte)number4);
				break;
			case 72:
			{
				for (int num20 = 0; num20 < Main.TravelShopMaxSlots; num20++)
				{
					writer.Write((short)Main.travelShop[num20]);
				}
				break;
			}
			case 73:
				writer.Write((byte)number);
				break;
			case 74:
			{
				writer.Write((byte)Main.anglerQuest);
				bool value5 = Main.anglerWhoFinishedToday.Contains(text.ToString());
				writer.Write(value5);
				break;
			}
			case 76:
				writer.Write((byte)number);
				writer.Write(Main.player[number].anglerQuestsFinished);
				writer.Write(Main.player[number].golferScoreAccumulated);
				break;
			case 77:
				writer.Write((short)number);
				writer.Write((ushort)number2);
				writer.Write((short)number3);
				writer.Write((short)number4);
				break;
			case 78:
				writer.Write(number);
				writer.Write((int)number2);
				writer.Write((sbyte)number3);
				writer.Write((sbyte)number4);
				break;
			case 79:
				writer.Write((short)number);
				writer.Write((short)number2);
				writer.Write((short)number3);
				writer.Write((short)number4);
				writer.Write((byte)number5);
				writer.Write((sbyte)number6);
				writer.Write(number7 == 1);
				break;
			case 80:
				writer.Write((byte)number);
				writer.Write((short)number2);
				break;
			case 81:
				writer.Write(number2);
				writer.Write(number3);
				writer.WriteRGB(new Color
				{
					PackedValue = (uint)number
				});
				writer.Write((int)number4);
				break;
			case 119:
				writer.Write(number2);
				writer.Write(number3);
				writer.WriteRGB(new Color
				{
					PackedValue = (uint)number
				});
				text.Serialize(writer);
				break;
			case 84:
			{
				byte b5 = (byte)number;
				float stealth = Main.player[b5].stealth;
				writer.Write(b5);
				writer.Write(stealth);
				break;
			}
			case 85:
				if (Main.netMode == 1)
				{
					QuickStacking.WriteNetInventorySlots(writer);
					writer.Write((byte)number);
				}
				else
				{
					QuickStacking.WriteBlockedChestList(writer);
				}
				break;
			case 86:
			{
				writer.Write(number);
				TileEntity result3;
				bool flag2 = TileEntity.TryGet<TileEntity>(number, out result3);
				writer.Write(flag2);
				if (flag2)
				{
					TileEntity.Write(writer, result3, networkSend: true);
				}
				break;
			}
			case 87:
				writer.Write((short)number);
				writer.Write((short)number2);
				writer.Write((byte)number3);
				break;
			case 88:
			{
				BitsByte bitsByte3 = (byte)number2;
				BitsByte bitsByte4 = (byte)number3;
				writer.Write((short)number);
				writer.Write(bitsByte3);
				WorldItem worldItem4 = Main.item[number];
				if (bitsByte3[0])
				{
					writer.Write(worldItem4.color.PackedValue);
				}
				if (bitsByte3[1])
				{
					writer.Write((ushort)worldItem4.damage);
				}
				if (bitsByte3[2])
				{
					writer.Write(worldItem4.knockBack);
				}
				if (bitsByte3[3])
				{
					writer.Write((ushort)worldItem4.useAnimation);
				}
				if (bitsByte3[4])
				{
					writer.Write((ushort)worldItem4.useTime);
				}
				if (bitsByte3[5])
				{
					writer.Write((short)worldItem4.shoot);
				}
				if (bitsByte3[6])
				{
					writer.Write(worldItem4.shootSpeed);
				}
				if (bitsByte3[7])
				{
					writer.Write(bitsByte4);
					if (bitsByte4[0])
					{
						writer.Write((ushort)worldItem4.width);
					}
					if (bitsByte4[1])
					{
						writer.Write((ushort)worldItem4.height);
					}
					if (bitsByte4[2])
					{
						writer.Write(worldItem4.scale);
					}
					if (bitsByte4[3])
					{
						writer.Write((short)worldItem4.ammo);
					}
					if (bitsByte4[4])
					{
						writer.Write((short)worldItem4.useAmmo);
					}
					if (bitsByte4[5])
					{
						writer.Write(worldItem4.notAmmo);
					}
				}
				break;
			}
			case 89:
			{
				writer.Write((short)number);
				writer.Write((short)number2);
				Item item4 = Main.player[(int)number4].inventory[(int)number3];
				writer.Write((short)item4.type);
				writer.Write(item4.prefix);
				writer.Write((short)number5);
				break;
			}
			case 91:
				writer.Write(number);
				writer.Write((byte)number2);
				if (number2 != 255f)
				{
					writer.Write((ushort)number3);
					writer.Write((ushort)number4);
					writer.Write((byte)number5);
					if (number5 < 0)
					{
						writer.Write((short)number6);
					}
				}
				break;
			case 92:
				writer.Write((short)number);
				writer.Write((int)number2);
				writer.Write(number3);
				writer.Write(number4);
				break;
			case 95:
				writer.Write((ushort)number);
				writer.Write((byte)number2);
				break;
			case 96:
			{
				writer.Write((byte)number);
				Player player3 = Main.player[number];
				writer.Write((short)number4);
				writer.Write(number2);
				writer.Write(number3);
				writer.WriteVector2(player3.velocity);
				break;
			}
			case 97:
				writer.Write((short)number);
				break;
			case 98:
				writer.Write((short)number);
				break;
			case 99:
				writer.Write((byte)number);
				writer.WriteVector2(Main.player[number].MinionRestTargetPoint);
				break;
			case 115:
				writer.Write((byte)number);
				writer.Write((short)Main.player[number].MinionAttackTargetNPC);
				break;
			case 100:
			{
				writer.Write((ushort)number);
				NPC nPC3 = Main.npc[number];
				writer.Write((short)number4);
				writer.Write(number2);
				writer.Write(number3);
				writer.WriteVector2(nPC3.velocity);
				break;
			}
			case 101:
				writer.Write((ushort)NPC.ShieldStrengthTowerSolar);
				writer.Write((ushort)NPC.ShieldStrengthTowerVortex);
				writer.Write((ushort)NPC.ShieldStrengthTowerNebula);
				writer.Write((ushort)NPC.ShieldStrengthTowerStardust);
				break;
			case 102:
				writer.Write((byte)number);
				writer.Write((ushort)number2);
				writer.Write(number3);
				writer.Write(number4);
				break;
			case 103:
				writer.Write(NPC.MaxMoonLordCountdown);
				writer.Write(NPC.MoonLordCountdown);
				break;
			case 104:
				writer.Write((byte)number);
				writer.Write((short)number2);
				writer.Write(((short)number3 < 0) ? 0f : number3);
				writer.Write((byte)number4);
				writer.Write(number5);
				writer.Write((byte)number6);
				break;
			case 105:
				writer.Write((short)number);
				writer.Write((short)number2);
				writer.Write(number3 == 1f);
				break;
			case 106:
				writer.Write(new HalfVector2(number, number2).PackedValue);
				break;
			case 108:
				writer.Write((short)number);
				writer.Write(number2);
				writer.Write((short)number3);
				writer.Write((short)number4);
				writer.Write((short)number5);
				writer.Write((short)number6);
				writer.Write((byte)number7);
				break;
			case 109:
				writer.Write((short)number);
				writer.Write((short)number2);
				writer.Write((short)number3);
				writer.Write((short)number4);
				writer.Write((byte)number5);
				break;
			case 110:
				writer.Write((short)number);
				writer.Write((short)number2);
				writer.Write((byte)number3);
				break;
			case 112:
				writer.Write((byte)number);
				writer.Write((int)number2);
				writer.Write((int)number3);
				writer.Write((byte)number4);
				writer.Write((short)number5);
				writer.Write((byte)number6);
				break;
			case 113:
				writer.Write((short)number);
				writer.Write((short)number2);
				break;
			case 116:
				writer.Write(number);
				break;
			case 117:
				writer.Write((byte)number);
				_currentPlayerDeathReason.WriteSelfTo(writer);
				writer.Write((short)number2);
				writer.Write((byte)(number3 + 1f));
				writer.Write((byte)number4);
				writer.Write((sbyte)number5);
				break;
			case 118:
				writer.Write((byte)number);
				_currentPlayerDeathReason.WriteSelfTo(writer);
				writer.Write((short)number2);
				writer.Write((byte)(number3 + 1f));
				writer.Write((byte)number4);
				break;
			case 120:
				writer.Write((byte)number);
				writer.Write((byte)number2);
				break;
			case 121:
			{
				int num6 = (int)number3;
				writer.Write((byte)number);
				writer.Write((int)number2);
				writer.Write((byte)num6);
				writer.Write((byte)number4);
				if (TileEntity.TryGet<TEDisplayDoll>((int)number2, out var result2))
				{
					result2.WriteData((int)number3, (int)number4, writer);
				}
				else
				{
					TEDisplayDoll.WriteDummySync((int)number3, (int)number4, writer);
				}
				break;
			}
			case 122:
				writer.Write(number);
				writer.Write((byte)number2);
				break;
			case 123:
			{
				writer.Write((short)number);
				writer.Write((short)number2);
				Item item3 = Main.player[(int)number4].inventory[(int)number3];
				writer.Write((short)item3.type);
				writer.Write(item3.prefix);
				writer.Write((short)number5);
				break;
			}
			case 124:
			{
				int num5 = (int)number3;
				bool flag = number4 == 1f;
				if (flag)
				{
					num5 += 2;
				}
				writer.Write((byte)number);
				writer.Write((int)number2);
				writer.Write((byte)num5);
				if (TileEntity.TryGet<TEHatRack>((int)number2, out var result))
				{
					result.WriteItem((int)number3, writer, flag);
					break;
				}
				writer.Write(0);
				writer.Write((byte)0);
				break;
			}
			case 125:
				writer.Write((byte)number);
				writer.Write((short)number2);
				writer.Write((short)number3);
				writer.Write((byte)number4);
				break;
			case 126:
				_currentRevengeMarker.WriteSelfTo(writer);
				break;
			case 127:
				writer.Write(number);
				break;
			case 128:
				writer.Write((byte)number);
				writer.Write((ushort)number5);
				writer.Write((ushort)number6);
				writer.Write((ushort)number2);
				writer.Write((ushort)number3);
				break;
			case 130:
				writer.Write((ushort)number);
				writer.Write((ushort)number2);
				writer.Write((short)number3);
				break;
			case 131:
			{
				writer.Write((ushort)number);
				writer.Write((byte)number2);
				byte b = (byte)number2;
				if (b == 1)
				{
					writer.Write((int)number3);
					writer.Write((short)number4);
				}
				break;
			}
			case 132:
				_currentNetSoundInfo.WriteSelfTo(writer);
				break;
			case 133:
			{
				writer.Write((short)number);
				writer.Write((short)number2);
				Item item2 = Main.player[(int)number4].inventory[(int)number3];
				writer.Write((short)item2.type);
				writer.Write(item2.prefix);
				writer.Write((short)number5);
				break;
			}
			case 134:
			{
				writer.Write((byte)number);
				Player player = Main.player[number];
				writer.Write(player.ladyBugLuckTimeLeft);
				writer.Write(player.torchLuck);
				writer.Write(player.luckPotion);
				writer.Write(player.HasGardenGnomeNearby);
				writer.Write(player.brokenMirrorBadLuck);
				writer.Write(player.equipmentBasedLuckBonus);
				writer.Write(player.coinLuck);
				writer.Write(player.kiteLuckLevel);
				break;
			}
			case 135:
				writer.Write((byte)number);
				break;
			case 136:
			{
				for (int i = 0; i < 2; i++)
				{
					for (int j = 0; j < 3; j++)
					{
						writer.Write((ushort)NPC.cavernMonsterType[i, j]);
					}
				}
				break;
			}
			case 137:
				writer.Write((short)number);
				writer.Write((ushort)number2);
				break;
			case 139:
			{
				writer.Write((byte)number);
				bool value = number2 == 1f;
				writer.Write(value);
				break;
			}
			case 140:
				writer.Write((byte)number);
				writer.Write((int)number2);
				break;
			case 141:
				writer.Write((byte)number);
				writer.Write((byte)number2);
				writer.Write(number3);
				writer.Write(number4);
				writer.Write(number5);
				writer.Write(number6);
				break;
			case 142:
			{
				writer.Write((byte)number);
				Player obj = Main.player[number];
				obj.piggyBankProjTracker.Write(writer);
				obj.voidLensChest.Write(writer);
				break;
			}
			case 146:
				writer.Write((byte)number);
				switch (number)
				{
				case 0:
					writer.WriteVector2(new Vector2((int)number2, (int)number3));
					break;
				case 1:
					writer.WriteVector2(new Vector2((int)number2, (int)number3));
					writer.Write((int)number4);
					break;
				}
				break;
			case 147:
				writer.Write((byte)number);
				writer.Write((byte)number2);
				WriteAccessoryVisibility(writer, Main.player[number].hideVisibleAccessory);
				break;
			case 149:
			{
				writer.Write((short)number);
				writer.Write((short)number2);
				Item item = Main.player[(int)number4].inventory[(int)number3];
				writer.Write((short)item.type);
				writer.Write(item.prefix);
				writer.Write((short)number5);
				break;
			}
			case 150:
				writer.Write((byte)number);
				writer.Write((short)number2);
				break;
			case 152:
				writer.Write((byte)number);
				break;
			case 153:
				writer.Write((byte)number);
				writer.Write((short)number2);
				break;
			case 155:
				writer.Write((short)number);
				writer.Write((short)number2);
				break;
			case 156:
				writer.Write((short)number);
				writer.Write((short)number2);
				writer.Write((short)number3);
				break;
			case 158:
				writer.Write((byte)number);
				break;
			case 159:
				writer.Write((short)number);
				writer.Write((short)number2);
				break;
			case 160:
			{
				WorldItem worldItem = Main.item[number];
				writer.Write((short)number);
				writer.WriteVector2(worldItem.position);
				break;
			}
			case 161:
				writer.Write(text.ToString());
				break;
			}
			int num21 = (int)writer.BaseStream.Position;
			if (num21 > 65535)
			{
				throw new Exception("Maximum packet length exceeded. id: " + msgType + " length: " + num21);
			}
			writer.BaseStream.Position = position;
			writer.Write((ushort)num21);
			writer.BaseStream.Position = num21;
			if (Main.netMode == 1)
			{
				if (Netplay.Connection.IsConnected())
				{
					SendPacketToServer(buffer[num].writeBuffer);
				}
			}
			else if (remoteClient == -1)
			{
				switch (msgType)
				{
				case 34:
				case 69:
				{
					for (int num27 = 0; num27 < 256; num27++)
					{
						if (num27 != ignoreClient && buffer[num27].broadcast && Netplay.Clients[num27].IsConnected())
						{
							SendPacket(buffer[num].writeBuffer, num27);
						}
					}
					break;
				}
				case 20:
				{
					for (int num23 = 0; num23 < 256; num23++)
					{
						if (num23 != ignoreClient && buffer[num23].broadcast && Netplay.Clients[num23].IsConnected() && Netplay.Clients[num23].SectionRange((int)Math.Max(number3, number4), number, (int)number2))
						{
							SendPacket(buffer[num].writeBuffer, num23);
						}
					}
					break;
				}
				case 23:
				{
					NPC nPC4 = Main.npc[number];
					bool flag4 = nPC4.boss || nPC4.netAlways || nPC4.townNPC || !nPC4.active || nPC4.life <= 0 || nPC4.spawnNeedsSyncing;
					if (flag4)
					{
						nPC4.spawnNeedsSyncing = false;
						nPC4.netStream = 0;
						nPC4.netUpdate = false;
						nPC4.netUpdatePendingSpamCooldown = false;
						nPC4.netUpdatePendingFullSpamCooldown = false;
						Array.Clear(nPC4.playerNetSyncState, 0, nPC4.playerNetSyncState.Length);
					}
					for (int num25 = 0; num25 < 256; num25++)
					{
						if (num25 == ignoreClient || !buffer[num25].broadcast || !Netplay.Clients[num25].IsConnected())
						{
							continue;
						}
						if (!flag4)
						{
							if (nPC4.playerNetSyncState[num25].skippedSyncs < 4 && !Netplay.Clients[num25].IsSectionActive(nPC4.NetSectionCoordinates))
							{
								nPC4.playerNetSyncState[num25].skippedSyncs++;
								continue;
							}
							nPC4.playerNetSyncState[num25] = default(NPC.PlayerNetSyncState);
						}
						SendPacket(buffer[num].writeBuffer, num25);
					}
					break;
				}
				case 28:
				{
					NPC nPC5 = Main.npc[number];
					for (int num28 = 0; num28 < 256; num28++)
					{
						if (num28 != ignoreClient && buffer[num28].broadcast && Netplay.Clients[num28].IsConnected() && (nPC5.life <= 0 || Netplay.Clients[num28].IsSectionActive(nPC5.NetSectionCoordinates)))
						{
							SendPacket(buffer[num].writeBuffer, num28);
						}
					}
					break;
				}
				case 13:
				{
					for (int num26 = 0; num26 < 256; num26++)
					{
						if (num26 != ignoreClient && buffer[num26].broadcast && Netplay.Clients[num26].IsConnected())
						{
							SendPacket(buffer[num].writeBuffer, num26);
						}
					}
					break;
				}
				case 27:
				{
					Projectile projectile2 = Main.projectile[number];
					bool flag3 = projectile2.type == 12 || Main.projPet[projectile2.type] || projectile2.aiStyle == 11 || projectile2.netImportant;
					if (flag3)
					{
						Array.Clear(projectile2.netSyncSkippedForPlayer, 0, projectile2.netSyncSkippedForPlayer.Length);
					}
					for (int num24 = 0; num24 < 256; num24++)
					{
						if (num24 == ignoreClient || !buffer[num24].broadcast || !Netplay.Clients[num24].IsConnected())
						{
							continue;
						}
						if (!flag3)
						{
							if (!Netplay.Clients[num24].IsSectionActive(projectile2.NetSectionCoordinates))
							{
								projectile2.netSyncSkippedForPlayer[num24] = true;
								continue;
							}
							projectile2.netSyncSkippedForPlayer[num24] = false;
						}
						SendPacket(buffer[num].writeBuffer, num24);
					}
					break;
				}
				default:
				{
					for (int num22 = 0; num22 < 256; num22++)
					{
						if (num22 != ignoreClient && (buffer[num22].broadcast || (Netplay.Clients[num22].State >= 3 && msgType == 10)) && Netplay.Clients[num22].IsConnected())
						{
							SendPacket(buffer[num].writeBuffer, num22);
						}
					}
					break;
				}
				}
			}
			else if (Netplay.Clients[remoteClient].IsConnected())
			{
				switch (msgType)
				{
				case 23:
					Main.npc[number].playerNetSyncState[remoteClient] = default(NPC.PlayerNetSyncState);
					break;
				case 27:
					Main.projectile[number].netSyncSkippedForPlayer[remoteClient] = false;
					break;
				}
				SendPacket(buffer[num].writeBuffer, remoteClient);
			}
			if (Main.verboseNetplay)
			{
				for (int num29 = 0; num29 < num21; num29++)
				{
				}
				for (int num30 = 0; num30 < num21; num30++)
				{
					_ = buffer[num].writeBuffer[num30];
				}
			}
			buffer[num].writeLocked = false;
			if (msgType == 2 && Main.netMode == 2)
			{
				Netplay.Clients[num].PendingTermination = true;
			}
		}
	}

	private static void SendPacketToServer(byte[] data)
	{
		SendPacket(data, 256);
	}

	private static void SendPacket(byte[] data, int remoteClient)
	{
		try
		{
			ushort num = BitConverter.ToUInt16(data, 0);
			byte messageId = data[2];
			buffer[remoteClient].spamCount++;
			Main.ActiveNetDiagnosticsUI.CountSentMessage(messageId, num);
			if (!Main.dedServ)
			{
				Netplay.Connection.Socket.AsyncSend(data, 0, num, Netplay.Connection.ClientWriteCallBack);
			}
			else
			{
				Netplay.Clients[remoteClient].Socket.AsyncSend(data, 0, num, Netplay.Clients[remoteClient].ServerWriteCallBack);
			}
		}
		catch
		{
			_ = Main.dedServ;
		}
	}

	public static void SendChestContentsTo(int chest, int targetPlayer)
	{
		TrySendData(155, targetPlayer, -1, null, chest, Main.chest[chest].maxItems);
		for (int i = 0; i < Main.chest[chest].maxItems; i++)
		{
			TrySendData(32, targetPlayer, -1, null, chest, i);
		}
	}

	private static void WriteAccessoryVisibility(BinaryWriter writer, bool[] hideVisibleAccessory)
	{
		ushort num = 0;
		for (int i = 0; i < hideVisibleAccessory.Length; i++)
		{
			if (hideVisibleAccessory[i])
			{
				num |= (ushort)(1 << i);
			}
		}
		writer.Write(num);
	}

	public static void CompressTileBlock(int xStart, int yStart, short width, short height, Stream stream)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Expected O, but got Unknown
		DeflateStream val = new DeflateStream(stream, (CompressionMode)0, true);
		try
		{
			BinaryWriter binaryWriter = new BinaryWriter((Stream)(object)val);
			binaryWriter.Write(xStart);
			binaryWriter.Write(yStart);
			binaryWriter.Write(width);
			binaryWriter.Write(height);
			CompressTileBlock_Inner(binaryWriter, xStart, yStart, width, height);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static void CompressTileBlock_Inner(BinaryWriter writer, int xStart, int yStart, int width, int height)
	{
		short num = 0;
		short num2 = 0;
		short num3 = 0;
		short num4 = 0;
		int num5 = 0;
		int num6 = 0;
		byte b = 0;
		byte[] array = new byte[16];
		Tile tile = null;
		for (int i = yStart; i < yStart + height; i++)
		{
			for (int j = xStart; j < xStart + width; j++)
			{
				Tile tile2 = Main.tile[j, i];
				if (tile2.isTheSameAs(tile) && TileID.Sets.AllowsSaveCompressionBatching[tile2.type])
				{
					num4++;
					continue;
				}
				if (tile != null)
				{
					if (num4 > 0)
					{
						array[num5] = (byte)(num4 & 0xFF);
						num5++;
						if (num4 > 255)
						{
							b |= 0x80;
							array[num5] = (byte)((num4 & 0xFF00) >> 8);
							num5++;
						}
						else
						{
							b |= 0x40;
						}
					}
					array[num6] = b;
					writer.Write(array, num6, num5 - num6);
					num4 = 0;
				}
				num5 = 4;
				byte b3;
				byte b4;
				byte b2;
				b = (b2 = (b3 = (b4 = 0)));
				if (tile2.active())
				{
					b |= 2;
					array[num5] = (byte)tile2.type;
					num5++;
					if (tile2.type > 255)
					{
						array[num5] = (byte)(tile2.type >> 8);
						num5++;
						b |= 0x20;
					}
					if (TileID.Sets.BasicChest[tile2.type] && tile2.frameX % 36 == 0 && tile2.frameY % 36 == 0)
					{
						short num7 = (short)Chest.FindChest(j, i);
						if (num7 != -1)
						{
							_compressChestList[num] = num7;
							num++;
						}
					}
					if (tile2.type == 88 && tile2.frameX % 54 == 0 && tile2.frameY % 36 == 0)
					{
						short num8 = (short)Chest.FindChest(j, i);
						if (num8 != -1)
						{
							_compressChestList[num] = num8;
							num++;
						}
					}
					if (tile2.type == 85 && tile2.frameX % 36 == 0 && tile2.frameY % 36 == 0)
					{
						short num9 = (short)Sign.ReadSign(j, i);
						if (num9 != -1)
						{
							_compressSignList[num2++] = num9;
						}
					}
					if (tile2.type == 55 && tile2.frameX % 36 == 0 && tile2.frameY % 36 == 0)
					{
						short num10 = (short)Sign.ReadSign(j, i);
						if (num10 != -1)
						{
							_compressSignList[num2++] = num10;
						}
					}
					if (tile2.type == 425 && tile2.frameX % 36 == 0 && tile2.frameY % 36 == 0)
					{
						short num11 = (short)Sign.ReadSign(j, i);
						if (num11 != -1)
						{
							_compressSignList[num2++] = num11;
						}
					}
					if (tile2.type == 573 && tile2.frameX % 36 == 0 && tile2.frameY % 36 == 0)
					{
						short num12 = (short)Sign.ReadSign(j, i);
						if (num12 != -1)
						{
							_compressSignList[num2++] = num12;
						}
					}
					if (tile2.type == 378 && tile2.frameX % 36 == 0 && tile2.frameY == 0)
					{
						int num13 = TileEntityType<TETrainingDummy>.Find(j, i);
						if (num13 != -1)
						{
							_compressEntities[num3++] = (short)num13;
						}
					}
					if (tile2.type == 395 && tile2.frameX % 36 == 0 && tile2.frameY == 0)
					{
						int num14 = TileEntityType<TEItemFrame>.Find(j, i);
						if (num14 != -1)
						{
							_compressEntities[num3++] = (short)num14;
						}
					}
					if (tile2.type == 698 && tile2.frameX % 18 == 0 && tile2.frameY == 0)
					{
						int num15 = TileEntityType<TEDeadCellsDisplayJar>.Find(j, i);
						if (num15 != -1)
						{
							_compressEntities[num3++] = (short)num15;
						}
					}
					if (tile2.type == 520 && tile2.frameX % 18 == 0 && tile2.frameY == 0)
					{
						int num16 = TileEntityType<TEFoodPlatter>.Find(j, i);
						if (num16 != -1)
						{
							_compressEntities[num3++] = (short)num16;
						}
					}
					if (tile2.type == 471 && tile2.frameX % 54 == 0 && tile2.frameY == 0)
					{
						int num17 = TileEntityType<TEWeaponsRack>.Find(j, i);
						if (num17 != -1)
						{
							_compressEntities[num3++] = (short)num17;
						}
					}
					if (tile2.type == 470 && tile2.frameX % 36 == 0 && tile2.frameY == 0)
					{
						int num18 = TileEntityType<TEDisplayDoll>.Find(j, i);
						if (num18 != -1)
						{
							_compressEntities[num3++] = (short)num18;
						}
					}
					if (tile2.type == 475 && tile2.frameX % 54 == 0 && tile2.frameY == 0)
					{
						int num19 = TileEntityType<TEHatRack>.Find(j, i);
						if (num19 != -1)
						{
							_compressEntities[num3++] = (short)num19;
						}
					}
					if (tile2.type == 597 && tile2.frameX % 54 == 0 && tile2.frameY % 72 == 0)
					{
						int num20 = TileEntityType<TETeleportationPylon>.Find(j, i);
						if (num20 != -1)
						{
							_compressEntities[num3++] = (short)num20;
						}
					}
					if (Main.tileFrameImportant[tile2.type])
					{
						array[num5] = (byte)(tile2.frameX & 0xFF);
						num5++;
						array[num5] = (byte)((tile2.frameX & 0xFF00) >> 8);
						num5++;
						array[num5] = (byte)(tile2.frameY & 0xFF);
						num5++;
						array[num5] = (byte)((tile2.frameY & 0xFF00) >> 8);
						num5++;
					}
					if (tile2.color() != 0)
					{
						b3 |= 8;
						array[num5] = tile2.color();
						num5++;
					}
				}
				if (tile2.wall != 0)
				{
					b |= 4;
					array[num5] = (byte)tile2.wall;
					num5++;
					if (tile2.wallColor() != 0)
					{
						b3 |= 0x10;
						array[num5] = tile2.wallColor();
						num5++;
					}
				}
				if (tile2.liquid != 0)
				{
					if (!tile2.shimmer())
					{
						b = (tile2.lava() ? ((byte)(b | 0x10)) : ((!tile2.honey()) ? ((byte)(b | 8)) : ((byte)(b | 0x18))));
					}
					else
					{
						b3 |= 0x80;
						b |= 8;
					}
					array[num5] = tile2.liquid;
					num5++;
				}
				if (tile2.wire())
				{
					b2 |= 2;
				}
				if (tile2.wire2())
				{
					b2 |= 4;
				}
				if (tile2.wire3())
				{
					b2 |= 8;
				}
				int num21 = (tile2.halfBrick() ? 16 : ((tile2.slope() != 0) ? (tile2.slope() + 1 << 4) : 0));
				b2 |= (byte)num21;
				if (tile2.actuator())
				{
					b3 |= 2;
				}
				if (tile2.inActive())
				{
					b3 |= 4;
				}
				if (tile2.wire4())
				{
					b3 |= 0x20;
				}
				if (tile2.wall > 255)
				{
					array[num5] = (byte)(tile2.wall >> 8);
					num5++;
					b3 |= 0x40;
				}
				if (tile2.invisibleBlock())
				{
					b4 |= 2;
				}
				if (tile2.invisibleWall())
				{
					b4 |= 4;
				}
				if (tile2.fullbrightBlock())
				{
					b4 |= 8;
				}
				if (tile2.fullbrightWall())
				{
					b4 |= 0x10;
				}
				num6 = 3;
				if (b4 != 0)
				{
					b3 |= 1;
					array[num6] = b4;
					num6--;
				}
				if (b3 != 0)
				{
					b2 |= 1;
					array[num6] = b3;
					num6--;
				}
				if (b2 != 0)
				{
					b |= 1;
					array[num6] = b2;
					num6--;
				}
				tile = tile2;
			}
		}
		if (num4 > 0)
		{
			array[num5] = (byte)(num4 & 0xFF);
			num5++;
			if (num4 > 255)
			{
				b |= 0x80;
				array[num5] = (byte)((num4 & 0xFF00) >> 8);
				num5++;
			}
			else
			{
				b |= 0x40;
			}
		}
		array[num6] = b;
		writer.Write(array, num6, num5 - num6);
		writer.Write(num);
		for (int k = 0; k < num; k++)
		{
			Chest chest = Main.chest[_compressChestList[k]];
			writer.Write(_compressChestList[k]);
			writer.Write((short)chest.x);
			writer.Write((short)chest.y);
			writer.Write(chest.name);
		}
		writer.Write(num2);
		for (int l = 0; l < num2; l++)
		{
			Sign sign = Main.sign[_compressSignList[l]];
			writer.Write(_compressSignList[l]);
			writer.Write((short)sign.x);
			writer.Write((short)sign.y);
			writer.Write(sign.text);
		}
		writer.Write(num3);
		for (int m = 0; m < num3; m++)
		{
			TileEntity.Write(writer, TileEntity.ByID[_compressEntities[m]]);
		}
	}

	public static void DecompressTileBlock(Stream stream)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Expected O, but got Unknown
		DeflateStream val = new DeflateStream(stream, (CompressionMode)1, true);
		try
		{
			BinaryReader binaryReader = new BinaryReader((Stream)(object)val);
			DecompressTileBlock_Inner(binaryReader, binaryReader.ReadInt32(), binaryReader.ReadInt32(), binaryReader.ReadInt16(), binaryReader.ReadInt16());
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static void DecompressTileBlock_Inner(BinaryReader reader, int xStart, int yStart, int width, int height)
	{
		Tile tile = null;
		int num = 0;
		for (int i = yStart; i < yStart + height; i++)
		{
			for (int j = xStart; j < xStart + width; j++)
			{
				if (num != 0)
				{
					num--;
					if (Main.tile[j, i] == null)
					{
						Main.tile[j, i] = new Tile(tile);
					}
					else
					{
						Main.tile[j, i].CopyFrom(tile);
					}
					continue;
				}
				byte b2;
				byte b3;
				byte b = (b2 = (b3 = 0));
				tile = Main.tile[j, i];
				if (tile == null)
				{
					tile = new Tile();
					Main.tile[j, i] = tile;
				}
				else
				{
					tile.ClearEverything();
				}
				byte b4 = reader.ReadByte();
				bool flag = false;
				if ((b4 & 1) == 1)
				{
					flag = true;
					b = reader.ReadByte();
				}
				bool flag2 = false;
				if (flag && (b & 1) == 1)
				{
					flag2 = true;
					b2 = reader.ReadByte();
				}
				if (flag2 && (b2 & 1) == 1)
				{
					b3 = reader.ReadByte();
				}
				bool flag3 = tile.active();
				byte b5;
				if ((b4 & 2) == 2)
				{
					tile.active(active: true);
					ushort type = tile.type;
					int num2;
					if ((b4 & 0x20) == 32)
					{
						b5 = reader.ReadByte();
						num2 = reader.ReadByte();
						num2 = (num2 << 8) | b5;
					}
					else
					{
						num2 = reader.ReadByte();
					}
					tile.type = (ushort)num2;
					if (Main.tileFrameImportant[num2])
					{
						tile.frameX = reader.ReadInt16();
						tile.frameY = reader.ReadInt16();
					}
					else if (!flag3 || tile.type != type)
					{
						tile.frameX = -1;
						tile.frameY = -1;
					}
					if ((b2 & 8) == 8)
					{
						tile.color(reader.ReadByte());
					}
				}
				if ((b4 & 4) == 4)
				{
					tile.wall = reader.ReadByte();
					if ((b2 & 0x10) == 16)
					{
						tile.wallColor(reader.ReadByte());
					}
				}
				b5 = (byte)((b4 & 0x18) >> 3);
				if (b5 != 0)
				{
					tile.liquid = reader.ReadByte();
					if ((b2 & 0x80) == 128)
					{
						tile.shimmer(shimmer: true);
					}
					else if (b5 > 1)
					{
						if (b5 == 2)
						{
							tile.lava(lava: true);
						}
						else
						{
							tile.honey(honey: true);
						}
					}
				}
				if (b > 1)
				{
					if ((b & 2) == 2)
					{
						tile.wire(wire: true);
					}
					if ((b & 4) == 4)
					{
						tile.wire2(wire2: true);
					}
					if ((b & 8) == 8)
					{
						tile.wire3(wire3: true);
					}
					b5 = (byte)((b & 0x70) >> 4);
					if (b5 != 0 && Main.tileSolid[tile.type])
					{
						if (b5 == 1)
						{
							tile.halfBrick(halfBrick: true);
						}
						else
						{
							tile.slope((byte)(b5 - 1));
						}
					}
				}
				if (b2 > 1)
				{
					if ((b2 & 2) == 2)
					{
						tile.actuator(actuator: true);
					}
					if ((b2 & 4) == 4)
					{
						tile.inActive(inActive: true);
					}
					if ((b2 & 0x20) == 32)
					{
						tile.wire4(wire4: true);
					}
					if ((b2 & 0x40) == 64)
					{
						b5 = reader.ReadByte();
						tile.wall = (ushort)((b5 << 8) | tile.wall);
					}
				}
				if (b3 > 1)
				{
					if ((b3 & 2) == 2)
					{
						tile.invisibleBlock(invisibleBlock: true);
					}
					if ((b3 & 4) == 4)
					{
						tile.invisibleWall(invisibleWall: true);
					}
					if ((b3 & 8) == 8)
					{
						tile.fullbrightBlock(fullbrightBlock: true);
					}
					if ((b3 & 0x10) == 16)
					{
						tile.fullbrightWall(fullbrightWall: true);
					}
				}
				num = (byte)((b4 & 0xC0) >> 6) switch
				{
					0 => 0, 
					1 => reader.ReadByte(), 
					_ => reader.ReadInt16(), 
				};
			}
		}
		short num3 = reader.ReadInt16();
		for (int k = 0; k < num3; k++)
		{
			short num4 = reader.ReadInt16();
			short x = reader.ReadInt16();
			short y = reader.ReadInt16();
			string name = reader.ReadString();
			if (num4 >= 0 && num4 < 8000)
			{
				Chest.CreateWorldChest(num4, x, y).name = name;
			}
		}
		num3 = reader.ReadInt16();
		for (int l = 0; l < num3; l++)
		{
			short num5 = reader.ReadInt16();
			short x2 = reader.ReadInt16();
			short y2 = reader.ReadInt16();
			string text = reader.ReadString();
			if (num5 >= 0 && num5 < 32000)
			{
				if (Main.sign[num5] == null)
				{
					Main.sign[num5] = new Sign();
				}
				Main.sign[num5].text = text;
				Main.sign[num5].x = x2;
				Main.sign[num5].y = y2;
			}
		}
		num3 = reader.ReadInt16();
		for (int m = 0; m < num3; m++)
		{
			TileEntity.Add(TileEntity.Read(reader, 318));
		}
		MapUpdateQueue.Add(new Rectangle(xStart, yStart, width, height));
		Main.sectionManager.SetTilesLoaded(xStart, yStart, xStart + width - 1, yStart + height - 1);
	}

	public static void ReceiveBytes(byte[] bytes, int streamLength, int i = 256)
	{
		lock (buffer[i])
		{
			try
			{
				Buffer.BlockCopy(bytes, 0, buffer[i].readBuffer, buffer[i].totalData, streamLength);
				buffer[i].totalData += streamLength;
				buffer[i].checkBytes = true;
			}
			catch
			{
				if (Main.netMode == 1)
				{
					Main.menuMode = 15;
					Main.statusText = Language.GetTextValue("Error.BadHeaderBufferOverflow");
					Netplay.Disconnect = true;
				}
				else
				{
					Netplay.Clients[i].PendingTermination = true;
				}
			}
		}
	}

	public static void CheckBytes(int bufferIndex = 256)
	{
		if (Main.dedServ && Netplay.Clients[bufferIndex].PendingTermination)
		{
			Netplay.Clients[bufferIndex].PendingTerminationApproved = true;
			return;
		}
		if (!Main.dedServ && !Netplay.Connection.IsConnected() && !Netplay.Connection.IsReading && !buffer[bufferIndex].checkBytes)
		{
			Netplay.Disconnect = true;
			Main.statusText = Language.GetTextValue("Net.LostConnection");
		}
		if (!buffer[bufferIndex].checkBytes)
		{
			return;
		}
		lock (buffer[bufferIndex])
		{
			buffer[bufferIndex].checkBytes = false;
			int num = 0;
			int num2 = buffer[bufferIndex].totalData;
			try
			{
				while (num2 >= 2)
				{
					int num3 = BitConverter.ToUInt16(buffer[bufferIndex].readBuffer, num);
					if (num3 < 3)
					{
						throw new IndexOutOfRangeException("Invalid packet. Message size too small (" + num3 + ")");
					}
					if (num2 >= num3)
					{
						long position = buffer[bufferIndex].reader.BaseStream.Position;
						buffer[bufferIndex].GetData(num + 2, num3 - 2, out var _);
						buffer[bufferIndex].reader.BaseStream.Position = position + num3;
						num2 -= num3;
						num += num3;
						continue;
					}
					break;
				}
			}
			catch (Exception)
			{
				if (Main.dedServ && num < buffer.Length - 100)
				{
					Console.WriteLine(Language.GetTextValue("Error.NetMessageError", buffer[num + 2]));
				}
				num2 = 0;
				num = 0;
			}
			if (num2 != buffer[bufferIndex].totalData)
			{
				for (int i = 0; i < num2; i++)
				{
					buffer[bufferIndex].readBuffer[i] = buffer[bufferIndex].readBuffer[i + num];
				}
				buffer[bufferIndex].totalData = num2;
			}
		}
	}

	public static void BootPlayer(int plr, NetworkText msg)
	{
		SendData(2, plr, -1, msg);
	}

	public static void SendObjectPlacement(int whoAmi, int x, int y, int type, int style, int alternative, int random, int direction)
	{
		int remoteClient;
		int ignoreClient;
		if (Main.netMode == 2)
		{
			remoteClient = -1;
			ignoreClient = whoAmi;
		}
		else
		{
			remoteClient = whoAmi;
			ignoreClient = -1;
		}
		SendData(79, remoteClient, ignoreClient, null, x, y, type, style, alternative, random, direction);
	}

	public static void SendTemporaryAnimation(int whoAmi, int animationType, int tileType, int xCoord, int yCoord)
	{
		if (Main.netMode == 2)
		{
			SendData(77, whoAmi, -1, null, animationType, tileType, xCoord, yCoord);
		}
	}

	public static void SendPlayerHurt(int playerTargetIndex, PlayerDeathReason reason, int damage, int direction, bool critical, bool pvp, int hitContext, int remoteClient = -1, int ignoreClient = -1)
	{
		_currentPlayerDeathReason = reason;
		BitsByte bitsByte = (byte)0;
		bitsByte[0] = critical;
		bitsByte[1] = pvp;
		SendData(117, remoteClient, ignoreClient, null, playerTargetIndex, damage, direction, (int)(byte)bitsByte, hitContext);
	}

	public static void SendPlayerDeath(int playerTargetIndex, PlayerDeathReason reason, int damage, int direction, bool pvp, int remoteClient = -1, int ignoreClient = -1)
	{
		_currentPlayerDeathReason = reason;
		BitsByte bitsByte = (byte)0;
		bitsByte[0] = pvp;
		SendData(118, remoteClient, ignoreClient, null, playerTargetIndex, damage, direction, (int)(byte)bitsByte);
	}

	public static void PlayNetSound(NetSoundInfo info, int remoteClient = -1, int ignoreClient = -1)
	{
		_currentNetSoundInfo = info;
		SendData(132, remoteClient, ignoreClient);
	}

	public static void SendCoinLossRevengeMarker(CoinLossRevengeSystem.RevengeMarker marker, int remoteClient = -1, int ignoreClient = -1)
	{
		_currentRevengeMarker = marker;
		SendData(126, remoteClient, ignoreClient);
	}

	public static void SendTileSquare(int whoAmi, int tileX, int tileY, int xSize, int ySize, TileChangeType changeType = TileChangeType.None)
	{
		SendData(20, whoAmi, -1, null, tileX, tileY, xSize, ySize, (int)changeType);
	}

	public static void SendTileSquare(int whoAmi, int tileX, int tileY, int centeredSquareSize, TileChangeType changeType = TileChangeType.None)
	{
		int num = (centeredSquareSize - 1) / 2;
		SendTileSquare(whoAmi, tileX - num, tileY - num, centeredSquareSize, centeredSquareSize, changeType);
	}

	public static void SendTileSquare(int whoAmi, int tileX, int tileY, TileChangeType changeType = TileChangeType.None)
	{
		int num = 1;
		int num2 = (num - 1) / 2;
		SendTileSquare(whoAmi, tileX - num2, tileY - num2, num, num, changeType);
	}

	public static void SendTravelShop(int remoteClient)
	{
		if (Main.netMode == 2)
		{
			SendData(72, remoteClient);
		}
	}

	public static void SendAnglerQuest(int remoteClient)
	{
		if (Main.netMode != 2)
		{
			return;
		}
		if (remoteClient == -1)
		{
			for (int i = 0; i < 255; i++)
			{
				if (Netplay.Clients[i].State == 10)
				{
					SendData(74, i, -1, NetworkText.FromLiteral(Main.player[i].name), Main.anglerQuest);
				}
			}
		}
		else if (Netplay.Clients[remoteClient].State == 10)
		{
			SendData(74, remoteClient, -1, NetworkText.FromLiteral(Main.player[remoteClient].name), Main.anglerQuest);
		}
	}

	public static void ResyncTiles(Rectangle area)
	{
		for (int i = 0; i < Netplay.Clients.Length; i++)
		{
			if (Netplay.Clients[i].IsActive)
			{
				ResyncTiles(i, area);
			}
		}
	}

	private static void ResyncTiles(int clientId, Rectangle area)
	{
		for (int i = area.Left; i < area.Right; i += 200)
		{
			for (int j = area.Top; j < area.Bottom; j += 150)
			{
				SendData(10, clientId, -1, null, i, j, Math.Min(area.Right - i, 200), Math.Min(area.Bottom - j, 150));
			}
		}
	}

	public static void SendSection(int whoAmi, int sectionX, int sectionY)
	{
		if (Main.netMode != 2)
		{
			return;
		}
		try
		{
			if (sectionX >= 0 && sectionY >= 0 && sectionX < Main.maxSectionsX && sectionY < Main.maxSectionsY && !Netplay.Clients[whoAmi].TileSections[sectionX, sectionY])
			{
				Netplay.Clients[whoAmi].TileSections[sectionX, sectionY] = true;
				int number = sectionX * 200;
				int num = sectionY * 150;
				int num2 = 150;
				for (int i = num; i < num + 150; i += num2)
				{
					SendData(10, whoAmi, -1, null, number, i, 200f, num2);
				}
				SyncNPCsForSection(whoAmi, sectionX, sectionY);
				SyncChestContentsForSection(whoAmi, sectionX, sectionY);
			}
		}
		catch
		{
		}
	}

	private static void SyncChestContentsForSection(int whoAmi, int sectionX, int sectionY)
	{
		for (int i = 0; i < 8000; i++)
		{
			Chest chest = Main.chest[i];
			if (chest != null)
			{
				int sectionX2 = Netplay.GetSectionX(chest.x);
				int sectionY2 = Netplay.GetSectionY(chest.y);
				if (sectionX == sectionX2 && sectionY == sectionY2)
				{
					SendChestContentsTo(i, whoAmi);
				}
			}
		}
	}

	private static void SyncNPCsForSection(int whoAmi, int sectionX, int sectionY)
	{
		for (int i = 0; i < Main.maxNPCs; i++)
		{
			if (Main.npc[i].active && Main.npc[i].townNPC)
			{
				int sectionX2 = Netplay.GetSectionX((int)(Main.npc[i].position.X / 16f));
				int sectionY2 = Netplay.GetSectionY((int)(Main.npc[i].position.Y / 16f));
				if (sectionX2 == sectionX && sectionY2 == sectionY)
				{
					SendData(23, whoAmi, -1, null, i);
				}
			}
		}
	}

	public static void greetPlayer(int plr)
	{
		if (Main.motd == "")
		{
			ChatHelper.SendChatMessageToClient(NetworkText.FromFormattable("{0} {1}!", Lang.mp[18].ToNetworkText(), Main.worldName), new Color(255, 240, 20), plr);
		}
		else
		{
			ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral(Main.motd), new Color(255, 240, 20), plr);
		}
		string text = "";
		for (int i = 0; i < 255; i++)
		{
			if (Main.player[i].active)
			{
				text = ((!(text == "")) ? (text + ", " + Main.player[i].name) : (text + Main.player[i].name));
			}
		}
		ChatHelper.SendChatMessageToClient(NetworkText.FromKey("Game.JoinGreeting", text), new Color(255, 240, 20), plr);
	}

	public static void sendWater(int x, int y)
	{
		if (Main.netMode == 1)
		{
			SendData(48, -1, -1, null, x, y);
			return;
		}
		for (int i = 0; i < 256; i++)
		{
			if ((buffer[i].broadcast || Netplay.Clients[i].State >= 3) && Netplay.Clients[i].IsConnected())
			{
				int num = x / 200;
				int num2 = y / 150;
				if (Netplay.Clients[i].TileSections[num, num2])
				{
					SendData(48, i, -1, null, x, y);
				}
			}
		}
	}

	public static void SyncDisconnectedPlayer(int plr)
	{
		SyncOnePlayer(plr, -1, plr);
		EnsureLocalPlayerIsPresent();
	}

	public static void SyncConnectedPlayer(int plr)
	{
		SyncOnePlayer(plr, -1, plr);
		for (int i = 0; i < 255; i++)
		{
			if (plr != i && Main.player[i].active)
			{
				SyncOnePlayer(i, plr, -1);
			}
		}
		SendNPCHousesAndTravelShop(plr);
		SendAnglerQuest(plr);
		CreditsRollEvent.SendCreditsRollRemainingTimeToPlayer(plr);
		NPC.RevengeManager.SendAllMarkersToPlayer(plr);
		EnsureLocalPlayerIsPresent();
		DebugOptions.SyncToJoiningPlayer(plr);
	}

	private static void SendNPCHousesAndTravelShop(int plr)
	{
		bool flag = false;
		for (int i = 0; i < Main.maxNPCs; i++)
		{
			NPC nPC = Main.npc[i];
			if (!nPC.active)
			{
				continue;
			}
			bool flag2 = nPC.townNPC && NPC.TypeToDefaultHeadIndex(nPC.type) > 0;
			if (nPC.aiStyle == 7)
			{
				flag2 = true;
			}
			if (flag2)
			{
				if (!flag && nPC.type == 368)
				{
					flag = true;
				}
				byte householdStatus = WorldGen.TownManager.GetHouseholdStatus(nPC);
				SendData(60, plr, -1, null, i, nPC.homeTileX, nPC.homeTileY, (int)householdStatus);
			}
		}
		if (flag)
		{
			SendTravelShop(plr);
		}
	}

	private static void EnsureLocalPlayerIsPresent()
	{
		if (!Main.autoShutdown)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < 255; i++)
		{
			if (DoesPlayerSlotCountAsAHost(i))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			Console.WriteLine(Language.GetTextValue("Net.ServerAutoShutdown"));
			Netplay.Disconnect = true;
		}
	}

	public static bool DoesPlayerSlotCountAsAHost(int plr)
	{
		if (Netplay.Clients[plr].State == 10)
		{
			return Netplay.Clients[plr].Socket.GetRemoteAddress().IsLocalHost();
		}
		return false;
	}

	private static void SyncOnePlayer(int plr, int toWho, int fromWho)
	{
		int num = 0;
		if (Main.player[plr].active)
		{
			num = 1;
		}
		if (Netplay.Clients[plr].State == 10)
		{
			SendData(14, toWho, fromWho, null, plr, num);
			SendData(4, toWho, fromWho, null, plr);
			SendData(13, toWho, fromWho, null, plr);
			if (Main.player[plr].statLife <= 0)
			{
				SendData(135, toWho, fromWho, null, plr);
			}
			SendData(16, toWho, fromWho, null, plr);
			SendData(30, toWho, fromWho, null, plr);
			SendData(45, toWho, fromWho, null, plr);
			SendData(42, toWho, fromWho, null, plr);
			SendData(50, toWho, fromWho, null, plr);
			SendData(80, toWho, fromWho, null, plr, Main.player[plr].chest);
			SendData(142, toWho, fromWho, null, plr);
			SendData(147, toWho, fromWho, null, plr, Main.player[plr].CurrentLoadoutIndex);
			TagEffectState.NetModule.SyncStateIfNecessary(Main.player[plr].TagEffectState, toWho, fromWho);
			for (int i = 0; i < 59; i++)
			{
				SendData(5, toWho, fromWho, null, plr, PlayerItemSlotID.Inventory0 + i);
			}
			for (int j = 0; j < Main.player[plr].armor.Length; j++)
			{
				SendData(5, toWho, fromWho, null, plr, PlayerItemSlotID.Armor0 + j);
			}
			for (int k = 0; k < Main.player[plr].dye.Length; k++)
			{
				SendData(5, toWho, fromWho, null, plr, PlayerItemSlotID.Dye0 + k);
			}
			SyncOnePlayer_ItemArray(plr, toWho, fromWho, Main.player[plr].miscEquips, PlayerItemSlotID.Misc0);
			SyncOnePlayer_ItemArray(plr, toWho, fromWho, Main.player[plr].miscDyes, PlayerItemSlotID.MiscDye0);
			SyncOnePlayer_ItemArray(plr, toWho, fromWho, Main.player[plr].Loadouts[0].Armor, PlayerItemSlotID.Loadout1_Armor_0);
			SyncOnePlayer_ItemArray(plr, toWho, fromWho, Main.player[plr].Loadouts[0].Dye, PlayerItemSlotID.Loadout1_Dye_0);
			SyncOnePlayer_ItemArray(plr, toWho, fromWho, Main.player[plr].Loadouts[1].Armor, PlayerItemSlotID.Loadout2_Armor_0);
			SyncOnePlayer_ItemArray(plr, toWho, fromWho, Main.player[plr].Loadouts[1].Dye, PlayerItemSlotID.Loadout2_Dye_0);
			SyncOnePlayer_ItemArray(plr, toWho, fromWho, Main.player[plr].Loadouts[2].Armor, PlayerItemSlotID.Loadout3_Armor_0);
			SyncOnePlayer_ItemArray(plr, toWho, fromWho, Main.player[plr].Loadouts[2].Dye, PlayerItemSlotID.Loadout3_Dye_0);
			if (!Netplay.Clients[plr].IsAnnouncementCompleted)
			{
				Netplay.Clients[plr].IsAnnouncementCompleted = true;
				ChatHelper.BroadcastChatMessage(NetworkText.FromKey(Lang.mp[19].Key, Main.player[plr].name), new Color(255, 240, 20), plr);
				if (Main.dedServ)
				{
					Console.WriteLine(Lang.mp[19].Format(Main.player[plr].name));
				}
			}
			for (int l = 0; l < 1000; l++)
			{
				Projectile projectile = Main.projectile[l];
				if (projectile.active && projectile.owner == plr)
				{
					SendData(27, toWho, -1, null, l);
				}
			}
			return;
		}
		num = 0;
		SendData(14, -1, plr, null, plr, num);
		if (Netplay.Clients[plr].IsAnnouncementCompleted)
		{
			Netplay.Clients[plr].IsAnnouncementCompleted = false;
			ChatHelper.BroadcastChatMessage(NetworkText.FromKey(Lang.mp[20].Key, Netplay.Clients[plr].Name), new Color(255, 240, 20), plr);
			if (Main.dedServ)
			{
				Console.WriteLine(Lang.mp[20].Format(Netplay.Clients[plr].Name));
			}
			Netplay.Clients[plr].Name = "Anonymous";
		}
		Player.Hooks.PlayerDisconnect(plr);
	}

	private static void SyncOnePlayer_ItemArray(int plr, int toWho, int fromWho, Item[] arr, int slot)
	{
		for (int i = 0; i < arr.Length; i++)
		{
			SendData(5, toWho, fromWho, null, plr, slot + i);
		}
	}
}
