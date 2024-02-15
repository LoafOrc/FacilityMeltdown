using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Logging;
using FacilityMeltdown.API;
using FacilityMeltdown.Effects;
using FacilityMeltdown.Lang;
using FacilityMeltdown.Patches;
using FacilityMeltdown.Util;
using HarmonyLib;
using LethalConfig;
using LethalConfig.ConfigItems;
using LethalConfig.ConfigItems.Options;
using LethalLib.Modules;
using UnityEngine;

namespace FacilityMeltdown {
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency("ainavt.lc.lethalconfig", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.willis.lc.lethalsettings", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("evaisa.lethallib")]
    [BepInDependency("io.github.CSync")]
    public class MeltdownPlugin : BaseUnityPlugin {
        internal const string modGUID = "me.loaforc.facilitymeltdown";
        internal const string modName = "FacilityMeltdown";
        internal const string modVersion = "2.3.0";

        private readonly Harmony harmony = new Harmony(modGUID);
        internal static MeltdownPlugin instance;
        internal static ManualLogSource logger;

        internal static MeltdownConfig meltdownConfig;

        void Awake() {
            if (instance == null) instance = this; // Singleton
            else return; // Make sure nothing else gets loaded.
            logger = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            logger.LogInfo("Initalising assets...");
            Assets.Init();

            RegisterNetworking();
            logger.LogInfo("Setting up language");
            LangParser.Init();

            logger.LogInfo("Setting up config...");
            meltdownConfig = new MeltdownConfig(Config);

            logger.LogInfo("Setting up language part 2..");
            LangParser.SetLanguage(MeltdownConfig.Default.LANGUAGE.Value);

            logger.LogInfo(LangParser.GetTranslationSet("meltdown.dialogue.start"));

            RegisterPatches();
            RegisterEffects();

            logger.LogInfo("Creating commands");
            TerminalHandler.Init();

            RegisterItems();

            logger.LogInfo(modName + ":" + modVersion + " has succesfully loaded!");
        }

        void OnDisable() { // called after everything has been inited
            // Check for silly meltdown chance mod.
            if(BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("den.meltdownchance")) {
                logger.LogWarning("================================");
                logger.LogWarning("");
                logger.LogWarning("YOU ARE USING THE MELTDOWN CHANCE MOD.");
                logger.LogWarning("THIS GOES AGAINST THE DESIGN OF MELTDOWN.");
                logger.LogWarning("YOU WILL RECIEVE NO SUPPORT FROM ME WHILE,");
                logger.LogWarning("YOU HAVE THE MELTDOWN CHANCE MOD");
                logger.LogWarning("for information about why i disagree with it:");
                logger.LogWarning("https://www.youtube.com/watch?v=pdqy3J5LF5M");
                logger.LogWarning("");
                logger.LogWarning("================================");
                logger.LogWarning("");
                logger.LogWarning("IF YOU ARE THE DEV OF MELTDOWN CHANCE");
                logger.LogWarning("Please do the following:");
                logger.LogWarning(" a) Add a warning in the readme that it is not ");
                logger.LogWarning("    the intended way to play meltdown");
                logger.LogWarning(" b) Do not attempt to suppress this warning. ");
                logger.LogWarning(" c) Go to my discord thread in the modding ");
                logger.LogWarning("    discord and I can show you the correct ");
                logger.LogWarning("    way to do it. ");
                logger.LogWarning("I want to help out another mod creator,");
                logger.LogWarning("This is something I don't agree with, and");
                logger.LogWarning("you've made it a seperate mod. I'm okay");
                logger.LogWarning("with that. It's just that people are going");
                logger.LogWarning("to blame base meltdown and I've got other");
                logger.LogWarning("stuff to do.");
                logger.LogWarning("================================");
            }
        }

        void RegisterItems() {
            logger.LogInfo("Registering Items");

            Items.RegisterShopItem(Assets.geigerCounterItemDef, null, null, Assets.geigerCounterNode, 90);
        }

        void RegisterNetworking() {
            logger.LogInfo("Doing networky stuff");
            var types = Assembly.GetExecutingAssembly().GetTypes();
                foreach (var type in types) {
                    try {
                        var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                        foreach (var method in methods) {
                            var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                            if (attributes.Length > 0) {
                                method.Invoke(null, null);
                            }
                        }
                    } catch (Exception e) {
                        logger.LogWarning("netcode patcher threw an exception -------- MOST LIKELY FINE");
                    }
                }

            logger.LogInfo("= Registering Network Prefabs");
            NetworkPrefabs.RegisterNetworkPrefab(Assets.meltdownHandlerPrefab);
            //NetworkPrefabs.RegisterNetworkPrefab(Assets.geigerCounterItem);
        }
        void RegisterPatches() {
            logger.LogInfo("Applying patches.");
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), modGUID);
        }
        void RegisterEffects() {
            logger.LogInfo("Using own API to register sequence effects.");
            MeltdownAPI.RegisterEffect([
                new EmergencyLightsEffect(),
                new InsideFacilityParticleEffects(),
                new ShockwaveSpawner(),
                new WarningAnnouncerEffect(),
                new RadiationIncreasingEffect(),
                new IntroDialogueSequencer()
            ]);
            
        }
    }
}