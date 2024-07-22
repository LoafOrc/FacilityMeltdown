using System;
using System.Runtime.CompilerServices;
using LobbyCompatibility;
using LobbyCompatibility.Enums;
using LobbyCompatibility.Features;

namespace FacilityMeltdown.Integrations;

class LobbyCompatibilityIntegration {
	public static bool Enabled { get; private set; } // If you want to check compatibility locally

	// This method will be called automatically if the compatible mod is loaded.
	[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
	static void Initialize() {
		Enabled = true;
		
		PluginHelper.RegisterPlugin(MeltdownPlugin.modGUID, Version.Parse(MeltdownPlugin.modVersion), CompatibilityLevel.Everyone, VersionStrictness.Patch);
	}
}