using Microsoft.Xna.Framework;

namespace Terraria.DataStructures;

public struct ItemSyncPersistentStats
{
	private Color color;

	private int type;

	public void CopyFrom(WorldItem item)
	{
		type = item.type;
		color = item.color;
	}

	public void PasteInto(WorldItem item)
	{
		if (type == item.type)
		{
			item.color = color;
		}
	}
}
