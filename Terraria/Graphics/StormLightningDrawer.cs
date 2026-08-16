using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.Utilities.Terraria.Utilities;

namespace Terraria.Graphics;

public struct StormLightningDrawer
{
	private static VertexStrip _vertexStrip = new VertexStrip();

	private float _width;

	private Color _color;

	private bool _isMainBolt;

	private float _progress;

	private FloatRange _progressRange;

	private bool _taperEnd;

	public void Draw(Vector2[] positions, float[] rotations, float width, Color color, float progress, bool isMainBolt, FloatRange progressRange, float intensity)
	{
		_width = width;
		_color = color;
		_isMainBolt = isMainBolt;
		_progress = progress;
		_progressRange = progressRange;
		_taperEnd = _progressRange.Maximum < 1f;
		MiscShaderData miscShaderData = GameShaders.Misc["StormLightning"];
		miscShaderData.UseSaturation(intensity);
		miscShaderData.UseOpacity(Utils.Remap(_progress, 0.1f, 0.25f, 0.5f, 1f) * Utils.Remap(_progress, 0.25f, 0.75f, 1f, 0f));
		miscShaderData.Apply();
		_vertexStrip.PrepareStrip(positions, rotations, StripColors, StripWidth, -Main.screenPosition);
		_vertexStrip.DrawTrail();
		Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
		Main.pixelShader.CurrentTechnique.Passes[0].Apply();
	}

	private static float WaveTransition(float progressOnStrip, float waveProgress, float transitionLength, float from, float to)
	{
		return Utils.Remap(progressOnStrip, MathHelper.Lerp(0f - transitionLength, 1f, waveProgress), MathHelper.Lerp(0f, 1f + transitionLength, waveProgress), to, from);
	}

	private Color StripColors(float progressOnStrip)
	{
		progressOnStrip = _progressRange.Lerp(progressOnStrip);
		float waveProgress = Utils.Remap(_progress, 0f, 0.15f, 0f, 1f);
		float num = WaveTransition(progressOnStrip, waveProgress, 0.5f, 0f, 1f);
		float waveProgress2 = Utils.Remap(_progress, 0.25f, 1f, 0f, 1f);
		float num2 = WaveTransition(progressOnStrip, waveProgress2, 0.5f, 1f, 0f);
		float num3 = num * num2;
		return _color * num3;
	}

	private float StripWidth(float progressOnStrip)
	{
		progressOnStrip = _progressRange.Lerp(progressOnStrip);
		float width = _width;
		width *= Utils.Remap(progressOnStrip, 0.5f, 1f, 1f, 0.5f);
		width *= Utils.Remap(_progress, 0.5f, 1f, 1f, 0.5f);
		if (_taperEnd)
		{
			width *= Utils.Remap(_progressRange.Maximum - progressOnStrip, 0.1f, 0f, 1f, _isMainBolt ? 0.5f : 0f);
		}
		return width;
	}
}
