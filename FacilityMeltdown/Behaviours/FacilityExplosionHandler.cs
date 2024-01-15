using FacilityMeltdown.Util;
using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace FacilityMeltdown.Behaviours;

internal class FacilityExplosionHandler : MonoBehaviour {
    PlayerControllerB player => GameNetworkManager.Instance.localPlayerController;
    float size, time;

    LocalVolumetricFog internalFog;

    void Awake() {
        internalFog = GetComponent<LocalVolumetricFog>();
        if(internalFog == null) {
            MeltdownPlugin.logger.LogError("Failed to get volumetric fog!");
        }

        if (MeltdownConfig.Default.CFG_SCREEN_SHAKE.Value) {
            HUDManager.Instance.ShakeCamera(ScreenShakeType.Small);
            HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
            HUDManager.Instance.ShakeCamera(ScreenShakeType.Long);
            HUDManager.Instance.ShakeCamera(ScreenShakeType.VeryStrong);
        }

        if (!ShouldIgnorePlayer() && player.isInsideFactory) KillPlayer();
    }

    void Update() {
        time += Time.deltaTime * 10;
        size = TimeToSize(time);

        transform.localScale = Vector3.one * size;
        if (internalFog != null) {
            internalFog.parameters.size = Vector3.one * size * 1.25f;
        }

        if (!ShouldIgnorePlayer() && PlayerIsInsideFireball()) {
            player.KillPlayer(Vector3.zero, false, CauseOfDeath.Blast);
        }
    }

    void KillPlayer() {
        player.KillPlayer(Vector3.zero, false, CauseOfDeath.Blast);
    }

    bool PlayerIsInsideFireball() {
        return Vector3.Distance(transform.position, player.transform.position) < size;
    }

    bool ShouldIgnorePlayer() {
        return player.isPlayerDead || (player.isInElevator && StartOfRound.Instance.shipIsLeaving);
    }

    float TimeToSize(float time) {
        return Mathf.Log(time) + 3 + (2 * time);
    } 
}