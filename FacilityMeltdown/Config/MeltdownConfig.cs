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
using FacilityMeltdown.Util.Config;

namespace FacilityMeltdown.Config
{
    internal class MeltdownConfig(ConfigFile file) : LoafSyncedConfig<MeltdownConfig>(file)
    {

        [ConfigGroup("GameBalance")]
        [ConfigDesc("Whether or not FacilityMeltdown should override appartus value. Only disable for compatibility reasons.")]
        public bool OverrideApparatusValue { get; private set; } = true;

        [ConfigDesc("When overriding the apparatus value what should it's base value be?")]
        [ConfigRange(80, 500)]
        public int ApparatusValue { get; private set; } = 240;

        [ConfigDesc("How many monsters should spawn during the meltdown sequence? Set to 0 to disable.")]
        [ConfigRange(0, 10)]
        public int MonsterSpawnAmount { get; private set; } = 5;

        [ConfigDesc("Should the lights turn on periodically? Disabling this option makes them permanently off. (Matches Vanilla Behaviour)")]
        public bool EmergencyLights { get; private set; } = true;


        // todo: look into maybe a custom tomltypeconverter and make this cleaner?
        [ConfigDesc("What enemies to exclude from spawning in the meltdown sequence. Comma seperated list. Supports modded entities.")]
        public string DisallowedEnemies { get; private set; } = "Centipede,Hoarding bug";

        [ConfigDesc("How long until the ship's scanner can scan the reactor. (Doesn't affect the vanilla `scan` command)")]
        [ConfigRange(0, 20)]
        public float ShipScanCooldown { get; private set; } = 15f;

        [ConfigDesc("How accurate is the ship's scanner when scanning the reactor. Higher values mean it is more uncertain, and lower values is more accurate. (Doesn't affect the vanilla `scan` command)")]
        [ConfigRange(0, 100)]
        public float ShipScanAccuracy { get; private set; } = 15f;

        [ConfigGroup("UNSUPPORTED")]
        [ConfigDesc("ABSOLUETLY NOT SUPPORTED OR RECOMMENDED! Change the length of the meltdown sequence. If this breaks I am not fixing it, you have been warned.")]
        [ConfigRange(60, 5 * 60)]
        public int MeltdownTime { get; private set; } = 120;
        
        public List<string> GetDisallowedEnemies()
        {
            return DisallowedEnemies.Split(',').ToList();
        }
    }
}
