using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FacilityMeltdown.Behaviours;
using HarmonyLib;

namespace FacilityMeltdown.Patches {
    [HarmonyPatch(typeof(BlobAI))]
    internal class BlobAIPatch {
        [HarmonyPrefix, HarmonyPatch(nameof(BlobAI.Start))]
        internal static void AddRadiationSource(BlobAI __instance) {
            RadiationSource source = __instance.gameObject.AddComponent<RadiationSource>();
            source.radiationAmount = 10;
            source.radiationDistance = 15;
        }
    }
}
