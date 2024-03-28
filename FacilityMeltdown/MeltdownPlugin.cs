using BepInEx;
using BepInEx.Logging;
using FacilityMeltdown.Behaviours;
using FacilityMeltdown.Equipment;
using FacilityMeltdown.Integrations;
using FacilityMeltdown.Lang;
using FacilityMeltdown.MeltdownSequence.Behaviours;
using FacilityMeltdown.Util;
using FacilityMeltdown.Util.Attributes;
using HarmonyLib;
using LethalLib.Modules;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.SceneManagement;

namespace FacilityMeltdown {
    [BepInPlugin(modGUID, modName, modVersion)]
    [CompatibleDependency("ainavt.lc.lethalconfig", typeof(LethalConfigIntergration))]
    [CompatibleDependency("com.willis.lc.lethalsettings", typeof(LethalSettingsIntegration))]
    [BepInDependency("evaisa.lethallib")]
    [BepInDependency("com.sigurd.csync")]
    [BepInDependency("atomic.terminalapi")]
    public class MeltdownPlugin : BaseUnityPlugin {
        internal const string modGUID = "me.loaforc.facilitymeltdown";
        internal const string modName = "FacilityMeltdown";
        internal const string modVersion = "2.5.0";

        internal static MeltdownPlugin instance;
        internal static ManualLogSource logger;

        public static bool loadedFully { get; private set; } = false;

        void Awake() {
            if (instance == null) instance = this; // Signleton
            else return; // Make sure nothing else gets loaded.
            logger = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            if (!RunLoadStep("Assets.Init", "Getting assets", Assets.Init)) return;
            if (!RunLoadStep("LangParser.Init", "Getting possible languages", LangParser.Init)) return;
            if (!RunLoadStep("new MeltdownConfig()", "Setting up config", () => { new MeltdownConfig(Config); })) return;
            if (!RunLoadStep("LangParser.SetLanguage", "Setting language", () => { LangParser.SetLanguage(MeltdownConfig.Instance.LANGUAGE.Value); })) return;
            if (!RunLoadStep("RegisterPatches", "Integrating into LethalCompany", RegisterPatches)) return;
            if (!RunLoadStep("RegisterNetworking", "Making sure everything is networked", RegisterNetworking)) return;
            if (!RunLoadStep("TerminalHandler.Init", "Adding commands to the terminal", TerminalHandler.Init)) {
                logger.LogWarning("Failed to initalise terminal commands,");
            }
            if (RunLoadStep("RegisterItems", "Adding the Geiger Counter", RegisterItems)) {
                logger.LogWarning("Failed to register the geiger counter.");
            }
            if(!RunLoadStep("CompatibleDependency.Init", "Checking for any soft dependencies", () => { CompatibleDependencyAttribute.Init(this); })) {
                logger.LogWarning("Doing something with soft dependencies broke, meltdown itself should be fine.");
            }

            SceneManager.sceneLoaded += (scene, __) => {
                if (scene.name == "SampleSceneRelay") return;
                if (scene.name == "CompanyBuilding") return;
                if(GameNetworkManager.Instance == null) return;
                if(GameNetworkManager.Instance.localPlayerController == null) return;
                if(GameObject.FindObjectOfType<MeltdownMoonMapper>() != null) return; // skipping as the moon has its own override

                MeltdownMoonMapper mappings = new GameObject("DefaultMeltdownMappings").AddComponent<MeltdownMoonMapper>();
                mappings.outsideEmergencyLights = GameObject.Find("Environment").GetComponentsInChildren<Light>().Where((light) => {
                    return CheckParentForDisallowed(light.transform);
                }).ToList();
            };

            loadedFully = true;
            logger.LogInfo(modName + ":" + modVersion + " has succesfully loaded!");
            logger.LogInfo(@"         .-_; ;_-.          ");
            logger.LogInfo(@"        / /     \ \        ");
            logger.LogInfo(@"       | |       | |      ");
            logger.LogInfo(@"        \ \.---./ /       ");
            logger.LogInfo(@"  .-\""""~   .---.   ~""""-.");
            logger.LogInfo(@"  ,`.-~/ .'`---`'. \\~-.`,");
            logger.LogInfo(@" ' `   | | \(_)/ | |   `' ");
            logger.LogInfo(@"  ,    \  \ | | /  /    , ");
            logger.LogInfo(@"  ;`'.,_\  `-'-'  /_,.'`; ");
            logger.LogInfo(@"   '-._  _.-'^'-._  _.-'    ");
        }

        bool CheckParentForDisallowed(Transform child) {
            if(child.gameObject.name == "Sun" || child.gameObject.name == "ItemShipAnimContainer") return false;
            if(child.parent == null) return true;
            return CheckParentForDisallowed(child.parent);
        }

        bool RunLoadStep(string stepName, string descrption, Action callback) {
            try {
                callback();
            } catch(Exception e) {
                logger.LogError($"`{stepName}` caused a fatal exception to be thrown.");
                logger.LogError(e);
                return false;
            }
            logger.LogInfo(stepName.PadRight(25) + " == " + descrption);
            return true;
        }

        void RegisterItems() {
            logger.LogInfo("Registering Items");

            Items.RegisterShopItem(Assets.geigerCounterItemDef, null, null, Assets.geigerCounterNode, 90);
        }

        void OnDisable() {
            if(!loadedFully) {
                Harmony.UnpatchID(modGUID);
                logger.LogInfo("Unpatching as something failed while loading.");
                return;
            }

            if (
                BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("den.meltdownchance") ||
                BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("PizzaProbability")
            ) {
                logger.LogInfo("You are using a mod that makes meltdown have a random chance of occuring.");
                logger.LogInfo("Keep in mind this goes against meltdown's core design, but your choice.");
                logger.LogWarning("However; BY DESIGN these mods need to mess with the internals of meltdown");
                logger.LogWarning("so I personally will be offering low to no support with these mods installed.");
            }
        }

        void RegisterNetworking() {
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
                } catch(Exception) {
                    logger.LogWarning("supressed an error from netcode patcher, probably fine but should still log that something happened.");
                }
            }
        }
        void RegisterPatches() {
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), modGUID);
        }
    }
}