using Microsoft.Xna.Framework;

namespace Terraria.GameContent.Items;

public class WhipTagEffect_Firecracker : WhipTagEffect
{
	private static float ProcDamageMultiplier = 1.75f;

	public override void ModifyProcHit(Player owner, Projectile optionalProjectile, NPC npcHit, ref int damageDealt, ref bool crit)
	{
		base.ModifyProcHit(owner, optionalProjectile, npcHit, ref damageDealt, ref crit);
		damageDealt += (int)((float)damageDealt * ProcDamageMultiplier);
	}

	public override void OnProcHit(Player owner, Projectile optionalProjectile, NPC npcHit, int calcDamage)
	{
		CreateExplosion(optionalProjectile, npcHit, (int)((float)calcDamage * ProcDamageMultiplier));
	}

	private static void CreateExplosion(Projectile projectile, NPC npcHit, int procDamage)
	{
		int num = Projectile.NewProjectile(projectile.GetProjectileSource_FromThis(), npcHit.Center, Vector2.Zero, 918, procDamage, 0f, projectile.owner);
		Main.projectile[num].localNPCImmunity[npcHit.whoAmI] = -1;
	}
}
