using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace FacilityMeltdown.Patches {
    [HarmonyPatch(typeof(GameNetworkManager))]
    internal class GameNetworkManagerPatch {
        internal static List<GameObject> networkPrefabsToRegister = [];

        [HarmonyPatch(nameof(GameNetworkManager.Start)), HarmonyPrefix]
        static void AddNetworkPrefabs() {
            foreach(GameObject prefab in networkPrefabsToRegister) {
                NetworkManager.Singleton.AddNetworkPrefab(prefab);
                MeltdownPlugin.logger.LogDebug($"Registered {prefab.name} as a network prefab.");
            }
            MeltdownPlugin.logger.LogInfo($"Succesfully registered {networkPrefabsToRegister.Count} network prefabs.");
        }
    }
}
