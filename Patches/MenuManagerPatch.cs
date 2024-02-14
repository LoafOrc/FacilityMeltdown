using HarmonyLib;

namespace FacilityMeltdown.Patches {
    [HarmonyPatch(typeof(MenuManager))]
    internal static class MenuManagerPatch {
        [HarmonyPostfix, HarmonyPatch(nameof(MenuManager.Start))]
        internal static void MeltdownChanceNotification(MenuManager __instance) {
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("den.meltdownchance")) {
                __instance.DisplayMenuNotification(
                    "You are using MeltdownChance. This goes AGAINST my best wishes as the meltdown dev. For more information check the log",
                    "[Continue]"
                );
            }
        }
    }
}
