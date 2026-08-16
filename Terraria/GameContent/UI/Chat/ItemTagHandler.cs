using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Terraria.GameContent.UI.Chat;

public class ItemTagHandler : ITagHandler
{
	private class ItemSnippet : TextSnippet
	{
		private Item _item;

		public ItemSnippet(Item item)
		{
			_item = item;
			Color = ItemRarity.GetColor(item.rare);
		}

		public override void OnHover()
		{
			Main.HoverItem = _item.Clone();
			Main.instance.MouseText(_item.Name, _item.rare, 0);
		}

		public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default(Vector2), Color color = default(Color), float scale = 1f)
		{
			if (Main.netMode != 2 && !Main.dedServ)
			{
				Main.instance.LoadItem(_item.type);
			}
			scale *= 0.75f;
			if (!justCheckingString && color != Color.Black)
			{
				float inventoryScale = Main.inventoryScale;
				Main.inventoryScale = scale;
				ItemSlot.Draw(spriteBatch, ref _item, 14, position - new Vector2(10f) * Main.inventoryScale, Color.White);
				Main.inventoryScale = inventoryScale;
			}
			size = new Vector2(32f) * scale;
			return true;
		}
	}

	TextSnippet ITagHandler.Parse(string text, Color baseColor, string options)
	{
		Item item = new Item();
		if (int.TryParse(text, out var result))
		{
			item.SetDefaults(result);
		}
		if (item.type <= 0)
		{
			return new TextSnippet(text);
		}
		item.stack = 1;
		if (options != null)
		{
			string[] array = options.Split(new char[1] { ',' });
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].Length == 0)
				{
					continue;
				}
				switch (array[i][0])
				{
				case 's':
				case 'x':
				{
					if (int.TryParse(array[i].Substring(1), out var result3))
					{
						item.stack = Utils.Clamp(result3, 1, item.maxStack);
					}
					break;
				}
				case 'p':
				{
					if (int.TryParse(array[i].Substring(1), out var result2))
					{
						item.Prefix((byte)Utils.Clamp(result2, 0, PrefixID.Count));
					}
					break;
				}
				}
			}
		}
		string text2 = "";
		if (item.stack > 1)
		{
			text2 = " (" + item.stack + ")";
		}
		return new ItemSnippet(item)
		{
			Text = "[" + item.AffixName() + text2 + "]",
			CheckForHover = true,
			DeleteWhole = true
		};
	}

	public static string GenerateTag(Item I)
	{
		string text = "[i";
		if (I.prefix != 0)
		{
			text = text + "/p" + I.prefix;
		}
		if (I.stack != 1)
		{
			text = text + "/s" + I.stack;
		}
		return text + ":" + I.type + "]";
	}
}
