using Microsoft.Xna.Framework;
using Terraria.Localization;

namespace Terraria.Testing;

public static class DebugOptions
{
	public static bool enableDebugCommands = false;

	public static bool Shared_ReportCommandUsage = true;

	public static int Shared_ServerPing = 0;

	public static double UpdateWaitInMs = 0.0;

	public static double DrawWaitInMs = 0.0;

	public static bool devLightTilesCheat;

	public static bool noLimits;

	public static bool noPause;

	public static int unlockMap;

	public static bool ShowSections;

	public static bool ShowUnbreakableWall;

	public static bool DrawLinkPoints;

	public static bool ShowNetOffsetDust;

	public static Vector2 FakeNetOffset;

	public static bool hideTiles = false;

	public static bool hideTiles2 = false;

	public static bool hideWalls = false;

	public static bool hideWater = false;

	public static bool NoDamageVar;

	public static bool LetProjectilesAimAtTargetDummies;

	public static bool PracticeMode;

	public static void SyncToJoiningPlayer(int playerIndex)
	{
		if (enableDebugCommands)
		{
			NetMessage.SendData(94, playerIndex, -1, NetworkText.FromLiteral("/showdebug"), 0, Shared_ReportCommandUsage ? 1 : 0);
			NetMessage.SendData(94, playerIndex, -1, NetworkText.FromLiteral("/setserverping"), 0, Shared_ServerPing);
		}
	}
}
