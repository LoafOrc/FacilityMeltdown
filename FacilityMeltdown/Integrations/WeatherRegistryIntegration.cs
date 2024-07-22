using System.Runtime.CompilerServices;
using FacilityMeltdown.Config;
using WeatherRegistry;

namespace FacilityMeltdown.Integrations;

class WeatherRegistryIntegration {
	public static bool Enabled { get; private set; } // If you want to check compatibility locally
	
	// This method will be called automatically if the compatible mod is loaded.
	[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
	static void Initialize() {
		Enabled = true;
	}

	[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
	internal static float GetWeatherMultiplier() {
		if (Enabled || !MeltdownPlugin.config.WeatherRegistryIntegration) return 1;
		return WeatherManager.GetCurrentWeather(RoundManager.Instance.currentLevel).ScrapValueMultiplier;
	}
}