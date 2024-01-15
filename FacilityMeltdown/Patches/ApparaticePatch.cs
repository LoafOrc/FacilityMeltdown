using System;
using FacilityMeltdown.Util;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace FacilityMeltdown.Patches;

[HarmonyPatch(typeof(LungProp))]
internal class ApparaticePatch {
    [HarmonyPrefix, HarmonyPatch(nameof(LungProp.EquipItem))]
    internal static void BeginMeltdownSequence(LungProp __instance, ref bool ___isLungDocked) {
        MeltdownPlugin.logger.LogInfo("begin seqeuence 1");
        if (!__instance.IsHost) return;
        MeltdownPlugin.logger.LogInfo("begin seqeuence 2");
        if (!___isLungDocked) return;
        MeltdownPlugin.logger.LogInfo("begin seqeuence 3");

        // We just took it out
        try { // make sure to surround in try catch because this is a prefix
            if (MeltdownConfig.Instance.OVERRIDE_APPARATUS_VALUE)
                __instance.scrapValue = MeltdownConfig.Instance.APPARATUS_VALUE;
            GameObject meltdown = GameObject.Instantiate(Assets.meltdownHandlerPrefab);
            MeltdownPlugin.logger.LogInfo("begin seqeuence 4");
            meltdown.GetComponent<NetworkObject>().Spawn();
            MeltdownPlugin.logger.LogInfo("begin seqeuence 5");
        } catch (Exception ex) { 
            MeltdownPlugin.logger.LogError(ex);
        }
    }

    [HarmonyPrefix, HarmonyPatch(nameof(LungProp.Start))]
    internal static void FixLateCompany(LungProp __instance, ref bool ___isLungDocked) {
        if (!__instance.isInElevator) return;

        // it is inside of the ship

        if (MeltdownConfig.Instance.OVERRIDE_APPARATUS_VALUE)
            __instance.scrapValue = MeltdownConfig.Instance.APPARATUS_VALUE;
        //___isLungDocked = false; // fix joining late
    }
}