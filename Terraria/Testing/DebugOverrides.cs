using System.Collections.Generic;
using System.Diagnostics;

namespace Terraria.Testing;

public class DebugOverrides
{
	public static Dictionary<string, double> Overrides = new Dictionary<string, double>();

	[Conditional("DEBUG")]
	public static void Replace(string key, ref int value)
	{
		if (Overrides.TryGetValue(key, out var value2))
		{
			value = (int)value2;
		}
	}

	[Conditional("DEBUG")]
	public static void Replace(string key, ref float value)
	{
		if (Overrides.TryGetValue(key, out var value2))
		{
			value = (float)value2;
		}
	}

	[Conditional("DEBUG")]
	public static void Set(string key, double value)
	{
		Overrides[key] = value;
	}
}
