using System;
using Microsoft.Xna.Framework;

namespace Terraria.GameContent.Items;

public class WhipTagEffect_ViolentDisplayOfFlower : WhipTagEffect
{
	public override void OnProcHit(Player owner, Projectile optionalProjectile, NPC npcHit, int calcDamage)
	{
		SpawnFlowerExplosionOn(optionalProjectile, npcHit, 40);
	}

	private void SpawnFlowerExplosionOn(Projectile projectile, NPC targetNPC, int petalDamage)
	{
		float num = Main.rand.NextFloat() * ((float)Math.PI * 2f);
		float num2 = 3f;
		for (int i = 0; (float)i < num2; i++)
		{
			float num3 = (float)i / num2 * ((float)Math.PI * 2f) + num;
			float num4 = (float)((targetNPC.width > targetNPC.height) ? targetNPC.width : targetNPC.height) / 8f;
			Vector2 velocity = Vector2.UnitX.RotatedBy(num3).RotatedByRandom(0.39269909262657166) * num4;
			int num5 = Projectile.NewProjectile(projectile.GetProjectileSource_FromThis(), targetNPC.Center, velocity, 1038, petalDamage, 0f, projectile.owner, Main.rand.NextFloat() * -20f);
			Main.projectile[num5].localNPCImmunity[targetNPC.whoAmI] = 30;
		}
	}
}
