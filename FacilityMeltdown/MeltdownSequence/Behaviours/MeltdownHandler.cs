using System;
using System.Collections;
using System.Collections.Generic;
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

namespace FacilityMeltdown.MeltdownSequence.Behaviours;
public class MeltdownHandler : NetworkBehaviour {
    public float TimeLeftUntilMeltdown { get { return meltdownTimer; } }
    public float Progress { get {
        return 1 - (TimeLeftUntilMeltdown / MeltdownConfig.Instance.MELTDOWN_TIME.Value);
    } }

    static PlayerControllerB Player => GameNetworkManager.Instance.localPlayerController;
    private AudioSource musicSource, warningSource;
    internal static MeltdownHandler Instance;

    internal float meltdownTimer;
    bool meltdownStarted = false;

    GameObject explosion, shockwave;
    List<ulong> readyPlayers = new List<ulong>();

    Vector3 effectOrigin;


    [ClientRpc]
    void StartMeltdownClientRpc() {
        meltdownTimer = MeltdownConfig.Instance.MELTDOWN_TIME.Value;
        Instance = this;
        MeltdownPlugin.logger.LogInfo("Beginning Meltdown Sequence! I'd run if I was you! MeltdownTimer: " + meltdownTimer);

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.clip = Assets.defaultMusic;
        musicSource.spatialBlend = 0;
        musicSource.loop = false;
        musicSource.Play();

        warningSource = gameObject.AddComponent<AudioSource>();
        warningSource.spatialBlend = 0;
        warningSource.loop = false;

        #region EFFECTS
        StartCoroutine(
            MeltdownEffects.WithDelay(() => {
                HUDManager.Instance.ReadDialogue(GetDialogue("meltdown.dialogue.start"));
            }, 5)
        );

        StartCoroutine(
            MeltdownEffects.WithDelay(
                MeltdownEffects.RepeatUntilEndOfMeltdown(
                    MeltdownEffects.WithDynamicRandomDelay(
                        MeltdownEffects.WarningAnnouncer(warningSource)
                    )
                )
            , 10)
        );

        StartCoroutine(
            MeltdownEffects.RepeatUntilEndOfMeltdown(
                MeltdownEffects.WithDynamicRandomDelay(
                    MeltdownEffects.InsideParticleEffects
                )
            )
        );

        MeltdownEffects.SetupEmergencyLights();
        StartCoroutine(
            MeltdownEffects.RepeatUntilEndOfMeltdown(
                MeltdownEffects.EmergencyLights(2, 5)
            )
        );

        StartCoroutine(
            MeltdownEffects.RepeatUntilEndOfMeltdown(
                MeltdownEffects.WithRandomDelay(
                    () => {
                        if(shockwave != null) GameObject.Destroy(shockwave);

                        shockwave = GameObject.Instantiate(Assets.shockwavePrefab);
                        shockwave.transform.position = effectOrigin;
                    }
                , 25, 35)
            )
        );

        StartCoroutine(
            MeltdownEffects.AtProgress(
                HUDManager.Instance.RadiationWarningHUD,
                .5f
            )
        );

        #endregion

        if(GameNetworkManager.Instance.localPlayerController.IsServer) {
            List<string> disallowed = MeltdownConfig.Instance.GetDisallowedEnemies();
            List<SpawnableEnemyWithRarity> allowedEnemies = new List<SpawnableEnemyWithRarity>();
            foreach (SpawnableEnemyWithRarity enemy in RoundManager.Instance.currentLevel.Enemies) {
                if (disallowed.Contains(enemy.enemyType.enemyName)) continue;
                allowedEnemies.Add(enemy);
            }
            List<int> spawnProbibilities = new List<int>();
            foreach (SpawnableEnemyWithRarity enemy in allowedEnemies) {
                if (EnemyCannotBeSpawned(enemy.enemyType)) continue;
                spawnProbibilities.Add(enemy.rarity);
            }

            List<EnemyVent> avaliableVents = new List<EnemyVent>();
            for (int i = 0; i < RoundManager.Instance.allEnemyVents.Length; i++) {
                if (!RoundManager.Instance.allEnemyVents[i].occupied) {
                    avaliableVents.Add(RoundManager.Instance.allEnemyVents[i]);
                }
            }
            avaliableVents.Shuffle();
            for (int i = 0; i < Mathf.Min(MeltdownConfig.Instance.MONSTER_SPAWN_AMOUNT.Value, avaliableVents.Count); i++) {
                EnemyVent vent = avaliableVents[i];
                int randomWeightedIndex = RoundManager.Instance.GetRandomWeightedIndex([.. spawnProbibilities], RoundManager.Instance.EnemySpawnRandom);
                if (EnemyCannotBeSpawned(allowedEnemies[randomWeightedIndex].enemyType)) continue;
                RoundManager.Instance.currentEnemyPower += allowedEnemies[randomWeightedIndex].enemyType.PowerLevel;

                MeltdownPlugin.logger.LogInfo("Spawning a " + allowedEnemies[randomWeightedIndex].enemyType.enemyName + " during the meltdown sequence");
                vent.SpawnEnemy(allowedEnemies[randomWeightedIndex]);
            }
        }

        effectOrigin = RoundManager.FindMainEntrancePosition(false, true);
        if (effectOrigin == Vector3.zero) {
            MeltdownPlugin.logger.LogError("Effect Origin is Vector3.Zero! We couldn't find the effect origin");
            HUDManager.Instance.DisplayGlobalNotification("Failed to find effect origin... Things will look broken.");
        }

        if (MeltdownConfig.Default.SCREEN_SHAKE.Value) {
            HUDManager.Instance.ShakeCamera(ScreenShakeType.VeryStrong);
            HUDManager.Instance.ShakeCamera(ScreenShakeType.Long);
        }
        MeltdownAPI.OnMeltdownStart();
        meltdownStarted = true;
    }

    [ServerRpc(RequireOwnership = false)]
    void MeltdownReadyServerRpc(ulong clientId) {
        readyPlayers.Add(clientId);

        if (readyPlayers.Count == StartOfRound.Instance.GetConnectedPlayers().Count) {
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
        if (!MeltdownPlugin.loadedFully) {
            MeltdownPlugin.logger.LogError("Meltdown didn't load fully correctly and so this client blocked the Meltdown Sequence");
            return;
        }
        MeltdownReadyServerRpc(NetworkManager.LocalClientId);
    }

    internal bool EnemyCannotBeSpawned(EnemyType type) {
        return type.spawningDisabled || type.numberSpawned >= type.MaxCount;
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
        if(shockwave != null)
            Destroy(shockwave);

        if (!meltdownStarted) {
            MeltdownPlugin.logger.LogError("MeltdownHandler was disabled without starting a meltdown, a client most likely failed the MeltdownReadyCheck. If you are going to report this make sure to provide ALL client logs.");
        }
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

            GameObject explosionPrefab = MeltdownMoonMapper.Instance.explosionPrefab;
            if (explosionPrefab == null)
                explosionPrefab = Assets.facilityExplosionPrefab;

            explosion = Instantiate(explosionPrefab);
            explosion.transform.position = effectOrigin;
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
        StartMatchLever startMatchLever = FindObjectOfType<StartMatchLever>();
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