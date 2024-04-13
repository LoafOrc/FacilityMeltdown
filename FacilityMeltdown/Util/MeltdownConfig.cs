using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using System.Runtime.CompilerServices;
using LethalSettings.UI;
using LethalSettings.UI.Components;
using Unity.Collections;
using Unity.Netcode;
using GameNetcodeStuff;
using HarmonyLib;
using System.Runtime.Serialization;
using CSync.Lib;
using CSync.Extensions;
using FacilityMeltdown.Lang;
using TMPro;

namespace FacilityMeltdown.Util {
    [DataContract]
    internal class MeltdownConfig : SyncedConfig<MeltdownConfig> {
        [DataMember]
        internal SyncedEntry<int> MONSTER_SPAWN_AMOUNT, APPARATUS_VALUE, MELTDOWN_TIME;
        [DataMember]
        internal SyncedEntry<bool> OVERRIDE_APPARATUS_VALUE, EMERGENCY_LIGHTS;
        [DataMember]
        internal SyncedEntry<float> SCAN_COOLDOWN, SCAN_ACCURACY;
        
        [DataMember]
        internal SyncedEntry<string> DISALLOWED_ENEMIES;

        internal ConfigEntry<float> MUSIC_VOLUME;
        internal ConfigEntry<bool> SCREEN_SHAKE, MUSIC_PLAYS_OUTSIDE, PARTICLE_EFFECTS;
        internal ConfigEntry<string> LANGUAGE;

        internal List<string> GetDisallowedEnemies() {
            return DISALLOWED_ENEMIES.Value.Split(',').ToList();
        }

        internal MeltdownConfig(ConfigFile file) : base(MeltdownPlugin.modGUID) {
            ConfigManager.Register(this);

            OVERRIDE_APPARATUS_VALUE = file.BindSyncedEntry("GameBalance", "OverrideAppartusValue", true, "Whether or not FacilityMeltdown should override appartus value. Only use for compatibility reasons");
            APPARATUS_VALUE = file.BindSyncedEntry("GameBalance", "AppartusValue", 240, "What the value of the appartus should be set as IF override appartus value is `true`");
            MONSTER_SPAWN_AMOUNT = file.BindSyncedEntry("GameBalance", "MonsterSpawnAmount", 5, "How many monsters should spawn during the meltdown sequence? Set to 0 to disable.");
            EMERGENCY_LIGHTS = file.BindSyncedEntry("GameBalance", "EmergencyLights", true, "Should the lights turn on periodically? Disabling this option makes them permanently off. (Matches Vanilla Behaviour)");

            DISALLOWED_ENEMIES = file.BindSyncedEntry("GameBalance", "DisallowedEnemies", "Centipede,Hoarding bug", "What enemies to exclude from spawning in the meltdown sequence. Comma seperated list. \"Should\" support modded entities");

            MELTDOWN_TIME = file.BindSyncedEntry("Unsupported", "MeltdownTime", 120, "ABSOLUETLY NOT SUPPORTED OR RECOMMENDED! Change the length of the meltdown sequence. If this breaks I am not fixing it, you have been warned.");

            SCAN_COOLDOWN = file.BindSyncedEntry("GameBalance", "ShipScannerCooldown", 15f, "How long until the ship's scanner can scan the reactor. (Doesn't affect the vanilla `scan` command)");
            SCAN_ACCURACY = file.BindSyncedEntry("GameBalance", "ShipScannerAccuracy", 10f, "How accurate is the ship's scanner when scanning the reactor. Higher values mean it is more uncertain, and lower values is more accurate. (Doesn't affect the vanilla `scan` command)");

            MUSIC_VOLUME = file.Bind("Audio", "MusicVolume", 100f, "What volume the music plays at. Should be between 0 and 100");
            MUSIC_PLAYS_OUTSIDE = file.Bind("Audio", "MusicPlaysOutside", true, "Does the music play outside the facility?");
            SCREEN_SHAKE = file.Bind("Visuals", "ScreenShake", true, "Whether or not to shake the screen during the meltdown sequence.");
            PARTICLE_EFFECTS = file.Bind("Visuals", "ParticleEffects", true, "Should meltdown sequence contain particle effects? Doesn't include particle effects on the fireball.");

            LANGUAGE = file.Bind(
                "Language",
                "ActiveLanguage",
                "en",
                "What language should FacilityMeltdown use? NOTE: This only affects facility meltdown and won't change the rest of the games langauge\nSome Languages may also need FontPatcher(https://thunderstore.io/c/lethal-company/p/LeKAKiD/FontPatcher/)\nLanguages Available: " +
                string.Join(", ", LangParser.languages.Keys)
                );
        }
    }
}
