using FacilityMeltdown.Lang;
using FacilityMeltdown.Util;
using LethalSettings.UI;
using LethalSettings.UI.Components;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;

namespace FacilityMeltdown.Integrations;
internal class LethalSettingsIntegration {
    public static bool Enabled { get; private set; } // If you want to check compatibility locally

    // This method will be called automatically if the compatible mod is loaded.
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    static void Initialize() {
        Enabled = true;

        SliderComponent appratusValueSlider = new SliderComponent {
            Value = MeltdownConfig.Default.APPARATUS_VALUE.Value,
            MinValue = 80,
            MaxValue = 500,
            WholeNumbers = true,
            Text = "Appartus Value",
            Enabled = MeltdownConfig.Default.OVERRIDE_APPARATUS_VALUE.Value,
            OnValueChanged = (self, value) => { MeltdownConfig.Default.APPARATUS_VALUE.Value = (int)value; }
        };

        VerticalComponent editableInGame = new() {
            Children = [
                    new LabelComponent {
                        Text = "Audio Settings [Client Side]"
                    },
                    new SliderComponent {
                        Value = MeltdownConfig.Default.MUSIC_VOLUME.Value,
                        MinValue = 0,
                        MaxValue = 100,
                        WholeNumbers = true,
                        Text = "Music Volume",
                        OnValueChanged = (self, value) => MeltdownConfig.Default.MUSIC_VOLUME.Value = (int) value
                    },
                    new ToggleComponent {
                        Text = "Play Music Outside?",
                        Value = MeltdownConfig.Default.MUSIC_PLAYS_OUTSIDE.Value,
                        OnValueChanged = (self, value) => MeltdownConfig.Default.MUSIC_PLAYS_OUTSIDE.Value = value
                    },
                    new LabelComponent {
                        Text = "Visual Settings [Client Side]"
                    },
                    new ToggleComponent {
                        Text = "Screen Shake",
                        Value = MeltdownConfig.Default.SCREEN_SHAKE.Value,
                        OnValueChanged = (self, value) => MeltdownConfig.Default.SCREEN_SHAKE.Value = value
                    },
                    new ToggleComponent {
                        Text = "Particle Effects",
                        Value = MeltdownConfig.Default.PARTICLE_EFFECTS.Value,
                        OnValueChanged = (self, value) => MeltdownConfig.Default.PARTICLE_EFFECTS.Value = value
                    },
                    new LabelComponent {
                        Text = "Language Settings [Client Side]",
                    },
                    new DropdownComponent {
                        Text = "Language",
                        Value = new TMP_Dropdown.OptionData(LangParser.languages[MeltdownConfig.Default.LANGUAGE.Value]),
                        Options = LangParser.languages.Values
                            .Select(language => new TMP_Dropdown.OptionData(language))
                            .ToList(),
                        OnValueChanged = (self, value) => {
                            // code absouletely shloinged from @willis
                            var language = LangParser.languages
                                .Where(x => x.Value == value.text)
                                .Select(x => x.Key)
                                .FirstOrDefault();
                            if(language == null) {
                                MeltdownPlugin.logger.LogError("Failed to get language! defaulting to english");
                                language = "en";
                            }
                            MeltdownConfig.Default.LANGUAGE.Value = language;
                            LangParser.SetLanguage(language);
                        }
                    }
                ]
        };

        ModMenu.RegisterMod(new ModMenu.ModSettingsConfig {
            Name = MeltdownPlugin.modName,
            Id = MeltdownPlugin.modGUID,
            Version = MeltdownPlugin.modVersion,
            Description = "Maybe taking the appartus isn't such a great idea...",

            MenuComponents = [
                    new LabelComponent {
                        Text = "Game Balance Settings [Synced]"
                    },
                    new ToggleComponent {
                        Text = "Override Appartus Value?",
                        Value = MeltdownConfig.Default.OVERRIDE_APPARATUS_VALUE.Value,
                        OnValueChanged = (self, value) => {
                            MeltdownConfig.Default.OVERRIDE_APPARATUS_VALUE.Value = value;
                            appratusValueSlider.Enabled = value;
                        }
                    },
                    appratusValueSlider,
                    new SliderComponent {
                        Value = MeltdownConfig.Default.MONSTER_SPAWN_AMOUNT.Value,
                        MinValue = 0,
                        MaxValue = 10,
                        WholeNumbers = true,
                        Text = "Monster Spawn Amount",
                        OnValueChanged = (self, value) => { MeltdownConfig.Default.MONSTER_SPAWN_AMOUNT.Value = (int)value; }
                    },
                    new ToggleComponent {
                        Text = "Facility has Emergency Lights?",
                        Value = MeltdownConfig.Default.EMERGENCY_LIGHTS.Value,
                        OnValueChanged = (self, value) => {
                            MeltdownConfig.Default.EMERGENCY_LIGHTS.Value = value;
                        }
                    },
                    new SliderComponent {
                        Value = MeltdownConfig.Default.APPARATUS_VALUE.Value,
                        MinValue = 0,
                        MaxValue = 10 * 60,
                        WholeNumbers = true,
                        Text = "Meltdown Sequence Time [NOT SUPPORTED, EDIT AT YOUR OWN RISK, NOT RECOMMENDED]",
                        Enabled = MeltdownConfig.Default.OVERRIDE_APPARATUS_VALUE.Value,
                        OnValueChanged = (self, value) => { MeltdownConfig.Default.MELTDOWN_TIME.Value = (int)value; }
                    },
                    new LabelComponent { Text = "Edit what enemies can spawn in the config file."},
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
}
