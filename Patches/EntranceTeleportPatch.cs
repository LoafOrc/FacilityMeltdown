using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

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
