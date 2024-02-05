using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;

namespace FacilityMeltdown.Patches {
    [HarmonyPatch(typeof(GameNetworkManager))]
    internal class GameNetworkManagerPatch {
        [HarmonyPrefix, HarmonyPatch(nameof(GameNetworkManager.Start))]
        internal static void RegisterPrefabs() {
            NetworkManager.Singleton.AddNetworkPrefab(Assets.meltdownHandlerPrefab);
            NetworkManager.Singleton.AddNetworkPrefab(Assets.geigerCounterItem);
        }
    }
}
