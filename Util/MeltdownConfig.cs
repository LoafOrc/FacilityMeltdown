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
using RuntimeNetcodeRPCValidator;
using GameNetcodeStuff;
using HarmonyLib;

namespace FacilityMeltdown.Util {
    [Serializable]
    internal class MeltdownConfig : SyncedInstance<MeltdownConfig> {
        internal ConfigEntry<int> MONSTER_SPAWN_AMOUNT;
        internal ConfigEntry<int> APPARATUS_VALUE;

        internal ConfigEntry<bool> OVERRIDE_APPARATUS_VALUE;

        internal ConfigEntry<float> MUSIC_VOLUME;
        internal ConfigEntry<bool> SCREEN_SHAKE;
        internal ConfigEntry<bool> MUSIC_PLAYS_OUTSIDE;

        private string MOD_VERSION = MeltdownPlugin.modVersion;

        internal MeltdownConfig(ConfigFile file) {
            MeltdownPlugin.logger.LogInfo("Setting up config...");
            InitInstance(this);         

            OVERRIDE_APPARATUS_VALUE = file.Bind("GameBalance", "OverrideAppartusValue", true, "Whether or not FacilityMeltdown should override appartus value. Only use for compatibility reasons");
            APPARATUS_VALUE = file.Bind("GameBalance", "AppartusValue", 240, "What the value of the appartus should be set as IF override appartus value is `true`");
            MONSTER_SPAWN_AMOUNT = file.Bind("GameBalance", "MonsterSpawnAmount", 5, "How many monsters should spawn during the meltdown sequence? Set to 0 to disable.");

            MUSIC_VOLUME = file.Bind("Audio", "MusicVolume", 100f, "What volume the music plays at. Should be between 0 and 100");
            MUSIC_PLAYS_OUTSIDE = file.Bind("Audio", "MusicPlaysOutside", true, "Does the music play outside the facility?");
            SCREEN_SHAKE = file.Bind("Visuals", "ScreenShake", true, "Whether or not to shake the screen during the meltdown sequence.");
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

            LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(OVERRIDE_APPARATUS_VALUE, false));
            LethalConfigManager.AddConfigItem(new IntSliderConfigItem(
                APPARATUS_VALUE,
                new IntSliderOptions {
                    Min = 80,
                    Max = 500,
                    RequiresRestart = false
                }
            ));
            LethalConfigManager.AddConfigItem(new IntSliderConfigItem(
                MONSTER_SPAWN_AMOUNT,
                new IntSliderOptions {
                    Min = 0,
                    Max = 10,
                    RequiresRestart = false
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
                OnValueChanged = (self, value) => APPARATUS_VALUE.Value = (int) value
            };

            ModMenu.RegisterMod(new ModMenu.ModSettingsConfig {
                Name = MeltdownPlugin.modName,
                Id = MeltdownPlugin.modGUID,
                Version = MeltdownPlugin.modVersion,
                Description = "Maybe taking the appartus isn't such a great idea...",
                MenuComponents = new MenuComponent[] {
                    new LabelComponent {
                        Text = "Game Balance Settings"
                    },
                    new ToggleComponent {
                        Text = "Override Appartus Value?",
                        Value = OVERRIDE_APPARATUS_VALUE.Value,
                        OnValueChanged = (self, value) => {
                            OVERRIDE_APPARATUS_VALUE.Value = value;
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
                        OnValueChanged = (self, value) => MONSTER_SPAWN_AMOUNT.Value = (int) value
                    },
                    new LabelComponent {
                        Text = "Audio Settings"
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
                        Text = "Visual Settings"
                    },
                    new ToggleComponent {
                        Text = "Screen Shake",
                        Value = SCREEN_SHAKE.Value,
                        OnValueChanged = (self, value) => SCREEN_SHAKE.Value = value
                    },
                }
            });
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
