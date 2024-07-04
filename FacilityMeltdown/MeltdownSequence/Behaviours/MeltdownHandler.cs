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
using Random = UnityEngine.Random;

namespace FacilityMeltdown.MeltdownSequence.Behaviours;
public class MeltdownHandler : NetworkBehaviour
{
    public float TimeLeftUntilMeltdown => meltdownTimer;

    public float Progress => 1 - (TimeLeftUntilMeltdown / MeltdownPlugin.config.MeltdownTime);

    static PlayerControllerB Player => GameNetworkManager.Instance.localPlayerController;
    internal static MeltdownHandler Instance { get; private set; }

    internal float meltdownTimer;
    bool meltdownStarted = false;

    GameObject explosion, shockwave;

    [SerializeField]
    AudioSource meltdownMusic, warningSource;

    Vector3 effectOrigin => MeltdownMoonMapper.Instance.EffectOrigin;

    [ClientRpc]
    void StartMeltdownClientRpc()
    {
        if (Instance != null) return;
        Instance = this;

        Stopwatch timer = new Stopwatch();

        MeltdownPlugin.logger.LogInfo("Beginning Meltdown Sequence! I'd run if I were you!");

        timer.Start();
        MeltdownMoonMapper.EnsureMeltdownMoonMapper();
        MeltdownInteriorMapper.EnsureMeltdownInteriorMapper();


        if (MeltdownInteriorMapper.Instance == null) MeltdownPlugin.logger.LogError("WHAT. Just ensured that the interior mapper exists and it doesnt?!?");
        if (MeltdownMoonMapper.Instance == null) MeltdownPlugin.logger.LogError("WHAT. Just ensured that the moon mapper exists and it doesnt?!?");

        timer.Stop();
        MeltdownPlugin.logger.LogDebug($"Ensuring data objects exist: {timer.ElapsedMilliseconds}ms");

        meltdownTimer = MeltdownPlugin.config.MeltdownTime;

        timer.Restart();

        timer.Stop();
        MeltdownPlugin.logger.LogDebug($"Creating audio: {timer.ElapsedMilliseconds}ms");

        timer.Restart();
        #region EFFECTS
        StartCoroutine(
            MeltdownEffects.WithDelay(() =>
            {
                HUDManager.Instance.ReadDialogue(GetDialogue("meltdown.dialogue.start"));
            }, 5)
        );

        StartCoroutine(
            MeltdownEffects.WithDelay(
                MeltdownEffects.RepeatUntilEndOfMeltdown(
                    () => MeltdownEffects.WithRandomDelay(
                        MeltdownEffects.WarningAnnouncer(warningSource),
                        15,
                        20
                    ))
            , 10)
        );

        StartCoroutine(
            MeltdownEffects.RepeatUntilEndOfMeltdown(
                () => MeltdownEffects.WithRandomDelay(
                    MeltdownEffects.InsideParticleEffects,
                    10,
                    15
                ))
        );

        if (MeltdownPlugin.config.EmergencyLights)
        {
            try
            {
                List<(Light, Color)> originalLightColours = MeltdownEffects.SetupEmergencyLights();
                StartCoroutine(
                    MeltdownEffects.RepeatUntilEndOfMeltdown(
                        () =>
                        {
                            return MeltdownEffects.EmergencyLights(2, 5, originalLightColours);
                        }
                    )
                );
            }
            catch (Exception e)
            {
                MeltdownPlugin.logger.LogError($"Failed to set the emergency light colour: {e}");
            }
        }

        StartCoroutine(
            MeltdownEffects.RepeatUntilEndOfMeltdown(
                () =>
                {
                    return MeltdownEffects.WithRandomDelay(
                        () =>
                        {
                            if (shockwave != null) GameObject.Destroy(shockwave);

                            shockwave = GameObject.Instantiate(MeltdownPlugin.assets.shockwavePrefab);
                            shockwave.transform.position = effectOrigin;
                        }
                    , 25, 35);
                }
            )
        );

        StartCoroutine(
            MeltdownEffects.AtProgress(
                HUDManager.Instance.RadiationWarningHUD,
                .5f
            )
        );

        #endregion

        timer.Stop();
        MeltdownPlugin.logger.LogDebug($"Starting effects: {timer.ElapsedMilliseconds}ms");

        if (GameNetworkManager.Instance.localPlayerController.IsServer)
        {
            SpawnEnemies();
        }

        timer.Restart();

        if (effectOrigin == Vector3.zero)
        {
            MeltdownPlugin.logger.LogError("Effect Origin is Vector3.Zero! We couldn't find the effect origin");
            HUDManager.Instance.DisplayGlobalNotification("Failed to find effect origin... Things will look broken.");
        }

        if (MeltdownPlugin.clientConfig.ScreenShake)
        {
            HUDManager.Instance.ShakeCamera(ScreenShakeType.VeryStrong);
            HUDManager.Instance.ShakeCamera(ScreenShakeType.Long);
        }
        timer.Stop();
        MeltdownPlugin.logger.LogDebug($"Getting effect origin: {timer.ElapsedMilliseconds}ms");

        timer.Restart();
        MeltdownAPI.OnMeltdownStart();
        timer.Stop();
        MeltdownPlugin.logger.LogDebug($"MeltdownAPI.OnMeltdownStart(): {timer.ElapsedMilliseconds}ms");
        meltdownStarted = true;
    }

    void SpawnEnemies()
    {
        Stopwatch timer = Stopwatch.StartNew();
        List<string> disallowed = MeltdownPlugin.config.GetDisallowedEnemies();
        List<SpawnableEnemyWithRarity> spawnProbibilities = RoundManager.Instance.currentLevel.Enemies.Where(enemy =>
        {
            return !EnemyCannotBeSpawned(enemy.enemyType) && !disallowed.Contains(enemy.enemyType.enemyName);
        }).ToList();

        timer.Stop();
        MeltdownPlugin.logger.LogDebug($"Filtering enemies: {timer.ElapsedMilliseconds}ms");

        timer.Restart();
        List<EnemyVent> avaliableVents = RoundManager.Instance.allEnemyVents.Where(vent => !vent.occupied).ToList();
        timer.Stop();
        MeltdownPlugin.logger.LogDebug($"Filtering vents: {timer.ElapsedMilliseconds}ms");
        timer.Restart();


        for (int i = 0; i < Mathf.Min(MeltdownPlugin.config.MonsterSpawnAmount, avaliableVents.Count); i++)
        {
            EnemyVent vent = avaliableVents[Random.Range(0, avaliableVents.Count)];
            avaliableVents.Remove(vent);
            SpawnableEnemyWithRarity enemy = spawnProbibilities[Random.Range(0, avaliableVents.Count)];

            RoundManager.Instance.currentEnemyPower += enemy.enemyType.PowerLevel;
            MeltdownPlugin.logger.LogInfo("Spawning a " + enemy.enemyType.enemyName + " during the meltdown sequence");
            vent.SpawnEnemy(enemy);
        }

        timer.Stop();
        MeltdownPlugin.logger.LogDebug($"Spawning enemies: {timer.ElapsedMilliseconds}ms");

    }

    public override void OnNetworkSpawn()
    {
        if (!MeltdownPlugin.loadedFully || !IsHost)
        {
            return;
        }
        StartMeltdownClientRpc();
    }

    internal bool EnemyCannotBeSpawned(EnemyType type)
    {
        return type.spawningDisabled || type.numberSpawned >= type.MaxCount;
    }

    internal static DialogueSegment[] GetDialogue(string translation)
    {
        JArray translatedDialogue = LangParser.GetTranslationSet(translation);
        DialogueSegment[] dialogue = new DialogueSegment[translatedDialogue.Count];
        for (int i = 0; i < translatedDialogue.Count; i++)
        {
            dialogue[i] = new DialogueSegment
            {
                bodyText = ((string)translatedDialogue[i]).Replace("<meltdown_time>", Math.Round((float)MeltdownPlugin.config.MeltdownTime / 60).ToString()),
                speakerText = "meltdown.dialogue.speaker".Translate()
            };
        }
        return dialogue;
    }

    void OnDisable()
    {
        if (Instance != this) return;

        MeltdownPlugin.logger.LogInfo("Cleaning up MeltdownHandler.");

        Instance = null;
        if (explosion != null)
            Destroy(explosion);
        if (shockwave != null)
            Destroy(shockwave);

        if (!meltdownStarted)
        {
            MeltdownPlugin.logger.LogError("MeltdownHandler was disabled without starting a meltdown, a client most likely failed the MeltdownReadyCheck. If you are going to report this make sure to provide ALL client logs.");
        }
    }

    void Update()
    {
        if (!meltdownStarted) return;
        if (HasExplosionOccured()) return;
        StartOfRound shipManager = StartOfRound.Instance;

        meltdownMusic.volume = MeltdownPlugin.clientConfig.MusicVolume / 100f;

        if (!Player.isInsideFactory && !MeltdownPlugin.clientConfig.MusicPlaysOutside)
        {
            meltdownMusic.volume = 0;
        }

        meltdownTimer -= Time.deltaTime;

        if (meltdownTimer <= 3 && !shipManager.shipIsLeaving)
        {
            StartCoroutine(ShipTakeOff());
        }

        if (meltdownTimer <= 0)
        {
            meltdownMusic.Stop();

            GameObject explosionPrefab = MeltdownMoonMapper.Instance.explosionPrefab;
            if (explosionPrefab == null)
                explosionPrefab = MeltdownPlugin.assets.facilityExplosionPrefab;

            explosion = Instantiate(explosionPrefab);
            explosion.transform.position = effectOrigin;
            if (!explosion.TryGetComponent<FacilityExplosionHandler>(out var _))
            {
                explosion.AddComponent<FacilityExplosionHandler>();
            }
        }
    }

    IEnumerator ShipTakeOff()
    {
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
        if (GameNetworkManager.Instance.localPlayerController.isPlayerDead)
        {
            GameNetworkManager.Instance.localPlayerController.SetSpectatedPlayerEffects(true);
        }
        yield return new WaitForSeconds(1f);

        yield return new WaitForSeconds(9.5f);

        yield break;
    }

    public bool HasExplosionOccured()
    {
        return explosion != null;
    }
}