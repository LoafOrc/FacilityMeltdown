using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Logging;
using FacilityMeltdown.Effects;
using FacilityMeltdown.Patches;
using FacilityMeltdown.Util;
using HarmonyLib;
using LethalConfig;
using LethalConfig.ConfigItems;
using LethalConfig.ConfigItems.Options;
using LethalLib.Modules;
using RuntimeNetcodeRPCValidator;
using UnityEngine;

namespace FacilityMeltdown {
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency("ainavt.lc.lethalconfig", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.willis.lc.lethalsettings", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(LethalLib.Plugin.ModGUID)]
    public class MeltdownPlugin : BaseUnityPlugin {
        internal const string modGUID = "me.loaforc.facilitymeltdown";
        internal const string modName = "FacilityMeltdown";
        internal const string modVersion = "1.2.3";

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

            RegisterNetworking();

            MeltdownPlugin.logger.LogInfo("Setting up config...");
            meltdownConfig = new MeltdownConfig(Config);

            RegisterPatches();
            RegisterEffects();

            logger.LogInfo("Creating commands");
            TerminalHandler.Init();

            logger.LogInfo(modName + ":" + modVersion + " has succesfully loaded!");
        }

        void RegisterNetworking() {
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
            } catch (Exception e) {
                logger.LogWarning("Caught exception: " + e);
                logger.LogWarning("why does this not work");
            }
            NetworkPrefabs.RegisterNetworkPrefab(Assets.meltdownHandlerPrefab);
        }
        void RegisterPatches() {
            logger.LogInfo("Applying patches.");
            harmony.PatchAll(typeof(ApparaticePatch));
            harmony.PatchAll(typeof(EntranceTeleportPatch));
            harmony.PatchAll(typeof(StartOfRoundPatches));
            harmony.PatchAll(typeof(MeltdownConfig));
        }
        void RegisterEffects() {
            logger.LogInfo("Using own API to register sequence effects.");
            new EmergencyLightsEffect();
            new InsideFacilityParticleEffects();
            new ShockwaveSpawner();
            new WarningAnnouncerEffect();
        }
    }
}