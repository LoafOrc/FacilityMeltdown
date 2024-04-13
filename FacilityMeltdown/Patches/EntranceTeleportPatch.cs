using FacilityMeltdown.MeltdownSequence.Behaviours;
using HarmonyLib;

namespace FacilityMeltdown.Patches {
    [HarmonyPatch(typeof(EntranceTeleport))]
    internal static class EntranceTeleportPatch {
        [HarmonyPrefix, HarmonyPatch(nameof(EntranceTeleport.FindExitPoint))]
        static bool dontAllowReneter(ref bool __result) {
            if (MeltdownHandler.Instance != null && MeltdownHandler.Instance.HasExplosionOccured()) {
                __result = false;
                return false;
            }

            return true;
        }
    }
}
