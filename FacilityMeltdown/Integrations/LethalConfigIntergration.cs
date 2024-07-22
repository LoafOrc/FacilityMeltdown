using BepInEx.Configuration;
using FacilityMeltdown.Util;
using FacilityMeltdown.Util.Config;
using LethalConfig;
using LethalConfig.ConfigItems;
using LethalConfig.ConfigItems.Options;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace FacilityMeltdown.Integrations;
class LethalConfigIntergration {
    public static bool Enabled { get; private set; } // If you want to check compatibility locally

    // This method will be called automatically if the compatible mod is loaded.
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    static void Initialize() {
        Enabled = true;

        LethalConfigManager.SetModDescription("Maybe taking the appartus isn't such a great idea...");

        HandleConfig(MeltdownPlugin.config);
        HandleConfig(MeltdownPlugin.clientConfig);
    }

    static void HandleConfig<T>(LoafConfig<T> config) where T : LoafConfig<T> {
        bool defaultRequiresRestart = true;
        if(config.GetType().GetCustomAttribute<RequiresRestartAttribute>() != null)
            defaultRequiresRestart = config.GetType().GetCustomAttribute<RequiresRestartAttribute>().RequiresRestart;
        foreach((PropertyInfo property, object uncastedEntry) in config.configEntries) {
            bool requiresRestart = defaultRequiresRestart;
            RequiresRestartAttribute propertyRequiresRestart = property.GetCustomAttribute<RequiresRestartAttribute>();
            if(propertyRequiresRestart != null)
                requiresRestart = propertyRequiresRestart.RequiresRestart;

            ConfigRangeAttribute rangeAttribute = property.GetCustomAttribute<ConfigRangeAttribute>();

            if(property.PropertyType == typeof(int)) {
                if(rangeAttribute != null) {
                    LethalConfigManager.AddConfigItem(new IntSliderConfigItem(
                        (ConfigEntry<int>)uncastedEntry,
                        new IntSliderOptions {
                            Min = (int)rangeAttribute.Min,
                            Max = (int)rangeAttribute.Max,
                            RequiresRestart = requiresRestart
                        }
                    ));
                    if(!requiresRestart) {
                        ((ConfigEntry<int>)uncastedEntry).SettingChanged += (obj, args) => {
                            property.SetValue(config, ((ConfigEntry<int>)uncastedEntry).Value);
                        };
                    }
                } else {
                    throw new NotImplementedException($"{property.Name}: config entry of type: int, must have a range attribute, because i was too lazy lmao");
                }
            }
            if(property.PropertyType == typeof(float)) {
                if(rangeAttribute != null) {
                    LethalConfigManager.AddConfigItem(new FloatSliderConfigItem(
                        (ConfigEntry<float>)uncastedEntry,
                        new FloatSliderOptions {
                            Min = rangeAttribute.Min,
                            Max = rangeAttribute.Max,
                            RequiresRestart = requiresRestart
                        }
                    ));
                    if(!requiresRestart) {
                        ((ConfigEntry<float>)uncastedEntry).SettingChanged += (obj, args) => {
                            property.SetValue(config, ((ConfigEntry<float>)uncastedEntry).Value);
                        };
                    }
                } else {
                    throw new NotImplementedException($"{property.Name}: config entry of type: float, must have a range attribute, because i was too lazy lmao");
                }
            }
            if(property.PropertyType == typeof(bool)) {
                LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(
                    (ConfigEntry<bool>)uncastedEntry,
                    requiresRestart
                ));
                if(!requiresRestart) {
                    ((ConfigEntry<bool>)uncastedEntry).SettingChanged += (obj, args) => {
                        property.SetValue(config, ((ConfigEntry<bool>)uncastedEntry).Value);
                    };
                }
            }
            if(property.PropertyType == typeof(string)) {
                LethalConfigManager.AddConfigItem(new TextInputFieldConfigItem((ConfigEntry<string>)uncastedEntry, requiresRestart));
                if(!requiresRestart) {
                    ((ConfigEntry<string>)uncastedEntry).SettingChanged += (obj, args) => {
                        property.SetValue(config, ((ConfigEntry<string>)uncastedEntry).Value);
                    };
                }
            }
        }
    }
}
