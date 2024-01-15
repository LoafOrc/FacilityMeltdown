﻿using System;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using FacilityMeltdown.Effects;
using FacilityMeltdown.Patches;
using FacilityMeltdown.Util;
using HarmonyLib;
using LethalLib.Modules;
using UnityEngine;

namespace FacilityMeltdown;

[BepInPlugin(modGUID, modName, modVersion)]
[BepInDependency("ainavt.lc.lethalconfig", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("com.willis.lc.lethalsettings", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(LethalLib.Plugin.ModGUID)]
public class MeltdownPlugin : BaseUnityPlugin {
    internal const string modGUID = "me.loaforc.facilitymeltdown";
    internal const string modName = MyPluginInfo.PLUGIN_NAME;
    internal const string modVersion = MyPluginInfo.PLUGIN_VERSION;

    private readonly Harmony harmony = new Harmony(modGUID);
    internal static MeltdownPlugin instance;
    internal static ManualLogSource logger;

    internal static MeltdownConfig meltdownConfig;

    void Awake() {
        if (instance == null) instance = this; // Signleton
        else return; // Make sure nothing else gets loaded.
        logger = BepInEx.Logging.Logger.CreateLogSource(modGUID);
        logger.LogInfo("Initalising assets...");
        Assets.Init();

        logger.LogInfo("Doing networky stuff");
        var types = Assembly.GetExecutingAssembly().GetTypes();
        try {
            foreach (var type in types) {
                logger.LogInfo(type.Assembly.FullName);
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods) {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0) {
                        method.Invoke(null, null);
                    }
                }
            }
        } catch(Exception e) {
            logger.LogWarning("Caught exception: " + e);
            logger.LogWarning("why does this not work");
        }
        NetworkPrefabs.RegisterNetworkPrefab(Assets.meltdownHandlerPrefab);

        meltdownConfig = new MeltdownConfig(Config);

        logger.LogInfo("Checking for any mod settings managers...");
        if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("ainavt.lc.lethalconfig")) {
            meltdownConfig.InitLethalConfig();
        }
        if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.willis.lc.lethalsettings")) {
            meltdownConfig.InitLethalSettings();
        }


        logger.LogInfo("Applying patches.");
        harmony.PatchAll(typeof(ApparaticePatch));
        harmony.PatchAll(typeof(EntranceTeleportPatch));
        harmony.PatchAll(typeof(StartOfRoundPatches));
        harmony.PatchAll(typeof(MeltdownConfig));

        logger.LogInfo("Using own API to register sequence effects.");
        new EmergencyLightsEffect();
        new InsideFacilityParticleEffects();
        new ShockwaveSpawner();
        new WarningAnnouncerEffect();

        logger.LogInfo(modName + ":" + modVersion + " has succesfully loaded!");
    }

    private static void AppratusIncrease() {
        logger.LogInfo("Making the reward worth the risk...");
        UnityEngine.Object[] scriptableObjects = Resources.FindObjectsOfTypeAll(typeof(ScriptableObject));
        foreach (ScriptableObject scriptableObject in scriptableObjects) {
            logger.LogInfo("Checking: " + scriptableObject.name);
            if (scriptableObject.name == "LungApparatus") {
                logger.LogInfo("LUNGAGAPRATQTEAF!");
                ((Item)scriptableObject).minValue = 120;
                ((Item)scriptableObject).maxValue = 240;
                ((Item)scriptableObject).creditsWorth = 240;

                break;
            }
        }
    }
}
