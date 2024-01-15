using System;
using System.Collections;
using System.Collections.Generic;
using FacilityMeltdown.API;
using FacilityMeltdown.Behaviours;
using FacilityMeltdown.Util;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

namespace FacilityMeltdown;

public class MeltdownHandler : NetworkBehaviour {
    static PlayerControllerB Player => GameNetworkManager.Instance.localPlayerController;

    private AudioSource musicSource;
    internal static MeltdownHandler Instance;

    internal float meltdownTimer = 2 * 60;

    private bool radiationIncrease = false;
    GameObject explosion;
    List<MeltdownSequenceEffect> activeEffects = new List<MeltdownSequenceEffect>();

    Vector3 effectOrigin;

    readonly static DialogueSegment[] shipTakeOffDialogue = new DialogueSegment[] { 
        new DialogueSegment { bodyText = "The company has deemed the current levels of radiation too high." },
        new DialogueSegment { bodyText = "The company can not risk damaging it's equipment." }
    };

    readonly static DialogueSegment[] introDialogue = new DialogueSegment[] {
        new DialogueSegment {
            bodyText = "... FAILED TO CONNECT TO INTERNAL FACILITY COMPUTER ... IDENTIFIYING ROOT CAUSE ..."
        },
        new DialogueSegment {
            bodyText = "UNSTABLE NUCLEAR REACTOR ... PREDICTING TIME UNTIL CATASTROPHIC EVENT ...",
            waitTime = 6
        },
        new DialogueSegment {
            bodyText = "<color=\"red\">2 MINUTES</color> UNTIL CATASTRPOHIC NUCLEAR REACTOR EVENT"
        }
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

        // Effect Handlers
        foreach(MeltdownSequenceEffect effect in MeltdownSequenceEffect.effects) {
            StartCoroutine(PlayMeltdownSequenceEffect(effect));
        }

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
        }

        effectOrigin = RoundManager.FindMainEntrancePosition(false, true);
        if(effectOrigin == Vector3.zero) {
            MeltdownPlugin.logger.LogError("Effect Origin is Vector3.Zero! We couldn't find the effect origin");
            HUDManager.Instance.DisplayGlobalNotification("Failed to find effect origin... Things will look broken.");
        }

        HUDManager.Instance.ReadDialogue(introDialogue);
        if (MeltdownConfig.Default.CFG_SCREEN_SHAKE.Value) {
            HUDManager.Instance.ShakeCamera(ScreenShakeType.VeryStrong);
            HUDManager.Instance.ShakeCamera(ScreenShakeType.Long);
        }
    }
        
    void OnDisable() { 
        Instance = null;
        if(explosion != null)
            Destroy(explosion);

        foreach(MeltdownSequenceEffect effect in activeEffects) {
            try {
                effect.Cleanup();
            } catch(Exception e) {
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
        if (HasExplosionOccured()) return;
        StartOfRound shipManager = StartOfRound.Instance;

        musicSource.volume = (float)MeltdownConfig.Default.CFG_MUSIC_VOLUME.Value / 100f;

        if (!Player.isInsideFactory && !MeltdownConfig.Default.CFG_MUSIC_PLAYS_OUTSIDE.Value) {
            musicSource.volume = 0;
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

            explosion = GameObject.Instantiate(Assets.facilityExplosionPrefab);
            explosion.transform.position = effectOrigin;
            explosion.AddComponent<FacilityExplosionHandler>();
        }
    }

    IEnumerator ShipTakeOff() {
        StartOfRound shipManager = StartOfRound.Instance;
        shipManager.shipLeftAutomatically = true;
        shipManager.shipIsLeaving = true;

        HUDManager.Instance.ReadDialogue(shipTakeOffDialogue);

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

        Debug.Log(string.Format("Is in elevator D?: {0}", GameNetworkManager.Instance.localPlayerController.isInElevator));
        yield return new WaitForSeconds(9.5f);
            
        yield break;
    }

    public bool HasExplosionOccured() {
        return explosion != null;
    }
}