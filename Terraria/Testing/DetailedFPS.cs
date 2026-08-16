using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria.GameContent;
using Terraria.Utilities;

namespace Terraria.Testing;

public static class DetailedFPS
{
	public enum OperationCategory
	{
		Idle,
		Update,
		Draw,
		Present,
		GC,
		End,
		Count
	}

	private struct Frame
	{
		public struct Event
		{
			public OperationCategory category;

			public long timestamp;

			public Event(OperationCategory category, long timestamp)
			{
				this.category = category;
				this.timestamp = timestamp;
			}
		}

		public List<Event> events;

		public int[] CollectionCount;

		public long Allocated;

		public void Init()
		{
			events = new List<Event>(16);
			CollectionCount = new int[GC.MaxGeneration + 1];
		}

		public void Start()
		{
			events.Clear();
			Begin(OperationCategory.Idle);
		}

		public void Begin(OperationCategory category)
		{
			if (events.Count >= 1000 || (events.Count > 0 && events.Last().category == category))
			{
				return;
			}
			long timestamp = Stopwatch.GetTimestamp();
			if (events.Count > 0)
			{
				Event obj = events.Last();
				if (obj.category == OperationCategory.Draw || obj.category == OperationCategory.Update)
				{
					TimeLogger.TotalDrawAndUpdate.Add((int)(timestamp - obj.timestamp));
				}
			}
			events.Add(new Event(category, timestamp));
			TimeSpan gCPauseTime = GetGCPauseTime();
			if (gCPauseTime > TimeSpan.Zero)
			{
				long num = Utils.TimeSpanToSWTicks(gCPauseTime);
				events.Insert(events.Count - 1, new Event(OperationCategory.GC, timestamp - num));
				TimeLogger.GCPause.Add((int)num);
			}
		}

		public void Finish()
		{
			Begin(OperationCategory.End);
			for (int i = 0; i <= GC.MaxGeneration; i++)
			{
				CollectionCount[i] = GetCollectionCount(i);
			}
			if (Main.CollectGen0EveryFrame)
			{
				CollectionCount[0]--;
			}
			Allocated = GetAllocatedBytes();
		}
	}

	public static readonly int FrameCount;

	private static Frame[] Frames;

	private static int oldest;

	private static int newest;

	private static TimeSpan LastGCPauseTime;

	private static int[] LastCollectionCount;

	private static long LastAllocatedBytes;

	private const int PixelsPerMs = 6;

	private const int FrameWidth = 2;

	private const int BoxHeight = 100;

	private static string[] _gcGenText;

	public static TimeSpan CurrentFrameTime
	{
		get
		{
			Frame frame = Frames[newest];
			return Utils.SWTicksToTimeSpan(frame.events.Last().timestamp - frame.events[0].timestamp);
		}
	}

	static DetailedFPS()
	{
		FrameCount = 300;
		Frames = new Frame[FrameCount];
		LastCollectionCount = new int[GC.MaxGeneration + 1];
		_gcGenText = new string[3] { "G0", "G1", "G2" };
		for (int i = 0; i < Frames.Length; i++)
		{
			Frames[i].Init();
		}
	}

	public static void StartNextFrame()
	{
		TimeLogger.StartNextFrame();
		Frames[newest].Finish();
		newest++;
		if (newest == Frames.Length)
		{
			newest = 0;
		}
		if (newest == oldest)
		{
			oldest++;
		}
		if (oldest == Frames.Length)
		{
			oldest = 0;
		}
		Frames[newest].Start();
	}

	public static void Begin(OperationCategory category)
	{
		Frames[newest].Begin(category);
	}

	public static void End()
	{
		TimeLogger.EndDrawFrame();
		Begin(OperationCategory.Idle);
	}

	public static float GetCPUUtilization(int numFrames)
	{
		long[] array = new long[6];
		int num = 0;
		foreach (Frame item in EnumerateFrames())
		{
			if (num++ == numFrames)
			{
				break;
			}
			if (item.events.Count < 2)
			{
				continue;
			}
			Frame.Event obj = item.events[0];
			foreach (Frame.Event @event in item.events)
			{
				array[(int)obj.category] += @event.timestamp - obj.timestamp;
				obj = @event;
			}
		}
		long num2 = array.Sum();
		return (float)((double)(array[2] + array[1]) / (double)num2);
	}

	public static bool VsyncAppearsActive()
	{
		long num = 0L;
		int num2 = 60;
		int num3 = 0;
		foreach (Frame item in EnumerateFrames())
		{
			if (num3++ == num2)
			{
				break;
			}
			if (item.events.Count < 2)
			{
				continue;
			}
			Frame.Event obj = item.events[0];
			foreach (Frame.Event @event in item.events)
			{
				if (obj.category == OperationCategory.Present)
				{
					num += @event.timestamp - obj.timestamp;
				}
				obj = @event;
			}
		}
		return Utils.SWTicksToTimeSpan(num / num2).TotalSeconds >= Main.TARGET_FRAME_TIME * 0.1;
	}

	private static TimeSpan GetGCPauseTime()
	{
		TimeSpan timeSpan = NewRuntimeMethods.GC_GetTotalPauseDuration();
		TimeSpan result = timeSpan - LastGCPauseTime;
		LastGCPauseTime = timeSpan;
		return result;
	}

	private static int GetCollectionCount(int gen)
	{
		int num = GC.CollectionCount(gen);
		int result = num - LastCollectionCount[gen];
		LastCollectionCount[gen] = num;
		return result;
	}

	private static int GetAllocatedBytes()
	{
		long num = NewRuntimeMethods.GC_GetTotalAllocatedBytes();
		long num2 = num - LastAllocatedBytes;
		LastAllocatedBytes = num;
		return (int)num2;
	}

	private static IEnumerable<Frame> EnumerateFrames()
	{
		int k = newest;
		while (k != oldest)
		{
			int num = k - 1;
			k = num;
			if (num < 0)
			{
				k = Frames.Length - 1;
			}
			yield return Frames[k];
		}
	}

	public static void Draw()
	{
		Rectangle r = new Rectangle((Main.screenWidth - Frames.Length * 2) / 2, Main.screenHeight - 100, Frames.Length * 2, 100);
		DrawFPSBox(r);
		int num = 0;
		long num2 = 0L;
		foreach (Frame item in EnumerateFrames())
		{
			num++;
			DrawFrame(r.Right - num * 2, item);
			num2 += item.Allocated;
		}
		if (num2 > 0)
		{
			long num3 = num2 / num;
			DynamicSpriteFontExtensionMethods.DrawString(Main.spriteBatch, FontAssets.MouseText.Value, $"Avg Alloc: {num3,5} bytes/frame", new Vector2((Main.screenWidth - Frames.Length * 2) / 2 - 240, Main.screenHeight - 24), Color.White);
		}
		if (Main.keyState.PressingAlt())
		{
			DynamicSpriteFontExtensionMethods.DrawString(Main.spriteBatch, FontAssets.MouseText.Value, $"Time Acc: {Main.UpdateTimeAccumulator * 1000.0,5:0.0} ms", new Vector2(Main.screenWidth - 200, Main.screenHeight - 24), Color.White);
		}
	}

	private static void DrawFPSBox(Rectangle r)
	{
		Color white = Color.White;
		DrawRect(new Rectangle(r.Left, r.Y, 2, r.Height), white);
		DrawRect(new Rectangle(r.Right, r.Y, 2, r.Height), white);
		DrawRect(new Rectangle(r.Left, r.Y, r.Width, 1), white);
		int num = 24;
		OperationCategory operationCategory = OperationCategory.Idle;
		while (operationCategory <= OperationCategory.GC)
		{
			if (operationCategory != OperationCategory.GC || !(LastGCPauseTime == TimeSpan.Zero))
			{
				DrawRect(new Rectangle(r.Right + 8, r.Bottom - num + 8, 8, 8), GetColor(operationCategory));
				DynamicSpriteFontExtensionMethods.DrawString(Main.spriteBatch, FontAssets.MouseText.Value, operationCategory.ToString(), new Vector2(r.Right + 20, r.Bottom - num), Color.White);
			}
			operationCategory++;
			num += 24;
		}
	}

	private static void DrawFrame(int x, Frame frame)
	{
		if (frame.events.Count < 2)
		{
			return;
		}
		int val = 0;
		Frame.Event obj = frame.events[0];
		long timestamp = obj.timestamp;
		for (int i = 1; i < frame.events.Count; i++)
		{
			Frame.Event obj2 = frame.events[i];
			int num = (int)(Utils.SWTicksToTimeSpan(obj.timestamp - timestamp).TotalMilliseconds * 6.0);
			int num2 = (int)(Utils.SWTicksToTimeSpan(obj2.timestamp - timestamp).TotalMilliseconds * 6.0);
			DrawRect(new Rectangle(x, Main.screenHeight - num2, 2, num2 - num), GetColor(obj.category));
			obj = obj2;
			val = num2;
		}
		val = Math.Max(val, 100);
		for (int j = 0; j <= GC.MaxGeneration; j++)
		{
			for (int k = 0; k < frame.CollectionCount[j]; k++)
			{
				DynamicSpriteFontExtensionMethods.DrawString(Main.spriteBatch, FontAssets.MouseText.Value, _gcGenText[j], new Vector2(x - 10, Main.screenHeight - val - 15), Color.White, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 0f, (Vector2[])null, (Color[])null);
				val += 10;
			}
		}
	}

	private static Color GetColor(OperationCategory category)
	{
		return category switch
		{
			OperationCategory.Update => Color.Orange, 
			OperationCategory.Draw => Color.Green, 
			OperationCategory.Present => Color.Magenta, 
			OperationCategory.Idle => Color.Gray, 
			OperationCategory.GC => Color.Blue, 
			_ => Color.Black, 
		};
	}

	private static void DrawRect(Rectangle r, Color c)
	{
		Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, r, c);
	}
}
