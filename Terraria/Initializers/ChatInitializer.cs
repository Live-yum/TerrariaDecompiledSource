using Terraria.Chat.Commands;
using Terraria.GameContent.UI.Chat;
using Terraria.UI.Chat;

namespace Terraria.Initializers;

public static class ChatInitializer
{
	public static void Load()
	{
		ChatManager.Register<ColorTagHandler>(new string[2] { "c", "color" });
		ChatManager.Register<ItemTagHandler>(new string[2] { "i", "item" });
		ChatManager.Register<NameTagHandler>(new string[2] { "n", "name" });
		ChatManager.Register<AchievementTagHandler>(new string[2] { "a", "achievement" });
		ChatManager.Register<GlyphTagHandler>(new string[2] { "g", "glyph" });
		ChatManager.Register<GlyphTagHandler.GlyphXboxTagHandler>(new string[2] { "gx", "glyph" });
		ChatManager.Register<GlyphTagHandler.GlyphPSTagHandler>(new string[2] { "gp", "glyph" });
		ChatManager.Register<GlyphTagHandler.GlyphSwitchTagHandler>(new string[2] { "gn", "glyph" });
		ChatManager.Commands.AddCommand<PartyChatCommand>().AddCommand<RollCommand>().AddCommand<EmoteCommand>()
			.AddCommand<ListPlayersCommand>()
			.AddCommand<RockPaperScissorsCommand>()
			.AddCommand<EmojiCommand>()
			.AddCommand<HelpCommand>()
			.AddCommand<DeathCommand>()
			.AddCommand<PVPDeathCommand>()
			.AddCommand<AllDeathCommand>()
			.AddCommand<AllPVPDeathCommand>()
			.AddCommand<BossDamageCommand>()
			.AddDefaultCommand<SayChatCommand>();
		ChatManager.Commands.PrepareAliases();
	}
}
