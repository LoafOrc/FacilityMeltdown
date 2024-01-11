using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace FacilityMeltdown.Patches {
    [HarmonyPatch(typeof(RoundManager))]
    internal static class RoundManagerPatch {
        [HarmonyPrefix, HarmonyPatch(nameof(RoundManager.UnloadSceneObjectsEarly))]
        internal static void UnloadMeltdownHandler() {
            if(MeltdownHandler.Instance)
                GameObject.Destroy(MeltdownHandler.Instance.gameObject);
        }
    }
}
