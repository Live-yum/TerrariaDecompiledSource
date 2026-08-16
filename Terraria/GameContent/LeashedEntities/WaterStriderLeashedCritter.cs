using Microsoft.Xna.Framework;

namespace Terraria.GameContent.LeashedEntities;

internal class WaterStriderLeashedCritter : JumperLeashedCritter
{
	public new static WaterStriderLeashedCritter Prototype = new WaterStriderLeashedCritter();

	public WaterStriderLeashedCritter()
	{
		minWaitTime = 60;
		maxWaitTime = 120;
		strayingRangeInBlocks = 5;
		maxJumpWidth = 32f;
		minJumpWidth = 8f;
		maxJumpHeight = 0f;
		maxJumpDuration = 14f;
		jumpCooldown = 15;
		canStandOnWater = true;
	}

	public override Vector2 GetDrawOffset()
	{
		Vector2 drawOffset = base.GetDrawOffset();
		Point pt = base.Center.ToTileCoordinates();
		for (int i = 0; i < 2; i++)
		{
			pt.Y++;
			byte liquid = Framing.GetTileSafely(pt).liquid;
			if (liquid != 0)
			{
				drawOffset.Y = (255 - liquid) / 16;
				break;
			}
		}
		return drawOffset;
	}
}
