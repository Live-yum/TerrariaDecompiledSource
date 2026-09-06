using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria;

internal static class ItemCatalogExporter
{
	private const string TerrariaVersion = "1.4.5.8";
	private const string SourceCommit = "8255d34616c780af12079425ac92a0a7aed87d71";
	private const int ShardSize = 1024;

	private static readonly string[] GameplaySchema =
	{
		"type", "width", "height", "maxStack", "damage", "defense", "knockBack", "crit", "armorPenetration",
		"useTime", "useAnimation", "useStyle", "reuseDelay", "mana", "healLife", "healMana", "pick", "axe", "hammer",
		"tileBoost", "createTile", "createWall", "placeStyle", "shoot", "shootSpeed", "ammo", "useAmmo", "buffType", "buffTime",
		"rare", "value", "headSlot", "bodySlot", "legSlot", "handOnSlot", "handOffSlot", "backSlot", "frontSlot", "shoeSlot",
		"waistSlot", "wingSlot", "shieldSlot", "neckSlot", "faceSlot", "balloonSlot", "beardSlot", "voiceSlot", "mountType",
		"fishingPole", "bait", "makeNPC", "dye", "hairDye", "paint", "paintCoating", "holdStyle", "channel", "accessory",
		"potion", "consumable", "autoReuse", "useTurn", "alpha", "glowMask", "scale", "lifeRegen", "manaIncrease", "noUseGraphic",
		"noMelee", "social", "vanity", "material", "noWet", "notAmmo", "cartTrack", "uniqueStack", "shopSpecialCurrency",
		"shopCustomPrice", "shootsEveryUse", "chlorophyteExtractinatorConsumable", "DD2Summon", "melee", "magic", "ranged",
		"summon", "sentry", "questItem", "expertOnly", "expert", "isAShopItem", "newAndShiny", "hasVanityEffects", "color"
	};

	public static void Export(string outputDirectory)
	{
		if (string.IsNullOrWhiteSpace(outputDirectory))
		{
			outputDirectory = "catalog-export";
		}
		outputDirectory = Path.GetFullPath(outputDirectory);
		Directory.CreateDirectory(outputDirectory);

		SetCulture(GameCulture.CultureName.English);
		Dictionary<int, Dictionary<string, object>> records = new Dictionary<int, Dictionary<string, object>>();
		for (int id = 1; id < ItemID.Count; id++)
		{
			Item item = new Item();
			item.SetDefaults(id);
			Dictionary<string, object> record = new Dictionary<string, object>
			{
				["id"] = id,
				["internalName"] = ItemID.Search.GetName(id) ?? string.Empty,
				["names"] = new Dictionary<string, string>
				{
					["en-US"] = Lang.GetItemNameValue(id) ?? string.Empty
				},
				["tooltips"] = new Dictionary<string, string[]>
				{
					["en-US"] = GetTooltipLines(id)
				},
				["gameplay"] = GetGameplay(item)
			};
			records[id] = record;
		}

		SetCulture(GameCulture.CultureName.Chinese);
		for (int id = 1; id < ItemID.Count; id++)
		{
			Dictionary<string, object> record = records[id];
			((Dictionary<string, string>)record["names"])["zh-Hans"] = Lang.GetItemNameValue(id) ?? string.Empty;
			((Dictionary<string, string[]>)record["tooltips"])["zh-Hans"] = GetTooltipLines(id);
		}

		List<Dictionary<string, object>> shards = new List<Dictionary<string, object>>();
		for (int startId = 1; startId < ItemID.Count; startId += ShardSize)
		{
			int endId = Math.Min(ItemID.Count - 1, startId + ShardSize - 1);
			List<Dictionary<string, object>> items = new List<Dictionary<string, object>>(endId - startId + 1);
			for (int id = startId; id <= endId; id++)
			{
				items.Add(records[id]);
			}
			string fileName = string.Format("items-{0:D4}-{1:D4}.json", startId, endId);
			WriteJson(Path.Combine(outputDirectory, fileName), new Dictionary<string, object>
			{
				["schemaVersion"] = 1,
				["terrariaVersion"] = TerrariaVersion,
				["sourceCommit"] = SourceCommit,
				["startId"] = startId,
				["endId"] = endId,
				["count"] = items.Count,
				["items"] = items
			});
			shards.Add(new Dictionary<string, object>
			{
				["file"] = fileName,
				["startId"] = startId,
				["endId"] = endId,
				["count"] = items.Count
			});
		}

		WriteJson(Path.Combine(outputDirectory, "manifest.json"), new Dictionary<string, object>
		{
			["schemaVersion"] = 1,
			["terrariaVersion"] = TerrariaVersion,
			["sourceRepository"] = "Live-yan/TerrariaDecompiledSource",
			["sourceCommit"] = SourceCommit,
			["itemIdCount"] = (int)ItemID.Count,
			["itemCount"] = (int)ItemID.Count - 1,
			["firstItemId"] = 1,
			["lastItemId"] = (int)ItemID.Count - 1,
			["locales"] = new[] { "en-US", "zh-Hans" },
			["gameplaySchema"] = GameplaySchema,
			["tooltipSource"] = "Lang.GetTooltip",
			["gameplaySource"] = "Item.SetDefaults",
			["shards"] = shards
		});

		Console.WriteLine("Exported {0} Terraria {1} items to {2}", ItemID.Count - 1, TerrariaVersion, outputDirectory);
	}

	private static void SetCulture(GameCulture.CultureName cultureName)
	{
		LanguageManager.Instance.SetLanguage(GameCulture.FromCultureName(cultureName));
		ItemTooltip.InvalidateTooltips();
		Lang.InitializeLegacyLocalization();
	}

	private static string[] GetTooltipLines(int id)
	{
		ItemTooltip tooltip = Lang.GetTooltip(id);
		if (tooltip == null || tooltip.Lines <= 0)
		{
			return Array.Empty<string>();
		}
		string[] lines = new string[tooltip.Lines];
		for (int i = 0; i < lines.Length; i++)
		{
			lines[i] = tooltip.GetLine(i) ?? string.Empty;
		}
		return lines;
	}

	private static Dictionary<string, object> GetGameplay(Item item)
	{
		return new Dictionary<string, object>
		{
			["type"] = item.type,
			["width"] = item.width,
			["height"] = item.height,
			["maxStack"] = item.maxStack,
			["damage"] = item.damage,
			["defense"] = item.defense,
			["knockBack"] = item.knockBack,
			["crit"] = item.crit,
			["armorPenetration"] = item.armorPenetration,
			["useTime"] = item.useTime,
			["useAnimation"] = item.useAnimation,
			["useStyle"] = item.useStyle,
			["reuseDelay"] = item.reuseDelay,
			["mana"] = item.mana,
			["healLife"] = item.healLife,
			["healMana"] = item.healMana,
			["pick"] = item.pick,
			["axe"] = item.axe,
			["hammer"] = item.hammer,
			["tileBoost"] = item.tileBoost,
			["createTile"] = item.createTile,
			["createWall"] = item.createWall,
			["placeStyle"] = item.placeStyle,
			["shoot"] = item.shoot,
			["shootSpeed"] = item.shootSpeed,
			["ammo"] = item.ammo,
			["useAmmo"] = item.useAmmo,
			["buffType"] = item.buffType,
			["buffTime"] = item.buffTime,
			["rare"] = item.rare,
			["value"] = item.value,
			["headSlot"] = item.headSlot,
			["bodySlot"] = item.bodySlot,
			["legSlot"] = item.legSlot,
			["handOnSlot"] = item.handOnSlot,
			["handOffSlot"] = item.handOffSlot,
			["backSlot"] = item.backSlot,
			["frontSlot"] = item.frontSlot,
			["shoeSlot"] = item.shoeSlot,
			["waistSlot"] = item.waistSlot,
			["wingSlot"] = item.wingSlot,
			["shieldSlot"] = item.shieldSlot,
			["neckSlot"] = item.neckSlot,
			["faceSlot"] = item.faceSlot,
			["balloonSlot"] = item.balloonSlot,
			["beardSlot"] = item.beardSlot,
			["voiceSlot"] = item.voiceSlot,
			["mountType"] = item.mountType,
			["fishingPole"] = item.fishingPole,
			["bait"] = item.bait,
			["makeNPC"] = item.makeNPC,
			["dye"] = item.dye,
			["hairDye"] = item.hairDye,
			["paint"] = item.paint,
			["paintCoating"] = item.paintCoating,
			["holdStyle"] = item.holdStyle,
			["channel"] = item.channel,
			["accessory"] = item.accessory,
			["potion"] = item.potion,
			["consumable"] = item.consumable,
			["autoReuse"] = item.autoReuse,
			["useTurn"] = item.useTurn,
			["alpha"] = item.alpha,
			["glowMask"] = item.glowMask,
			["scale"] = item.scale,
			["lifeRegen"] = item.lifeRegen,
			["manaIncrease"] = item.manaIncrease,
			["noUseGraphic"] = item.noUseGraphic,
			["noMelee"] = item.noMelee,
			["social"] = item.social,
			["vanity"] = item.vanity,
			["material"] = item.material,
			["noWet"] = item.noWet,
			["notAmmo"] = item.notAmmo,
			["cartTrack"] = item.cartTrack,
			["uniqueStack"] = item.uniqueStack,
			["shopSpecialCurrency"] = item.shopSpecialCurrency,
			["shopCustomPrice"] = item.shopCustomPrice,
			["shootsEveryUse"] = item.shootsEveryUse,
			["chlorophyteExtractinatorConsumable"] = item.chlorophyteExtractinatorConsumable,
			["DD2Summon"] = item.DD2Summon,
			["melee"] = item.melee,
			["magic"] = item.magic,
			["ranged"] = item.ranged,
			["summon"] = item.summon,
			["sentry"] = item.sentry,
			["questItem"] = item.questItem,
			["expertOnly"] = item.expertOnly,
			["expert"] = item.expert,
			["isAShopItem"] = item.isAShopItem,
			["newAndShiny"] = item.newAndShiny,
			["hasVanityEffects"] = item.hasVanityEffects,
			["color"] = new[] { (int)item.color.R, (int)item.color.G, (int)item.color.B, (int)item.color.A }
		};
	}

	private static void WriteJson(string path, object value)
	{
		File.WriteAllText(path, JsonConvert.SerializeObject(value, Formatting.None));
	}
}
