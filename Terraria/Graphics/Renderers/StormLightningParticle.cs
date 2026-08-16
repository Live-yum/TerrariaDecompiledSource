using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.Utilities;

namespace Terraria.Graphics.Renderers;

public class StormLightningParticle : IPooledParticle, IParticle
{
	public Color Color;

	public Vector2 EndPosition;

	public Vector2 StartPosition;

	private List<LightningGenerator.Bolt> bolts = new List<LightningGenerator.Bolt>();

	private int _lifeTimeCounted;

	private int _lifeTimeTotal;

	public bool ShouldBeRemovedFromRenderer { get; private set; }

	public bool IsRestingInPool { get; private set; }

	public void RestInPool()
	{
		IsRestingInPool = true;
	}

	public virtual void FetchFromPool()
	{
		_lifeTimeCounted = 0;
		_lifeTimeTotal = 0;
		IsRestingInPool = false;
		ShouldBeRemovedFromRenderer = false;
		bolts.Clear();
	}

	public void Prepare(uint seed, Vector2 targetPosition, int lifeTimeTotal, Color color)
	{
		Color = color;
		_lifeTimeTotal = lifeTimeTotal;
		LightningGenerator.Bolt bolt = LightningGenerator.StormLightning.Generate(bolts, seed, targetPosition);
		StartPosition = bolt.positions[0];
		EndPosition = bolt.positions[bolt.positions.Length - 1];
		LCG32Random lCG32Random = new LCG32Random(seed);
		int maxValue = (int)Math.Ceiling((float)bolt.positions.Length / 10f);
		for (int i = 0; i < bolt.positions.Length; i++)
		{
			if (lCG32Random.Next(maxValue) == 0)
			{
				Vector2 position = bolt.positions[i];
				Vector2 velocity = Vector2.UnitY;
				if (bolt.rotations != null)
				{
					velocity = -bolt.rotations[i].ToRotationVector2();
				}
				Dust dust = Dust.NewDustPerfect(position, 226);
				dust.HackFrame(278);
				dust.color = color;
				dust.customData = dust.color;
				dust.velocity = velocity;
				dust.velocity *= 3f + lCG32Random.NextFloat() * 6.5f;
				dust.fadeIn = 0f;
				dust.scale = 0.4f + lCG32Random.NextFloat() * 0.5f;
				dust.noGravity = true;
				dust.position -= dust.velocity * 6f;
				Dust.CloneDust(dust).velocity *= 0.5f;
				dust.scale -= 0.3f;
			}
		}
	}

	public void Update(ref ParticleRendererSettings settings)
	{
		Color color = new Color(80, 220, 220);
		float num = (float)_lifeTimeCounted / (float)_lifeTimeTotal;
		float num2 = Utils.Remap(num, 0f, 0.4f, 1f, 0f);
		if (num < 0.3f)
		{
			ParticleOrchestrator.RequestParticleSpawn(clientOnly: true, ParticleOrchestraType.StormlightningWindup, new ParticleOrchestraSettings
			{
				PositionInWorld = StartPosition,
				MovementVector = Vector2.Zero,
				UniqueInfoPiece = (int)color.PackedValue
			});
		}
		for (int i = 0; i < 3; i++)
		{
			if (Main.rand.Next(4) == 0 && !(Main.rand.NextFloat() > num2 * 0.13f))
			{
				Dust dust = Dust.NewDustDirect(StartPosition, 16, 16, 306, 0f, 0f, 0, new Color(color.R, color.G, color.B, 0));
				dust.velocity = new Vector2(0f, -4f).RotatedByRandom(1.5707963705062866) * (0.5f + 0.2f * Main.rand.NextFloatDirection());
				dust.scale = 1.8f;
				dust.fadeIn = 0f;
				dust.noGravity = Main.rand.Next(3) != 0;
				dust.noLight = (dust.noLightEmittence = true);
				Dust dust2 = Dust.CloneDust(dust);
				dust2.color = new Color(255, 255, 255, 0);
				dust2.scale = 1.3f;
			}
		}
		for (int j = -1; j <= 1; j += 2)
		{
			if (Main.rand.Next(4) == 0 && !(Main.rand.NextFloat() > num2 * 0.2f))
			{
				Dust dust3 = Dust.NewDustPerfect(StartPosition, 306, new Vector2(0f, -4f).RotatedBy((float)Math.PI / 4f * (float)j * 1f));
				dust3.color = new Color(color.R, color.G, color.B, 0);
				dust3.scale = 1.8f;
				dust3.fadeIn = 0f;
				dust3.noGravity = Main.rand.Next(3) != 0;
				dust3.noLight = (dust3.noLightEmittence = true);
				Dust dust4 = Dust.CloneDust(dust3);
				dust4.color = new Color(255, 255, 255, 0);
				dust4.scale = 1.3f;
			}
		}
		for (int k = 0; k < 2; k++)
		{
			if (Main.rand.Next(4) == 0 && !(Main.rand.NextFloat() > 0.2f))
			{
				Dust dust5 = Dust.NewDustPerfect(StartPosition, 226);
				dust5.HackFrame(278);
				dust5.color = color;
				dust5.customData = dust5.color;
				dust5.velocity *= 1f + Main.rand.NextFloat() * 2.5f;
				dust5.velocity += new Vector2(0f, -2f);
				dust5.fadeIn = 0f;
				dust5.scale = 0.4f + Main.rand.NextFloat() * 0.5f;
				dust5.velocity.X *= 2f;
				dust5.velocity = Main.rand.NextVector2Circular(3f, 2f) + new Vector2(0f, -2f);
				dust5.noLight = (dust5.noLightEmittence = true);
				dust5.position -= dust5.velocity * 3f;
			}
		}
		if (++_lifeTimeCounted >= _lifeTimeTotal)
		{
			ShouldBeRemovedFromRenderer = true;
		}
	}

	public void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch)
	{
		StormLightningDrawer stormLightningDrawer = default(StormLightningDrawer);
		foreach (LightningGenerator.Bolt bolt in bolts)
		{
			float intensity = (bolt.IsMainBolt ? 1f : (0.5f * (float)Math.Pow(0.8, bolt.forkDepth - 1)));
			stormLightningDrawer.Draw(bolt.positions, bolt.rotations, 16f, Color, (float)_lifeTimeCounted / (float)_lifeTimeTotal, bolt.IsMainBolt, bolt.progressRange, intensity);
		}
	}
}
