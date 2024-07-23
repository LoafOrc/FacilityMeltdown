using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FacilityMeltdown.API;
using FacilityMeltdown.Behaviours;
using FacilityMeltdown.Integrations;
using FacilityMeltdown.Util;
using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FacilityMeltdown.Patches
{
    [HarmonyPatch(typeof(LungProp))]
    static class ApparaticePatch
    {
        [HarmonyPrefix, HarmonyPatch(nameof(LungProp.EquipItem))]
        internal static void BeginMeltdownSequence(LungProp __instance, ref bool ___isLungDocked)
        {
            if (!__instance.IsHost) return;
            if (!___isLungDocked) return;
            if (MeltdownAPI.MeltdownStarted) return;

            // We just took it out
            try
            { // make sure to surround in try catch because this is a prefix
                if (MeltdownPlugin.config.OverrideApparatusValue)
                    __instance.scrapValue = Mathf.RoundToInt(MeltdownPlugin.config.ApparatusValue * WeatherRegistryIntegration.GetWeatherMultiplier());
                GameObject meltdown = GameObject.Instantiate(MeltdownPlugin.assets.meltdownHandlerPrefab);
                meltdown.GetComponent<NetworkObject>().Spawn();
            }
            catch (Exception ex)
            {
                MeltdownPlugin.logger.LogError(ex);
            }
        }



        [HarmonyPrefix, HarmonyPatch(nameof(LungProp.Start))]
        internal static void AddRadiationSource(LungProp __instance)
        {
            MeltdownMoonMapper.EnsureMeltdownMoonMapper();
            MeltdownInteriorMapper.EnsureMeltdownInteriorMapper();

            try
            {
                RadiationSource source = __instance.gameObject.AddComponent<RadiationSource>();
                source.radiationAmount = 80;
                source.radiationDistance = 60;

                MeltdownMoonMapper.EnsureMeltdownMoonMapper();

                if (MeltdownPlugin.config.OverrideApparatusValue)
                    __instance.scrapValue = Mathf.RoundToInt(MeltdownPlugin.config.ApparatusValue * WeatherRegistryIntegration.GetWeatherMultiplier());
                MeltdownPlugin.logger.LogDebug(__instance.scrapValue);
                //___isLungDocked = false; // fix joining late
            }
            catch (Exception ex)
            {
                MeltdownPlugin.logger.LogError(ex);
            }
        }
    }
}
