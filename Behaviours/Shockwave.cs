using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FacilityMeltdown.Util;
using GameNetcodeStuff;
using UnityEngine;

namespace FacilityMeltdown.Behaviours {
    public class Shockwave : MonoBehaviour {
        bool localPlayerCameraShake = false;
        float size = 0;

        AudioSource sound;

        void Awake() {
            sound = gameObject.AddComponent<AudioSource>();
            sound.clip = Assets.shockwave;
            sound.spatialBlend = 0;
            sound.loop = false;
        }

        void Update() {
            PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;


            size += Time.deltaTime * 50;
            transform.localScale = Vector3.one * size;

            if(player.isInsideFactory) {
                // hide the shockwave
            } else {
                if(PlayerIsInsideShockwave() && !localPlayerCameraShake && MeltdownConfig.Default.CFG_SCREEN_SHAKE.Value) {
                    if(MeltdownConfig.Default.CFG_SCREEN_SHAKE.Value) {
                        ScreenShake();
                    }
                    localPlayerCameraShake = true;
                    sound.Play();
                }
            }
        }

        void ScreenShake() {
            HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
        }

        internal bool PlayerIsInsideShockwave() {
            return Vector3.Distance(transform.position, GameNetworkManager.Instance.localPlayerController.transform.position) <= size;
        }
    }
}
