namespace Terraria.GameContent.Items;

public class WhipTagEffect_Possession : WhipTagEffect
{
	public override void OnProcHit(Player owner, Projectile optionalProjectile, NPC npcHit, int calcDamage)
	{
		Projectile.SpawnMoonLordWhipProc(optionalProjectile, npcHit, 20, 0);
	}
}
