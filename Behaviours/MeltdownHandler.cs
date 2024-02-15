using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FacilityMeltdown.API;
using FacilityMeltdown.Behaviours;
using FacilityMeltdown.Lang;
using FacilityMeltdown.Util;
using GameNetcodeStuff;
using JetBrains.Annotations;
using LethalLib.Modules;
using Newtonsoft.Json.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.AI;
using UnityEngine.Rendering.HighDefinition;

namespace FacilityMeltdown {
    public class MeltdownHandler : NetworkBehaviour {
        static PlayerControllerB Player => GameNetworkManager.Instance.localPlayerController;
        private AudioSource musicSource;
        internal static MeltdownHandler Instance;

        internal float meltdownTimer;
        bool meltdownStarted = false;

        GameObject explosion;
        List<MeltdownSequenceEffect> activeEffects = new List<MeltdownSequenceEffect>();
        List<ulong> readyPlayers = new List<ulong>();

        Vector3 effectOrigin;

        [ClientRpc]
        void StartMeltdownClientRpc() {
            if(meltdownStarted) return;
            Stopwatch entireFunction = Stopwatch.StartNew();
            meltdownTimer = MeltdownConfig.Instance.MELTDOWN_TIME.Value;
            MeltdownPlugin.logger.LogInfo("Beginning Meltdown Sequence! I'd run if I was you!");

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.clip = Assets.defaultMusic;
            musicSource.spatialBlend = 0;
            musicSource.loop = false;
            musicSource.Play();

            // Effect Handlers
            IEnumerator PlayEffects() {
                foreach (MeltdownSequenceEffect effect in MeltdownAPI.RegisteredEffects) {
                    StartCoroutine(PlayMeltdownSequenceEffect(effect));
                    yield return new WaitForSeconds(.1f);
                }

                yield break;
            }
            StartCoroutine(PlayEffects());

            Stopwatch findMainEntrance = Stopwatch.StartNew();
            effectOrigin = RoundManager.FindMainEntrancePosition(false, true);
            if (effectOrigin == Vector3.zero) {
                MeltdownPlugin.logger.LogError("Effect Origin is Vector3.Zero! We couldn't find the effect origin");
                HUDManager.Instance.DisplayGlobalNotification("Failed to find effect origin... Things will look broken.");
            }
            findMainEntrance.Stop();
            MeltdownPlugin.logger.LogInfo($"[Measurements] Finding Main Entrance took: {findMainEntrance.ElapsedMilliseconds} ms.");

            if (MeltdownConfig.Default.SCREEN_SHAKE.Value) {
                HUDManager.Instance.ShakeCamera(ScreenShakeType.VeryStrong);
                HUDManager.Instance.ShakeCamera(ScreenShakeType.Long);
            }
            meltdownStarted = true;
            entireFunction.Stop();
            MeltdownPlugin.logger.LogInfo($"[Measurements] Entire meltdown setup took: {entireFunction.ElapsedMilliseconds} ms.");
        } 

        [ServerRpc(RequireOwnership = false)]
        void MeltdownReadyServerRpc(ulong clientId) {
            readyPlayers.Add(clientId);

            if(readyPlayers.Count == StartOfRound.Instance.GetConnectedPlayers().Count) {
                StartMeltdownClientRpc();
            }
        }

        void Start() {
            if (Instance != null) {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public override void OnNetworkSpawn() {
            MeltdownReadyServerRpc(NetworkManager.LocalClientId);
        }
            
        internal static DialogueSegment[] GetDialogue(string translation) {
            JArray translatedDialogue = LangParser.GetTranslationSet(translation);
            DialogueSegment[] dialogue = new DialogueSegment[translatedDialogue.Count];
            for (int i = 0; i < translatedDialogue.Count; i++) {
                dialogue[i] = new DialogueSegment {
                    bodyText = ((string)translatedDialogue[i]).Replace("<meltdown_time>", Math.Round((float)MeltdownConfig.Instance.MELTDOWN_TIME.Value / 60).ToString()),
                    speakerText = "meltdown.dialogue.speaker".Translate()
                };
            }
            return dialogue;
        }

        void OnDisable() {
            Instance = null;
            if (explosion != null)
                Destroy(explosion);

            if(!meltdownStarted) {
                MeltdownPlugin.logger.LogError("MeltdownHandler was disabled without starting a meltdown, a client most likely failed the MeltdownReadyCheck. If you are going to report this make sure to provide ALL client logs.");
            }

            foreach (MeltdownSequenceEffect effect in activeEffects) {
                try {
                    effect.Cleanup();
                } catch (Exception e) {
                    MeltdownPlugin.logger.LogError(effect.FullName + " produced a " + e.GetType().Name + " during Cleanup(). Some objects may still be visible between moons." + "\n" + e);
                }
            }
        }

        IEnumerator PlayMeltdownSequenceEffect(MeltdownSequenceEffect effect) {
            yield return null;
            if (!effect.IsEnabledOnThisMoon(StartOfRound.Instance.currentLevel)) {
                MeltdownPlugin.logger.LogInfo(effect.Name + " will not be playing on this moon.");
                yield break;
            }
            try {
                effect.Setup();
            } catch (Exception e) {
                MeltdownPlugin.logger.LogError(effect.FullName + " produced a " + e.GetType().Name + " during Setup(). This error is fatal and the effect will not continue." + "\n" + e);
                yield break;
            }
            if (!effect.IsOneShot && !effect.Playing) {
                MeltdownPlugin.logger.LogWarning(
                    effect.FullName + " is not playing and is not marked as one shot. By default it will play once. If you are not the dev please report this." +
                    "If you are the dev, did you intend this effect to play once? Or play continously?" +
                    "If you meant to have it play once. Please overwrite .IsOneShot to be true." +
                    "If you meant to have it play continously. Please call base.Setup() in your Setup function."
                    );
            }
            bool effectivelyOneShot = effect.IsOneShot || !effect.Playing;

            if (effectivelyOneShot) {
                yield return StartCoroutine(effect.Play(meltdownTimer));
            } else {
                while (!HasExplosionOccured() && effect.Playing) {
                    yield return StartCoroutine(effect.Play(meltdownTimer));
                }
            }

            yield return StartCoroutine(effect.Stop());

            yield break;
        }

        void Update() {
            if (!meltdownStarted) return;
            if (HasExplosionOccured()) return;
            StartOfRound shipManager = StartOfRound.Instance;

            musicSource.volume = (float)MeltdownConfig.Default.MUSIC_VOLUME.Value / 100f;

            if (!Player.isInsideFactory && !MeltdownConfig.Default.MUSIC_PLAYS_OUTSIDE.Value) {
                musicSource.volume = 0;
            }

            meltdownTimer -= Time.deltaTime;

            if (meltdownTimer <= 3 && !shipManager.shipIsLeaving) {
                StartCoroutine(ShipTakeOff());
            }

            if (meltdownTimer <= 0) {
                musicSource.Stop();

                explosion = GameObject.Instantiate(Assets.facilityExplosionPrefab);
                explosion.transform.position = effectOrigin;
                explosion.AddComponent<FacilityExplosionHandler>();
            }
        }

        IEnumerator ShipTakeOff() {
            StartOfRound shipManager = StartOfRound.Instance;
            shipManager.shipLeftAutomatically = true;
            shipManager.shipIsLeaving = true;

            HUDManager.Instance.ReadDialogue(GetDialogue("meltdown.dialogue.shiptakeoff"));

            yield return new WaitForSeconds(3f); // Wait for explosion
            yield return new WaitForSeconds(3f);
            HUDManager.Instance.shipLeavingEarlyIcon.enabled = false;
            StartMatchLever startMatchLever = GameObject.FindObjectOfType<StartMatchLever>();
            startMatchLever.triggerScript.animationString = "SA_PushLeverBack";
            startMatchLever.leverHasBeenPulled = false;
            startMatchLever.triggerScript.interactable = false;
            startMatchLever.leverAnimatorObject.SetBool("pullLever", false);
            shipManager.ShipLeave();
            yield return new WaitForSeconds(1.5f);
            shipManager.SetSpectateCameraToGameOverMode(true, null);
            if (GameNetworkManager.Instance.localPlayerController.isPlayerDead) {
                GameNetworkManager.Instance.localPlayerController.SetSpectatedPlayerEffects(true);
            }
            yield return new WaitForSeconds(1f);

            yield return new WaitForSeconds(9.5f);

            yield break;
        }

        public bool HasExplosionOccured() {
            return explosion != null;
        }
    }
}
