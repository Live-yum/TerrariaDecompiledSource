using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Terraria.Testing;

public class DebugLineDraw
{
	private enum UpdatePhase
	{
		Update,
		UpdateInWorld,
		Draw
	}

	private class LineDrawer
	{
		public Vector2 vS;

		public Vector2 vE;

		public Color cS;

		public Color cE;

		public int TimeLeft;

		public float Width;

		public UpdatePhase Phase = CurrentPhase;

		public LineDrawer(Vector2 start, Vector2 end, Color colorStart, Color colorEnd = default(Color), int LifeTime = 1, float width = 1f)
		{
			vS = start;
			vE = end;
			cS = colorStart;
			cE = ((colorEnd == default(Color)) ? colorStart : colorEnd);
			TimeLeft = LifeTime;
			Width = width;
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			Utils.DrawLine(spriteBatch, vS, vE, cS, cE, Width);
		}
	}

	public static readonly DebugLineDraw UI = new DebugLineDraw(ui: true);

	public static readonly DebugLineDraw World = new DebugLineDraw(ui: false);

	private static UpdatePhase CurrentPhase;

	private readonly List<LineDrawer> lines = new List<LineDrawer>();

	private readonly bool _ui;

	private DebugLineDraw(bool ui)
	{
		_ui = ui;
	}

	public void AddLine(Vector2 start, Vector2 end, Color colorStart, Color colorEnd = default(Color), int LifeTime = 1, float width = 1f)
	{
		lines.Add(new LineDrawer(start, end, colorStart, colorEnd, LifeTime, width));
	}

	public void AddLine(Point start, Point end, Color colorStart, Color colorEnd = default(Color), int LifeTime = 1, float width = 1f)
	{
		lines.Add(new LineDrawer(start.ToVector2(), end.ToVector2(), colorStart, colorEnd, LifeTime, width));
	}

	public void AddRectangle(Vector2 start, Vector2 end, Color colorStart, Color colorEnd = default(Color), int LifeTime = 1, float width = 1f)
	{
		lines.Add(new LineDrawer(start, new Vector2(start.X, end.Y), colorStart, colorEnd, LifeTime, width));
		lines.Add(new LineDrawer(start, new Vector2(end.X, start.Y), colorStart, colorEnd, LifeTime, width));
		lines.Add(new LineDrawer(end, new Vector2(start.X, end.Y), colorStart, colorEnd, LifeTime, width));
		lines.Add(new LineDrawer(end, new Vector2(end.X, start.Y), colorStart, colorEnd, LifeTime, width));
	}

	public static void PreUpdate()
	{
		SetPhase(UpdatePhase.Update);
	}

	public static void PreWorldUpdate()
	{
		SetPhase(UpdatePhase.UpdateInWorld);
	}

	public static void PreDraw()
	{
		SetPhase(UpdatePhase.Draw);
	}

	private static void SetPhase(UpdatePhase phase)
	{
		CurrentPhase = phase;
		UI.TickLines();
		World.TickLines();
	}

	public void TickLines()
	{
		int num = 0;
		for (int i = 0; i < lines.Count; i++)
		{
			LineDrawer lineDrawer = lines[i];
			if (lineDrawer.Phase == CurrentPhase)
			{
				lineDrawer.TimeLeft--;
			}
			if (lineDrawer.TimeLeft >= 0)
			{
				lines[num++] = lineDrawer;
			}
		}
		lines.RemoveRange(num, lines.Count - num);
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		if (lines.Count != 0)
		{
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, null, null, null, _ui ? (Matrix.CreateTranslation(Main.screenPosition.X, Main.screenPosition.Y, 0f) * Main.UIScaleMatrix) : Main.GameViewMatrix.TransformationMatrix);
			for (int i = 0; i < lines.Count; i++)
			{
				lines[i].Draw(spriteBatch);
			}
			spriteBatch.End();
		}
	}
}
