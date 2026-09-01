using Microsoft.Xna.Framework;

namespace Terraria.GameContent.Drawing;

public struct DrawBlackHelper(uint layer, Vector2 drawOffset)
{
	private readonly uint layer = layer;

	private readonly Vector2 drawOffset = drawOffset;

	private int y = 0;

	private int startX = 0;

	private int endX = 0;

	public void DrawBlack(int x, int y)
	{
		if (y == this.y && x == endX)
		{
			endX++;
			return;
		}
		EndStrip();
		this.y = y;
		startX = x;
		endX = x + 1;
	}

	public void EndStrip()
	{
		if (startX != endX)
		{
			Vector2 vector = new Vector2(startX << 4, y << 4) - Main.screenPosition + drawOffset;
			Main.tileBatch.SetLayer(layer, 0);
			Main.tileBatch.Draw(TextureAssets.BlackTile.Value, new Vector4(vector.X, vector.Y, endX - startX << 4, 16f), Color.Black);
		}
	}
}
