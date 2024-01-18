using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FacilityMeltdown.Behaviours;
using FacilityMeltdown.Util;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FacilityMeltdown.Patches {
    [HarmonyPatch(typeof(LungProp))]
    internal class ApparaticePatch {
        [HarmonyPrefix, HarmonyPatch(nameof(LungProp.EquipItem))]
        internal static void BeginMeltdownSequence(LungProp __instance, ref bool ___isLungDocked) {
            if (!__instance.IsHost) return;
            if (!___isLungDocked) return;

            // We just took it out
            try { // make sure to surround in try catch because this is a prefix
                if (MeltdownConfig.Instance.OVERRIDE_APPARATUS_VALUE)
                    __instance.scrapValue = MeltdownConfig.Instance.APPARATUS_VALUE;
                GameObject meltdown = GameObject.Instantiate(Assets.meltdownHandlerPrefab);
                meltdown.GetComponent<NetworkObject>().Spawn();
            } catch (Exception ex) { 
                MeltdownPlugin.logger.LogError(ex);
            }
        }

        [HarmonyPrefix, HarmonyPatch(nameof(LungProp.Start))]
        internal static void AddRadiationSource(LungProp __instance) {
            RadiationSource source = __instance.gameObject.AddComponent<RadiationSource>();
            source.radiationAmount = 40;
            source.radiationDistance = 40;

            if (MeltdownConfig.Instance.OVERRIDE_APPARATUS_VALUE)
                __instance.scrapValue = MeltdownConfig.Instance.APPARATUS_VALUE;
            //___isLungDocked = false; // fix joining late
        }
    }
}
