using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;   
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
            if (!___isLungDocked) return;

            // We just took it out
            try { // make sure to surround in try catch because this is a prefix
                if(MeltdownConfig.Instance.OVERRIDE_APPARATUS_VALUE)
                    __instance.scrapValue = MeltdownConfig.Instance.APPARATUS_VALUE;
                GameObject meltdown = new GameObject();

                SceneManager.MoveGameObjectToScene(meltdown, SceneManager.GetSceneByName(RoundManager.Instance.currentLevel.sceneName));
                meltdown.name = "MeltdownHandler";

                MeltdownHandler handler = meltdown.AddComponent<MeltdownHandler>();
                handler.causedBy = (int) __instance.playerHeldBy.playerClientId;
            } catch(Exception ex) { 
                MeltdownPlugin.logger.LogError(ex);
            }
        }

        [HarmonyPrefix, HarmonyPatch(nameof(LungProp.Start))]
        internal static void FixLateCompany(LungProp __instance, ref bool ___isLungDocked) {
            if (!___isLungDocked) return;

            // It is docked
            if (!__instance.isInElevator) return;
            // it is inside of the ship

            if (MeltdownConfig.Instance.OVERRIDE_APPARATUS_VALUE)
                __instance.scrapValue = MeltdownConfig.Instance.APPARATUS_VALUE;
            ___isLungDocked = false; // fix joining late
        }
    }
}
