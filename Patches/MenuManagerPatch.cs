using HarmonyLib;

namespace FacilityMeltdown.Patches {
    [HarmonyPatch(typeof(MenuManager))]
    internal static class MenuManagerPatch {
        [HarmonyPostfix, HarmonyPatch(nameof(MenuManager.Start))]
        internal static void MeltdownChanceNotification(MenuManager __instance) {
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("den.meltdownchance")) {
                __instance.DisplayMenuNotification(
                    "MeltdownChance can cause instability with FacilityMeltdown, read the log for more info.",
                    "[Continue]"
                );
            }
        }
    }
}
