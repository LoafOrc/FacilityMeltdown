using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using LethalConfig.ConfigItems.Options;
using LethalConfig.ConfigItems;
using LethalConfig;
using System.Runtime.CompilerServices;
using LethalSettings.UI;
using LethalSettings.UI.Components;
using FacilityMeltdown.Networking;
using Unity.Collections;
using Unity.Netcode;
using GameNetcodeStuff;
using HarmonyLib;
using System.Runtime.Serialization;
using CSync.Lib;
using CSync.Util;

namespace FacilityMeltdown.Util {
    [DataContract]
    internal class MeltdownConfig : SyncedInstance<MeltdownConfig> {
        [DataMember]
        internal SyncedEntry<int> MONSTER_SPAWN_AMOUNT, APPARATUS_VALUE, MELTDOWN_TIME;
        [DataMember]
        internal SyncedEntry<bool> OVERRIDE_APPARATUS_VALUE, EMERGENCY_LIGHTS;
        [DataMember]
        internal SyncedEntry<float> SCAN_COOLDOWN, SCAN_ACCURACY;
        
        // todo: set this to a synced entry when CSync fixes
        internal ConfigEntry<string> DISALLOWED_ENEMIES;

        internal ConfigEntry<float> MUSIC_VOLUME;
        internal ConfigEntry<bool> SCREEN_SHAKE, MUSIC_PLAYS_OUTSIDE, PARTICLE_EFFECTS;

        [DataMember]
        internal string DISALLOWED_ENEMIES_HACKFIX; // CSync doesn't let you SyncedEntry<string>????

        internal List<string> GetDisallowedEnemies() {
            return DISALLOWED_ENEMIES_HACKFIX.Split(',').ToList();
        }

        internal MeltdownConfig(ConfigFile file) { 
            InitInstance(this);         

            OVERRIDE_APPARATUS_VALUE = file.BindSyncedEntry("GameBalance", "OverrideAppartusValue", true, "Whether or not FacilityMeltdown should override appartus value. Only use for compatibility reasons");
            APPARATUS_VALUE = file.BindSyncedEntry("GameBalance", "AppartusValue", 240, "What the value of the appartus should be set as IF override appartus value is `true`");
            MONSTER_SPAWN_AMOUNT = file.BindSyncedEntry("GameBalance", "MonsterSpawnAmount", 5, "How many monsters should spawn during the meltdown sequence? Set to 0 to disable.");
            EMERGENCY_LIGHTS = file.BindSyncedEntry("GameBalance", "EmergencyLights", true, "Should the lights turn on periodically? Disabling this option makes them permanently off. (Matches Vanilla Behaviour)");

            DISALLOWED_ENEMIES = file.Bind("GameBalance", "DisallowedEnemies", "Centipede,Hoarding bug", "What enemies to exclude from spawning in the meltdown sequence. Comma seperated list. \"Should\" support modded entities");
            DISALLOWED_ENEMIES_HACKFIX = DISALLOWED_ENEMIES.Value;

            MELTDOWN_TIME = file.BindSyncedEntry("GameBalance", "MeltdownTime", 120, "ABSOLUETLY NOT SUPPORTED OR RECOMMENDED! Change the length of the meltdown sequence. If this breaks I am not fixing it, you have been warned.");

            SCAN_COOLDOWN = file.BindSyncedEntry("GameBalance", "ShipScannerCooldown", 15f, "How long until the ship's scanner can scan the reactor. (Doesn't affect the vanilla `scan` command)");
            SCAN_ACCURACY = file.BindSyncedEntry("GameBalance", "ShipScannerAccuracy", 10f, "How accurate is the ship's scanner when scanning the reactor. Higher values mean it is more uncertain, and lower values is more accurate. (Doesn't affect the vanilla `scan` command)");

            MUSIC_VOLUME = file.Bind("Audio", "MusicVolume", 100f, "What volume the music plays at. Should be between 0 and 100");
            MUSIC_PLAYS_OUTSIDE = file.Bind("Audio", "MusicPlaysOutside", true, "Does the music play outside the facility?");
            SCREEN_SHAKE = file.Bind("Visuals", "ScreenShake", true, "Whether or not to shake the screen during the meltdown sequence.");
            PARTICLE_EFFECTS = file.Bind("Visuals", "ParticleEffects", true, "Should meltdown sequence contain particle effects? Doesn't include particle effects on the fireball.");

            MeltdownPlugin.logger.LogInfo("Checking for any mod settings managers...");
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("ainavt.lc.lethalconfig")) {
                InitLethalConfig();
            }
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.willis.lc.lethalsettings")) {
                InitLethalSettings();
            }
        }
        public static void RequestSync() {
            if (!IsClient) return;

            FastBufferWriter stream = new FastBufferWriter(IntSize, Allocator.Temp);
            MessageManager.SendNamedMessage("FacilityMeltdown_OnRequestConfigSync", 0uL, stream);
        }

        public static void OnRequestSync(ulong clientId, FastBufferReader _) {
            if (!IsHost) return;

            MeltdownPlugin.logger.LogInfo($"Config sync request received from client: {clientId}");

            byte[] array = SerializeToBytes(Instance);
            int value = array.Length;

            FastBufferWriter stream = new FastBufferWriter(value + IntSize, Allocator.Temp);

            try {
                stream.WriteValueSafe(in value, default);
                stream.WriteBytesSafe(array);

                MessageManager.SendNamedMessage("FacilityMeltdown_OnReceiveConfigSync", clientId, stream);
            } catch (Exception e) {
                MeltdownPlugin.logger.LogInfo($"Error occurred syncing config with client: {clientId}\n{e}");
            }
        }

        public static void OnReceiveSync(ulong _, FastBufferReader reader) {
            if (!reader.TryBeginRead(IntSize)) {
                MeltdownPlugin.logger.LogError("Config sync error: Could not begin reading buffer.");
                return;
            }

            reader.ReadValueSafe(out int val, default);
            if (!reader.TryBeginRead(val)) {
                MeltdownPlugin.logger.LogError("Config sync error: Host could not sync.");
                return;
            }

            byte[] data = new byte[val];
            reader.ReadBytesSafe(ref data, val);

            SyncInstance(data);

            if (MeltdownPlugin.modVersion != Instance.MOD_VERSION) {
                HUDManager.Instance.AddTextToChatOnServer("FacilityMeltdown versions do not match! Please make sure all clients are running the latest version. The ability to play together on mismatched versions will be removed in later versions of FacilityMeltdown!");
            } else {
                MeltdownPlugin.logger.LogInfo("Successfully synced config with host.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal void InitLethalConfig() { // i really do not like this syntax :sob:
            MeltdownPlugin.logger.LogInfo("Setting up LethalConfig settings");

            LethalConfigManager.SetModDescription("Maybe taking the appartus isn't such a great idea...");

            LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(OVERRIDE_APPARATUS_VALUE, true));
            LethalConfigManager.AddConfigItem(new IntSliderConfigItem(
                APPARATUS_VALUE,
                new IntSliderOptions {
                    Min = 80,
                    Max = 500,
                    RequiresRestart = true
                }
            ));
            LethalConfigManager.AddConfigItem(new IntSliderConfigItem(
                MONSTER_SPAWN_AMOUNT,
                new IntSliderOptions {
                    Min = 0,
                    Max = 10,
                    RequiresRestart = true
                }
            ));

            LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(EMERGENCY_LIGHTS, true));

            LethalConfigManager.AddConfigItem(new FloatSliderConfigItem(
                SCAN_COOLDOWN,
                new FloatSliderOptions {
                    Min = 0,
                    Max = 30,
                    RequiresRestart = true
                }
            ));
            LethalConfigManager.AddConfigItem(new FloatStepSliderConfigItem(
                SCAN_ACCURACY,
                new FloatStepSliderOptions {
                    Min = 0,
                    Step = 1,
                    Max = 50,
                    RequiresRestart = true
                }
            ));

            LethalConfigManager.AddConfigItem(new IntSliderConfigItem(
                MELTDOWN_TIME,
                new IntSliderOptions {
                    Min = 0,
                    Max = 5 * 60,
                    RequiresRestart = true
                }
            ));

            LethalConfigManager.AddConfigItem(new FloatStepSliderConfigItem(
                MUSIC_VOLUME,
                new FloatStepSliderOptions() {
                    Min = 0,
                    Max = 100,
                    Step = 1,
                    RequiresRestart = false
                }
            ));
            LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(MUSIC_PLAYS_OUTSIDE, false));

            LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(SCREEN_SHAKE, false));
            LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(PARTICLE_EFFECTS, false));
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal void InitLethalSettings() {
            SliderComponent appratusValueSlider = new SliderComponent {
                Value = APPARATUS_VALUE.Value,
                MinValue = 80,
                MaxValue = 500,
                WholeNumbers = true,
                Text = "Appartus Value",
                Enabled = OVERRIDE_APPARATUS_VALUE.Value,
                OnValueChanged = (self, value) => { APPARATUS_VALUE.Value = (int)value; Default.APPARATUS_VALUE = (int)value; }
            };

            VerticalComponent editableInGame = new VerticalComponent {
                Children = new MenuComponent[] {
                    new LabelComponent {
                        Text = "Audio Settings [Client Side]"
                    },
                    new SliderComponent {
                        Value = MUSIC_VOLUME.Value,
                        MinValue = 0,
                        MaxValue = 100,
                        WholeNumbers = true,
                        Text = "Music Volume",
                        OnValueChanged = (self, value) => MUSIC_VOLUME.Value = (int) value
                    },
                    new ToggleComponent {
                        Text = "Play Music Outside?",
                        Value = MUSIC_PLAYS_OUTSIDE.Value,
                        OnValueChanged = (self, value) => MUSIC_PLAYS_OUTSIDE.Value = value
                    },
                    new LabelComponent {
                        Text = "Visual Settings [Client Side]"
                    },
                    new ToggleComponent {
                        Text = "Screen Shake",
                        Value = SCREEN_SHAKE.Value,
                        OnValueChanged = (self, value) => SCREEN_SHAKE.Value = value
                    },
                    new ToggleComponent {
                        Text = "Particle Effects",
                        Value = PARTICLE_EFFECTS.Value,
                        OnValueChanged = (self, value) => PARTICLE_EFFECTS.Value = value
                    },
                }
            };

            ModMenu.RegisterMod(new ModMenu.ModSettingsConfig {
                Name = MeltdownPlugin.modName,
                Id = MeltdownPlugin.modGUID,
                Version = MeltdownPlugin.modVersion,
                Description = "Maybe taking the appartus isn't such a great idea...",
                
                MenuComponents = new MenuComponent[] {
                    new LabelComponent {
                        Text = "Game Balance Settings [Synced]"
                    },
                    new ToggleComponent {
                        Text = "Override Appartus Value?",
                        Value = OVERRIDE_APPARATUS_VALUE.Value,
                        OnValueChanged = (self, value) => {
                            OVERRIDE_APPARATUS_VALUE.Value = value;
                            OVERRIDE_APPARATUS_VALUE = value;
                            appratusValueSlider.Enabled = value;
                        }
                    },
                    appratusValueSlider,
                    new SliderComponent {
                        Value = MONSTER_SPAWN_AMOUNT.Value,
                        MinValue = 0,
                        MaxValue = 10,
                        WholeNumbers = true,
                        Text = "Monster Spawn Amount",
                        OnValueChanged = (self, value) => { MONSTER_SPAWN_AMOUNT.Value = (int)value; Default.MONSTER_SPAWN_AMOUNT = (int)value; }
                    },
                    new ToggleComponent {
                        Text = "Facility has Emergency Lights?",
                        Value = EMERGENCY_LIGHTS.Value,
                        OnValueChanged = (self, value) => {
                            EMERGENCY_LIGHTS.Value = value;
                            EMERGENCY_LIGHTS = value;
                        }
                    },
                    new SliderComponent {
                        Value = APPARATUS_VALUE.Value,
                        MinValue = 0,
                        MaxValue = 10 * 60,
                        WholeNumbers = true,
                        Text = "Meltdown Sequence Time [NOT SUPPORTED, EDIT AT YOUR OWN RISK, NOT RECOMMENDED]",
                        Enabled = OVERRIDE_APPARATUS_VALUE.Value,
                        OnValueChanged = (self, value) => { MELTDOWN_TIME.Value = (int)value; Default.MELTDOWN_TIME = (int)value; }
                    },
                    new LabelComponent { Text = "Edit what enemies can spawn in the config file."},
                    editableInGame
                }
            });

            ModMenu.RegisterMod(new ModMenu.ModSettingsConfig {
                Name = MeltdownPlugin.modName,
                Id = MeltdownPlugin.modGUID,
                Version = MeltdownPlugin.modVersion,
                Description = "Maybe taking the appartus isn't such a great idea... (GameSettings are hidden in game)",
                MenuComponents = new MenuComponent[] {
                    editableInGame
                }
            }, false, true);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.ConnectClientToPlayerObject))]
        public static void InitializeLocalPlayer() {
            if (IsHost) {
                MessageManager.RegisterNamedMessageHandler("FacilityMeltdown_OnRequestConfigSync", OnRequestSync);
                Synced = true;

                return;
            }

            Synced = false;
            MessageManager.RegisterNamedMessageHandler("FacilityMeltdown_OnReceiveConfigSync", OnReceiveSync);
            RequestSync();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameNetworkManager), "StartDisconnect")]
        public static void PlayerLeave() {
            RevertSync();
        }
    }
}
