using Microsoft.Xna.Framework;

namespace Terraria.UI;

public struct CalculatedStyle(float x, float y, float width, float height)
{
	public float X = x;

	public float Y = y;

	public float Width = width;

	public float Height = height;

	public Rectangle ToRectangle()
	{
		return new Rectangle((int)X, (int)Y, (int)Width, (int)Height);
	}

	public Vector2 Position()
	{
		return new Vector2(X, Y);
	}

	public Vector2 Center()
	{
		return new Vector2(X + Width * 0.5f, Y + Height * 0.5f);
	}
}
