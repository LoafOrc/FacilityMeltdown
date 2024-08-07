﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using FacilityMeltdown.Lang;
using TMPro;
using FacilityMeltdown.Util.Config;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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

        [ConfigDesc("How many people need to be nearby to the apparatus for it to be collectable? Will go UP TO the value, e.g. if it's set to 3 and there are only 2 people in the lobby, everyone will need to be there.\nThis value is ignored and anybody can grab it from 9PM")]
        [ConfigRange(1, 10)]
        public int MinPeopleToPullApparatus { get; private set; } = 2;


        // todo: look into maybe a custom tomltypeconverter and make this cleaner?
        [ConfigDesc("What enemies to exclude from spawning in the meltdown sequence. Comma seperated list. Supports modded entities.")]
        public string DisallowedEnemies { get; private set; } = "Centipede,Hoarding bug";

        [ConfigDesc("How long until the ship's scanner can scan the reactor. (Doesn't affect the vanilla `scan` command)")]
        [ConfigRange(0, 20)]
        public float ShipScanCooldown { get; private set; } = 15f;

        [ConfigDesc("How accurate is the ship's scanner when scanning the reactor. Higher values mean it is more uncertain, and lower values is more accurate. (Doesn't affect the vanilla `scan` command)")]
        [ConfigRange(0, 100)]
        public float ShipScanAccuracy { get; private set; } = 15f;

        [ConfigGroup("Integrations")]
        [ConfigDesc("Should the apparatus value be multiplied by the current weather. Requires WeatherRegistry to work.")]
        public bool WeatherRegistryIntegration { get; private set; } = true;
        
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
