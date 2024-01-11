using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Logging;
using FacilityMeltdown.Patches;
using FacilityMeltdown.Util;
using HarmonyLib;
using LethalConfig;
using LethalConfig.ConfigItems;
using LethalConfig.ConfigItems.Options;
using RuntimeNetcodeRPCValidator;
using Unity.Netcode;
using UnityEngine;

namespace FacilityMeltdown {
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency("ainavt.lc.lethalconfig", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.willis.lc.lethalsettings", BepInDependency.DependencyFlags.SoftDependency)]
    public class MeltdownPlugin : BaseUnityPlugin {
        internal const string modGUID = "me.loaforc.facilitymeltdown";
        internal const string modName = "FacilityMeltdown";
        internal const string modVersion = "1.2.0";

        private readonly Harmony harmony = new Harmony(modGUID);
        internal static MeltdownPlugin instance;
        internal static ManualLogSource logger;

        internal static MeltdownConfig meltdownConfig;

        void Awake() {
            if (instance == null) instance = this; // Signleton
            else return; // Make sure nothing else gets loaded.
            logger = BepInEx.Logging.Logger.CreateLogSource(modGUID);

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

            meltdownConfig = new MeltdownConfig(Config);

            logger.LogInfo("Checking for any mod settings managers...");
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("ainavt.lc.lethalconfig")) {
                meltdownConfig.InitLethalConfig();
            }
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.willis.lc.lethalsettings")) {
                meltdownConfig.InitLethalSettings();
            }

            logger.LogInfo("Initalising assets...");
            Assets.Init();

            logger.LogInfo("Applying patches.");
            harmony.PatchAll(typeof(ApparaticePatch));
            harmony.PatchAll(typeof(EntranceTeleportPatch));
            harmony.PatchAll(typeof(RoundManagerPatch));
            harmony.PatchAll(typeof(MeltdownConfig));


            logger.LogInfo(modName + ":" + modVersion + " has succesfully loaded!");
        }

        void OnDestroy() {
            //logger.LogInfo("========== DEBUG");
            //var unlitHandler = new GameObject("UnlitModeHandler").AddComponent<UnlitModeHandler>();
            //DontDestroyOnLoad(unlitHandler);
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
}