using System.Collections;
using System.Collections.Generic;
using FacilityMeltdown.Util;
using GameNetcodeStuff;
using RuntimeNetcodeRPCValidator;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.HighDefinition;

namespace FacilityMeltdown {
    public class MeltdownHandler : MonoBehaviour {
        private AudioSource musicSource, warningSound, shockwaveSound;
        internal static MeltdownHandler Instance;

        internal float meltdownTimer = 2 * 60, explosionTime = 0;
        private float timeUntilNextWarning = 3, timeUntilNextShake = 10, timeUntilNextShockwave = 20, timeUntilMoreEffects = 15;
        internal int causedBy;

        private bool explosionOccured = false, radiationIncrease = false, shockwaveShookCamera = false;
        LocalVolumetricFog explosionSmoke;
        GameObject Shockwave;
        private float explosionSize, shockwaveSize;
        List<GameObject> spawnedParticleEffects = new List<GameObject>();

        Vector3 effectOrigin;

        Coroutine emergencyLightsCoroutine;
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

            shockwaveSound = gameObject.AddComponent<AudioSource>();
            shockwaveSound.clip = Assets.shockwave;
            shockwaveSound.spatialBlend = 0;
            shockwaveSound.loop = false;

            warningSound = gameObject.AddComponent<AudioSource>();
            warningSound.spatialBlend = 0;
            warningSound.loop = false;


            emergencyLightsCoroutine = StartCoroutine(EmergencyLights());
            if (GameNetworkManager.Instance.localPlayerController.IsServer) {
                List<EnemyVent> avaliableVents = new List<EnemyVent>();
                for (int i = 0; i < RoundManager.Instance.allEnemyVents.Length; i++) {
                    if (!RoundManager.Instance.allEnemyVents[i].occupied) {
                        avaliableVents.Add(RoundManager.Instance.allEnemyVents[i]);
                    }
                }
                avaliableVents.Shuffle();
                for (int i = 0; i < Mathf.Min(MeltdownConfig.Instance.MONSTER_SPAWN_AMOUNT.Value, avaliableVents.Count); i++) {
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
            if (MeltdownConfig.Default.SCREEN_SHAKE.Value) {
                HUDManager.Instance.ShakeCamera(ScreenShakeType.VeryStrong);
                HUDManager.Instance.ShakeCamera(ScreenShakeType.Long);
            }
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
            if(Shockwave != null) Destroy(Shockwave);

            Shockwave = Instantiate(Assets.shockwavePrefab);
            Shockwave.transform.position = effectOrigin;
            shockwaveShookCamera = false;
            shockwaveSize = 0;
        }
        
        void OnDisable() { 
            StopCoroutine(emergencyLightsCoroutine);
            if(explosionSmoke != null)
            Destroy(explosionSmoke.transform.parent.gameObject);
            if(Shockwave != null)
                Destroy(Shockwave);
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

            while (!explosionOccured) {
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
            PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
            StartOfRound shipManager = StartOfRound.Instance;

            musicSource.volume = (float) MeltdownConfig.Default.MUSIC_VOLUME.Value / 100f;
            warningSound.volume = (float) MeltdownConfig.Default.MUSIC_VOLUME.Value / 100f;

            if(!player.isInsideFactory && !MeltdownConfig.Default.MUSIC_PLAYS_OUTSIDE.Value) {
                musicSource.volume = 0;   
                warningSound.volume = 0;
            }

            if(Shockwave != null) {
                shockwaveSize += Time.deltaTime * 20;
                Shockwave.transform.localScale = Vector3.one * shockwaveSize;
            }

            if(timeUntilNextShockwave <= 0) {
                CreateShockwave();
                timeUntilNextShockwave = Random.Range(20, 30);
            }

            if(!player.isInsideFactory && !shockwaveShookCamera && MeltdownConfig.Default.SCREEN_SHAKE.Value && Vector3.Distance(Shockwave.transform.position, player.transform.position) < shockwaveSize) {
                HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
                shockwaveSound.Play();
                shockwaveShookCamera = true;
            }

            if (explosionOccured) {
                explosionTime += Time.deltaTime * 10;
                explosionSize = Mathf.Log(explosionTime) + 3 + 2*explosionTime;

                if(explosionSmoke != null) {
                    explosionSmoke.transform.parent.localScale = Vector3.one * explosionSize;
                    explosionSmoke.parameters.size = Vector3.one * explosionSize * 1.25f;
                }

                if (player.isPlayerDead || (player.isInElevator && shipManager.shipIsLeaving)) return;
                if (Vector3.Distance(explosionSmoke.transform.parent.position, player.transform.position) < explosionSize*.4f) {
                    player.KillPlayer(Vector3.zero, false, CauseOfDeath.Blast);
                }
                 
                return;
            }
            meltdownTimer -= Time.deltaTime;
            timeUntilNextWarning -= Time.deltaTime;
            timeUntilNextShake -= Time.deltaTime;
            timeUntilMoreEffects -= Time.deltaTime;
            timeUntilNextShockwave -= Time.deltaTime;

            if(timeUntilMoreEffects <= 0) {
                for (int i = 0; i < Random.Range(5, 15); i++) { // todo: make this scale based on map size
                    SpawnRandomEffect();
                }
                timeUntilMoreEffects = Random.Range(8f, 14f);
            }

            if(timeUntilNextWarning <= 0) {
                PlayRandomWarningSound();
                timeUntilNextWarning = Random.Range(10f, 17f); // TODO: Make this speed up towards the end. Maybe a quadratic?
            }

            if(meltdownTimer <= 60 && !radiationIncrease) {
                radiationIncrease = true;
                HUDManager.Instance.RadiationWarningHUD();
            }

            if(meltdownTimer <= 3 && !shipManager.shipIsLeaving) {
                shipManager.shipLeftAutomatically = true;
                shipManager.shipIsLeaving = true;
                StartCoroutine(shipTakeOff());
            }

            if (meltdownTimer <= 0) {
                explosionOccured = true;
                timeUntilNextShockwave = 0;
                if(player.isInsideFactory) {
                    player.KillPlayer(Vector3.zero, false, CauseOfDeath.Blast);
                }

                musicSource.Stop();
                warningSound.Stop();

                if (MeltdownConfig.Default.SCREEN_SHAKE.Value) {
                    HUDManager.Instance.ShakeCamera(ScreenShakeType.Small);
                    HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
                    HUDManager.Instance.ShakeCamera(ScreenShakeType.Long);
                    HUDManager.Instance.ShakeCamera(ScreenShakeType.VeryStrong);
                }

                GameObject explosion = GameObject.Instantiate(Assets.facilityExplosionPrefab);
                explosion.transform.position = effectOrigin;
                explosionSmoke = explosion.GetComponentInChildren<LocalVolumetricFog>();

            }

            if (timeUntilNextShake <= 0) {
                if(GameNetworkManager.Instance.localPlayerController.isInsideFactory)
                    GameObject.Instantiate(StartOfRound.Instance.explosionPrefab, GetRandomPositionInsideFacility(), Quaternion.Euler(-90f, 0f, 0f), RoundManager.Instance.mapPropsContainer.transform);
                if (MeltdownConfig.Default.SCREEN_SHAKE.Value) {
                    if (meltdownTimer > 60) {
                        HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
                    } else {
                        HUDManager.Instance.ShakeCamera(ScreenShakeType.VeryStrong);
                    }
                }

                timeUntilNextShake = Random.Range(6f, 10f);
            }
        }

        IEnumerator shipTakeOff() {
            StartOfRound shipManager = StartOfRound.Instance;
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
            return explosionOccured;
        }

        void PlayRandomWarningSound() {
            AudioClip sound = Assets.warnings[Random.Range(0, Assets.warnings.Length)];
            warningSound.clip = sound;
            warningSound.Play();
        }
    }
}
