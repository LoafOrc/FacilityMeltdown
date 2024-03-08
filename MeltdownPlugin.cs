using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Logging;
using FacilityMeltdown.Behaviours;
using FacilityMeltdown.Effects;
using FacilityMeltdown.Integrations;
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
    [BepInDependency("me.loaforc.soundapi", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("evaisa.lethallib")]
    [BepInDependency("com.sigurd.csync")]
    public class MeltdownPlugin : BaseUnityPlugin {
        internal const string modGUID = "me.loaforc.facilitymeltdown";
        internal const string modName = "FacilityMeltdown";
        internal const string modVersion = "2.4.5";

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

            /*
            if(BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("me.loaforc.soundapi")) {
                try {
                    MeltdownSoundAPI.Integrate();
                } catch(Exception e) {
                    Logger.LogError("An error occured while trying to integrate into soundapi!");
                    Logger.LogWarning("Keep in mind I may not have actually updated soundapi to include the new api :3");
                    Logger.LogError(e);
                    Logger.LogWarning("Keep in mind I may not have actually updated soundapi to include the new api :3");
                }
            }
            */

            

                logger.LogInfo(modName + ":" + modVersion + " has succesfully loaded!");
        }

        void RegisterItems() {
            logger.LogInfo("Registering Items");

            Items.RegisterShopItem(Assets.geigerCounterItemDef, null, null, Assets.geigerCounterNode, 90);
        }

        void OnDisable() {
            if (
                BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("den.meltdownchance") ||
                BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("PizzaProbability")
            ) {
                logger.LogInfo("You are using a mod that makes meltdown have a random chance of occuring.");
                logger.LogInfo("Keep in mind this goes against meltdown, but your choice.");
                logger.LogWarning("However; BY DESIGN these mods need to mess with the internals of meltdown");
                logger.LogWarning("so I personally will be offering low to no support with these mods installed.");
            }
        }

        void RegisterNetworking() {
            logger.LogInfo("Doing networky stuff");
                var types = new Type[]{ typeof(MeltdownHandler), typeof(GeigerCounterItem) };
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
                        logger.LogWarning("supressed an error from netcode patcher, probably fine but should still log that something happened.");
                    }
                }

            logger.LogInfo("= Registering Network Prefabs");
            //NetworkPrefabs.RegisterNetworkPrefab(Assets.meltdownHandlerPrefab);
            //NetworkPrefabs.RegisterNetworkPrefab(Assets.geigerCounterItem);
        }
        void RegisterPatches() {
            logger.LogInfo("Applying patches.");
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), modGUID);
        }
        void RegisterEffects() {
            logger.LogInfo("Using own API to register sequence effects.");
            new EmergencyLightsEffect();
            new InsideFacilityParticleEffects();
            new ShockwaveSpawner();
            new WarningAnnouncerEffect();
            new RadiationIncreasingEffect();
            new IntroDialogueSequencer();
        }
    }
}