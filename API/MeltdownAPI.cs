using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace FacilityMeltdown.API;
public static class MeltdownAPI {
    public static bool MeltdownStarted { get { return MeltdownHandler.Instance != null; } }

    public static void StartMeltdown(string modGUID) {
        if (MeltdownStarted) return;
        MeltdownPlugin.logger.LogInfo($"A mod ({modGUID}) has begun a meltdown!");
        if(!GameNetworkManager.Instance.localPlayerController.IsHost) {
            MeltdownPlugin.logger.LogWarning("Local Player isn't host! Aborting!!");
        }
        GameObject meltdown = GameObject.Instantiate(Assets.meltdownHandlerPrefab);
        meltdown.GetComponent<NetworkObject>().Spawn();
    }
}
