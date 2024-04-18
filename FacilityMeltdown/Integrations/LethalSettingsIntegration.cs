using BepInEx.Configuration;
using FacilityMeltdown.Lang;
using FacilityMeltdown.Util;
using FacilityMeltdown.Util.Config;
using LethalSettings.UI;
using LethalSettings.UI.Components;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using TMPro;

namespace FacilityMeltdown.Integrations;
internal class LethalSettingsIntegration {
    public static bool Enabled { get; private set; } // If you want to check compatibility locally

    // This method will be called automatically if the compatible mod is loaded.
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    static void Initialize() {
        Enabled = true;

        VerticalComponent editableInGame = BuildConfig(MeltdownPlugin.clientConfig);

        ModMenu.RegisterMod(new ModMenu.ModSettingsConfig {
            Name = MeltdownPlugin.modName,
            Id = MeltdownPlugin.modGUID,
            Version = MeltdownPlugin.modVersion,
            Description = "Maybe taking the appartus isn't such a great idea...",

            MenuComponents = [
                    BuildConfig(MeltdownPlugin.config),
                    editableInGame
                ]
        });

        ModMenu.RegisterMod(new ModMenu.ModSettingsConfig {
            Name = MeltdownPlugin.modName,
            Id = MeltdownPlugin.modGUID,
            Version = MeltdownPlugin.modVersion,
            Description = "Maybe taking the appartus isn't such a great idea... (GameSettings are hidden in game)",
            MenuComponents = [
                    editableInGame
                ]
        }, false, true);
    }

    static VerticalComponent BuildConfig<T>(LoafConfig<T> config) where T : LoafConfig<T> {
        bool defaultRequiresRestart = true;
        if(config.GetType().GetCustomAttribute<RequiresRestartAttribute>() != null)
            defaultRequiresRestart = config.GetType().GetCustomAttribute<RequiresRestartAttribute>().RequiresRestart;

        List<MenuComponent> children = [];
        string currentHeader = "Misc";

        foreach((PropertyInfo property, object uncastedEntry) in config.configEntries) {
            bool requiresRestart = defaultRequiresRestart;
            RequiresRestartAttribute propertyRequiresRestart = property.GetCustomAttribute<RequiresRestartAttribute>();
            if(propertyRequiresRestart != null)
                requiresRestart = propertyRequiresRestart.RequiresRestart;

            ConfigGroupAttribute headerAttribute = (ConfigGroupAttribute)property.GetCustomAttribute(typeof(ConfigGroupAttribute));
            if(headerAttribute != null) {
                currentHeader = headerAttribute.Group;
                children.Add(
                    new LabelComponent {
                        Text = currentHeader,
                    });
            } else if(currentHeader == "Misc") {
                children.Add(
                    new LabelComponent {
                        Text = currentHeader,
                    });
            }

            ConfigRangeAttribute rangeAttribute = property.GetCustomAttribute<ConfigRangeAttribute>();

            if(property.PropertyType == typeof(int)) {
                children.Add(
                    new SliderComponent {
                        Value = (int)property.GetValue(config),
                        WholeNumbers = true,
                        MinValue = rangeAttribute.Min,
                        MaxValue = rangeAttribute.Max,
                        Text = property.Name,
                        OnValueChanged = (self, value) => {
                            ((ConfigEntry<int>)uncastedEntry).Value = (int)value;
                            if(requiresRestart) return;
                            property.SetValue(config, value);
                        }
                    }
                );
            }
            if(property.PropertyType == typeof(float)) {
                children.Add(
                    new SliderComponent {
                        Value = (float)property.GetValue(config),
                        MinValue = rangeAttribute.Min,
                        MaxValue = rangeAttribute.Max,
                        Text = property.Name,
                        OnValueChanged = (self, value) => {
                            ((ConfigEntry<float>)uncastedEntry).Value = value;
                            if(requiresRestart) return;
                            property.SetValue(config, value);
                        }
                    }
                );
            }
            if(property.PropertyType == typeof(bool)) {
                children.Add(
                    new ToggleComponent {
                        Text = property.Name,
                        Value = (bool)property.GetValue(config),
                        OnValueChanged = (self, value) => {
                            ((ConfigEntry<bool>) uncastedEntry).Value = value;
                            if(requiresRestart) return;
                            property.SetValue(config, value);
                        }
                    }
                );
            }
            if(property.PropertyType == typeof(string)) {
                children.Add(
                    new InputComponent {
                        Placeholder = property.Name,
                        Value = (string)property.GetValue(config),
                        OnValueChanged = (self, value) => {
                            ((ConfigEntry<string>)uncastedEntry).Value = value;
                            if(requiresRestart) return;
                            property.SetValue(config, value);
                        }
                    }
                );
            }
        }
        
        return null;
    }
}
