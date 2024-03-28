using FacilityMeltdown.Util;
using LethalConfig;
using LethalConfig.ConfigItems;
using LethalConfig.ConfigItems.Options;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace FacilityMeltdown.Integrations;
internal class LethalConfigIntergration {
    public static bool Enabled { get; private set; } // If you want to check compatibility locally

    // This method will be called automatically if the compatible mod is loaded.
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    static void Initialize() {
        Enabled = true;

        LethalConfigManager.SetModDescription("Maybe taking the appartus isn't such a great idea...");

        LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(MeltdownConfig.Default.OVERRIDE_APPARATUS_VALUE.Entry, true));
        LethalConfigManager.AddConfigItem(new IntSliderConfigItem(
            MeltdownConfig.Default.APPARATUS_VALUE.Entry,
            new IntSliderOptions {
                Min = 80,
                Max = 500,
                RequiresRestart = true
            }
        ));
        LethalConfigManager.AddConfigItem(new IntSliderConfigItem(
            MeltdownConfig.Default.MONSTER_SPAWN_AMOUNT.Entry,
            new IntSliderOptions {
                Min = 0,
                Max = 10,
                RequiresRestart = true
            }
        ));

        LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(MeltdownConfig.Default.EMERGENCY_LIGHTS.Entry, true));

        LethalConfigManager.AddConfigItem(new FloatSliderConfigItem(
            MeltdownConfig.Default.SCAN_COOLDOWN.Entry,
            new FloatSliderOptions {
                Min = 0,
                Max = 30,
                RequiresRestart = true
            }
        ));
        LethalConfigManager.AddConfigItem(new FloatStepSliderConfigItem(
            MeltdownConfig.Default.SCAN_ACCURACY.Entry,
            new FloatStepSliderOptions {
                Min = 0,
                Step = 1,
                Max = 50,
                RequiresRestart = true
            }
        ));

        LethalConfigManager.AddConfigItem(new IntSliderConfigItem(
            MeltdownConfig.Default.MELTDOWN_TIME.Entry,
            new IntSliderOptions {
                Min = 0,
                Max = 5 * 60,
                RequiresRestart = true
            }
        ));

        LethalConfigManager.AddConfigItem(new FloatStepSliderConfigItem(
            MeltdownConfig.Default.MUSIC_VOLUME,
            new FloatStepSliderOptions() {
                Min = 0,
                Max = 100,
                Step = 1,
                RequiresRestart = false
            }
        ));
        LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(MeltdownConfig.Default.MUSIC_PLAYS_OUTSIDE, false));

        LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(MeltdownConfig.Default.SCREEN_SHAKE, false));
        LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(MeltdownConfig.Default.PARTICLE_EFFECTS, false));

        LethalConfigManager.AddConfigItem(new TextInputFieldConfigItem(MeltdownConfig.Default.LANGUAGE, true));
    }
}
