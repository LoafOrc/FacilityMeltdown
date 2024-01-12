using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using FacilityMeltdown.Behaviours;
using FacilityMeltdown.Util;
using GameNetcodeStuff;
using RuntimeNetcodeRPCValidator;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.HighDefinition;

namespace FacilityMeltdown {
    public class MeltdownHandler : MonoBehaviour {
        PlayerControllerB player => GameNetworkManager.Instance.localPlayerController;

        private AudioSource musicSource, warningSound;
        internal static MeltdownHandler Instance;

        internal float meltdownTimer = 2 * 60;
        internal int causedBy;

        private bool radiationIncrease = false;
        GameObject shockwave, explosion;
        List<GameObject> spawnedParticleEffects = new List<GameObject>();

        Vector3 effectOrigin;

        DialogueSegment[] shipTakeOffDialogue = new DialogueSegment[] { 
            new DialogueSegment { bodyText = "The company has deemed the current levels of radiation too high." },
            new DialogueSegment { bodyText = "The company can not risk damaging it's equipment." }
        };

        void Start() {
            if(Instance != null) {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            MeltdownPlugin.logger.LogInfo("Beginning Meltdown Sequence! I'd run if I was you!");

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.clip = Assets.music;
            musicSource.spatialBlend = 0;
            musicSource.loop = false;
            musicSource.Play();

            warningSound = gameObject.AddComponent<AudioSource>();
            warningSound.spatialBlend = 0;
            warningSound.loop = false;

            // Effect Handlers
            StartCoroutine(EmergencyLights());
            StartCoroutine(EffectSpawningHandler());
            StartCoroutine(WarningAnnouncerHandler());
            StartCoroutine(ShockwaveSpawningHandler());

            if (GameNetworkManager.Instance.localPlayerController.IsServer) {
                List<EnemyVent> avaliableVents = new List<EnemyVent>();
                for (int i = 0; i < RoundManager.Instance.allEnemyVents.Length; i++) {
                    if (!RoundManager.Instance.allEnemyVents[i].occupied) {
                        avaliableVents.Add(RoundManager.Instance.allEnemyVents[i]);
                    }
                }
                avaliableVents.Shuffle();
                for (int i = 0; i < Mathf.Min(MeltdownConfig.Instance.MONSTER_SPAWN_AMOUNT, avaliableVents.Count); i++) {
                    RoundManager.Instance.SpawnEnemyFromVent(avaliableVents[i]);
                }
                foreach (Turret turret in GameObject.FindObjectsOfType<Turret>()) {
                    turret.EnterBerserkModeServerRpc(causedBy);
                }
            }

            MeltdownPlugin.logger.LogInfo("Spawning Effects");
            for(int i = 0; i < Random.Range(5, 15); i++) { // todo: make this scale based on map size
                SpawnRandomEffect();
            }

            effectOrigin = RoundManager.FindMainEntrancePosition(false, true);
            if(effectOrigin == Vector3.zero) {
                MeltdownPlugin.logger.LogError("Effect Origin is Vector3.Zero! We couldn't find the effect origin");
                HUDManager.Instance.DisplayGlobalNotification("Failed to find effect origin... Things will look broken.");
            }

            HUDManager.Instance.ReadDialogue(new DialogueSegment[] {
                new DialogueSegment {
                    bodyText = "... FAILED TO CONNECT TO INTERNAL FACILITY COMPUTER ... IDENTIFIYING ROOT CAUSE ..."
                },
                new DialogueSegment {
                    bodyText = "UNSTABLE NUCLEAR REACTOR ... PREDICTING TIME UNTIL CATASTROPHIC EVENT ...",
                    waitTime = 6
                },
                new DialogueSegment {
                    bodyText = "2 MINUTES UNTIL CATASTRPOHIC NUCLEAR REACTOR EVENT"
                }
            });
            if (MeltdownConfig.Default.CFG_SCREEN_SHAKE.Value) {
                HUDManager.Instance.ShakeCamera(ScreenShakeType.VeryStrong);
                HUDManager.Instance.ShakeCamera(ScreenShakeType.Long);
            }
        }

        Vector3 GetRandomPositionNearPlayer(float radius = 15f) {
            Vector3 position = player.transform.position + (Random.insideUnitSphere * radius);
            return RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(position, 10f, layerMask: -1, randomSeed: new System.Random());
        }

        Vector3 GetRandomPositionInsideFacility() {
            Vector3 position = RoundManager.Instance.insideAINodes[Random.Range(0, RoundManager.Instance.insideAINodes.Length)].transform.position;
            return RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(position, 10f, layerMask: -1, randomSeed: new System.Random());
        }

        void SpawnRandomEffect() {
            Vector3 position = GetRandomPositionInsideFacility() + Vector3.up;
            RaycastHit hit;
            if (Physics.Raycast(new Ray(position, Vector3.up), out hit, 20f, 256)) {
                GameObject prefab = Assets.facilityEffects[Random.Range(0, Assets.facilityEffects.Length)];
                GameObject created = Instantiate(prefab);
                prefab.transform.position = hit.point;
                spawnedParticleEffects.Add(created);
            }
            MeltdownPlugin.logger.LogWarning("Failed to spawn effect, raycast failed.");
        }


        void CreateShockwave() {
            if(shockwave != null) Destroy(shockwave);

            shockwave = Instantiate(Assets.shockwavePrefab);
            shockwave.transform.position = effectOrigin;
            shockwave.AddComponent<Shockwave>();
        }
        
        void OnDisable() { 
            if(explosion != null)
                Destroy(explosion);
            if(shockwave != null)
                Destroy(shockwave);
            Instance = null;

            foreach(GameObject effect in spawnedParticleEffects) {
                Destroy(effect);
            }
        }

        IEnumerator EmergencyLights() {
            yield return null;
            yield return new WaitForSeconds(5f);

            for (int i = 0; i < RoundManager.Instance.allPoweredLights.Count; i++) {
                RoundManager.Instance.allPoweredLights[i].color = Color.red;
            }

            while (!HasExplosionOccured()) {
                for (int i = 0; i < RoundManager.Instance.allPoweredLightsAnimators.Count; i++) {
                    RoundManager.Instance.allPoweredLightsAnimators[i].SetBool("on", true);
                }

                yield return new WaitForSeconds(2f);

                for (int i = 0; i < RoundManager.Instance.allPoweredLightsAnimators.Count; i++) {
                    RoundManager.Instance.allPoweredLightsAnimators[i].SetBool("on", false);
                }

                yield return new WaitForSeconds(5f);
            }

            yield break;
        }

        void Update() {
            StartOfRound shipManager = StartOfRound.Instance;

            musicSource.volume = (float)MeltdownConfig.Default.CFG_MUSIC_VOLUME.Value / 100f;
            warningSound.volume = (float)MeltdownConfig.Default.CFG_MUSIC_VOLUME.Value / 100f;

            if (!player.isInsideFactory && !MeltdownConfig.Default.CFG_MUSIC_PLAYS_OUTSIDE.Value) {
                musicSource.volume = 0;
                warningSound.volume = 0;
            }

            meltdownTimer -= Time.deltaTime;

            if (meltdownTimer <= 60 && !radiationIncrease) {
                radiationIncrease = true;
                HUDManager.Instance.RadiationWarningHUD();
            }

            if (meltdownTimer <= 3 && !shipManager.shipIsLeaving) { 
                StartCoroutine(ShipTakeOff());
            }

            if (meltdownTimer <= 0) {
                musicSource.Stop();
                warningSound.Stop();

                explosion = GameObject.Instantiate(Assets.facilityExplosionPrefab);
                explosion.transform.position = effectOrigin;
            }
        }

        IEnumerator WarningAnnouncerHandler() {
            yield return null;

            while (!HasExplosionOccured()) {
                AudioClip sound = Assets.warnings[Random.Range(0, Assets.warnings.Length)];
                warningSound.clip = sound;
                warningSound.Play();

                yield return new WaitForSeconds(Random.Range(10f, 17f));
            }

            yield break;
        }

        IEnumerator ShockwaveSpawningHandler() {
            yield return null;

            while (!HasExplosionOccured()) {
                if (shockwave != null) Destroy(shockwave);

                shockwave = Instantiate(Assets.shockwavePrefab);
                shockwave.transform.position = effectOrigin;
                shockwave.AddComponent<Shockwave>();


                yield return new WaitForSeconds(Random.Range(20f, 30f));
            }

            yield break;
        }

        IEnumerator EffectSpawningHandler() {
            yield return null;

            while(!HasExplosionOccured()) {
                for (int i = 0; i < Random.Range(5, 15); i++) { // todo: make this scale based on map size
                    SpawnRandomEffect();
                }

                if (player.isInsideFactory)
                    Instantiate(StartOfRound.Instance.explosionPrefab, GetRandomPositionNearPlayer(), Quaternion.Euler(-90f, 0f, 0f), RoundManager.Instance.mapPropsContainer.transform);
                if (MeltdownConfig.Default.CFG_SCREEN_SHAKE.Value) {
                    if (meltdownTimer > 60) {
                        HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
                    } else {
                        HUDManager.Instance.ShakeCamera(ScreenShakeType.VeryStrong);
                    }
                }

                yield return new WaitForSeconds(Random.Range(8f, 14f));
            }

            yield break;
        }

        IEnumerator ShipTakeOff() {
            StartOfRound shipManager = StartOfRound.Instance;
            shipManager.shipLeftAutomatically = true;
            shipManager.shipIsLeaving = true;

            HUDManager.Instance.ReadDialogue(shipTakeOffDialogue);

            yield return new WaitForSeconds(3f); // Wait for explosion
            yield return new WaitForSeconds(3f);
            HUDManager.Instance.shipLeavingEarlyIcon.enabled = false;
            StartMatchLever startMatchLever = Object.FindObjectOfType<StartMatchLever>();
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

            Debug.Log(string.Format("Is in elevator D?: {0}", GameNetworkManager.Instance.localPlayerController.isInElevator));
            yield return new WaitForSeconds(9.5f);
            
            yield break;
        }

        public bool HasExplosionOccured() {
            return explosion != null;
        }
    }
}
