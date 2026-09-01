using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics;

namespace Terraria.DataStructures;

public struct SpriteBatchBeginner(SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState, Effect effect, Matrix transformMatrix)
{
	private SpriteSortMode sortMode = sortMode;

	private BlendState blendState = blendState;

	private SamplerState samplerState = samplerState;

	private DepthStencilState depthStencilState = depthStencilState;

	private RasterizerState rasterizerState = rasterizerState;

	private Effect effect = effect;

	public Matrix transformMatrix = transformMatrix;

	public void Begin(SpriteBatch spriteBatch)
	{
		spriteBatch.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, transformMatrix);
	}

	public void Begin(SpriteBatch spriteBatch, SpriteSortMode sortMode)
	{
		spriteBatch.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, transformMatrix);
	}

	public void Begin(TileBatch tileBatch)
	{
		tileBatch.Begin(rasterizerState, transformMatrix);
	}
}
