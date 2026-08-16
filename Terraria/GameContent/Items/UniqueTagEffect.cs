namespace Terraria.GameContent.Items;

public abstract class UniqueTagEffect
{
	public bool NetSync;

	public bool SyncProcs;

	public int TagDuration;

	public virtual bool CanApplyTagToNPC(int npcType)
	{
		return true;
	}

	public virtual void OnRemovedFromPlayer(Player owner)
	{
	}

	public virtual void OnSetToPlayer(Player owner)
	{
	}

	public virtual void OnTagAppliedToNPC(Player owner, NPC npc)
	{
	}

	public virtual bool CanRunHitEffects(Player owner, Projectile optionalProjectile, NPC npcHit)
	{
		return true;
	}

	public virtual void ModifyTaggedHit(Player owner, Projectile optionalProjectile, NPC npcHit, ref int damageDealt, ref bool crit)
	{
	}

	public virtual void ModifyProcHit(Player owner, Projectile optionalProjectile, NPC npcHit, ref int damageDealt, ref bool crit)
	{
	}

	public virtual void OnTaggedHit(Player owner, Projectile optionalProjectile, NPC npcHit, int calcDamage)
	{
	}

	public virtual void OnProcHit(Player owner, Projectile optionalProjectile, NPC npcHit, int calcDamage)
	{
	}
}
