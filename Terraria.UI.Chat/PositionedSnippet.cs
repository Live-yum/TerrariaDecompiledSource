using Microsoft.Xna.Framework;

namespace Terraria.UI.Chat;

public struct PositionedSnippet(TextSnippet snippet, int origIndex, int line, Vector2 position, Vector2 size)
{
	public readonly TextSnippet Snippet = snippet;

	public readonly int OrigIndex = origIndex;

	public readonly int Line = line;

	public Vector2 Position = position;

	public Vector2 Size = size;

	public void Scale(float scale)
	{
		Position *= scale;
		Size *= scale;
	}
}
