using Microsoft.Xna.Framework;
using ReLogic.Peripherals.RGB;
using Terraria.IO;

namespace Terraria.Initializers;

public static class ChromaInitializer
{
	private static ChromaEngine _engine;

	private const string GAME_NAME_ID = "TERRARIA";

	private static float _rgbUpdateRate = 45f;

	private static bool _useRazer = true;

	private static bool _useCorsair = true;

	private static bool _useLogitech = true;

	private static bool _useSteelSeries = true;

	private static VendorColorProfile _razerColorProfile = new VendorColorProfile(new Vector3(1f, 0.765f, 0.568f));

	private static VendorColorProfile _corsairColorProfile = new VendorColorProfile();

	private static VendorColorProfile _logitechColorProfile = new VendorColorProfile();

	private static VendorColorProfile _steelSeriesColorProfile = new VendorColorProfile();

	public static void BindTo(Preferences preferences)
	{
		preferences.OnSave += Configuration_OnSave;
		preferences.OnLoad += Configuration_OnLoad;
	}

	private static void Configuration_OnLoad(Preferences obj)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected O, but got Unknown
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Expected O, but got Unknown
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Expected O, but got Unknown
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Expected O, but got Unknown
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Expected O, but got Unknown
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Expected O, but got Unknown
		_useRazer = obj.Get("UseRazerRGB", defaultValue: true);
		_useCorsair = obj.Get("UseCorsairRGB", defaultValue: true);
		_useLogitech = obj.Get("UseLogitechRGB", defaultValue: true);
		_useSteelSeries = obj.Get("UseSteelSeriesRGB", defaultValue: true);
		_razerColorProfile = obj.Get<VendorColorProfile>("RazerColorProfile", new VendorColorProfile(new Vector3(1f, 0.765f, 0.568f)));
		_corsairColorProfile = obj.Get<VendorColorProfile>("CorsairColorProfile", new VendorColorProfile());
		_logitechColorProfile = obj.Get<VendorColorProfile>("LogitechColorProfile", new VendorColorProfile());
		_steelSeriesColorProfile = obj.Get<VendorColorProfile>("SteelSeriesColorProfile", new VendorColorProfile());
		if (_razerColorProfile == null)
		{
			_razerColorProfile = new VendorColorProfile(new Vector3(1f, 0.765f, 0.568f));
		}
		if (_corsairColorProfile == null)
		{
			_corsairColorProfile = new VendorColorProfile();
		}
		if (_logitechColorProfile == null)
		{
			_logitechColorProfile = new VendorColorProfile();
		}
		if (_steelSeriesColorProfile == null)
		{
			_steelSeriesColorProfile = new VendorColorProfile();
		}
		_rgbUpdateRate = obj.Get("RGBUpdatesPerSecond", 45f);
		if (_rgbUpdateRate <= 1E-07f)
		{
			_rgbUpdateRate = 45f;
		}
	}

	private static void Configuration_OnSave(Preferences preferences)
	{
		preferences.Put("RGBUpdatesPerSecond", _rgbUpdateRate);
		preferences.Put("UseRazerRGB", _useRazer);
		preferences.Put("RazerColorProfile", _razerColorProfile);
		preferences.Put("UseCorsairRGB", _useCorsair);
		preferences.Put("CorsairColorProfile", _corsairColorProfile);
		preferences.Put("UseLogitechRGB", _useLogitech);
		preferences.Put("LogitechColorProfile", _logitechColorProfile);
		preferences.Put("UseSteelSeriesRGB", _useSteelSeries);
		preferences.Put("SteelSeriesColorProfile", _steelSeriesColorProfile);
	}

	public static void DisableAllDeviceGroups()
	{
		if (_engine != null)
		{
			_engine.DisableAllDeviceGroups();
		}
	}

	public static void Load()
	{
		_engine = Main.Chroma;
	}

	public static void UpdateEvents()
	{
	}
}
