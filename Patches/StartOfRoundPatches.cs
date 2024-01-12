using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace FacilityMeltdown.Patches {
    [HarmonyPatch(typeof(StartOfRound))]
    internal static class StartOfRoundPatches {
        [HarmonyPrefix, HarmonyPatch(nameof(StartOfRound.EndOfGame))]
        internal static void UnloadMeltdownHandler() {
            if(MeltdownHandler.Instance)
                GameObject.Destroy(MeltdownHandler.Instance.gameObject);
        }
    }
}
