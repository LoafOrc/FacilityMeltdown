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
            Value = MeltdownPlugin.config.APPARATUS_VALUE.LocalValue,
            MinValue = 80,
            MaxValue = 500,
            WholeNumbers = true,
            Text = "Appartus Value",
            Enabled = MeltdownPlugin.config.OVERRIDE_APPARATUS_VALUE.LocalValue,
            OnValueChanged = (self, value) => { MeltdownPlugin.config.APPARATUS_VALUE.LocalValue = (int)value; }
        };

        VerticalComponent editableInGame = new() {
            Children = [
                    new LabelComponent {
                        Text = "Audio Settings [Client Side]"
                    },
                    new SliderComponent {
                        Value = MeltdownPlugin.config.MUSIC_VOLUME.Value,
                        MinValue = 0,
                        MaxValue = 100,
                        WholeNumbers = true,
                        Text = "Music Volume",
                        OnValueChanged = (self, value) => MeltdownPlugin.config.MUSIC_VOLUME.Value = (int) value
                    },
                    new ToggleComponent {
                        Text = "Play Music Outside?",
                        Value = MeltdownPlugin.config.MUSIC_PLAYS_OUTSIDE.Value,
                        OnValueChanged = (self, value) => MeltdownPlugin.config.MUSIC_PLAYS_OUTSIDE.Value = value
                    },
                    new LabelComponent {
                        Text = "Visual Settings [Client Side]"
                    },
                    new ToggleComponent {
                        Text = "Screen Shake",
                        Value = MeltdownPlugin.config.SCREEN_SHAKE.Value,
                        OnValueChanged = (self, value) => MeltdownPlugin.config.SCREEN_SHAKE.Value = value
                    },
                    new ToggleComponent {
                        Text = "Particle Effects",
                        Value = MeltdownPlugin.config.PARTICLE_EFFECTS.Value,
                        OnValueChanged = (self, value) => MeltdownPlugin.config.PARTICLE_EFFECTS.Value = value
                    },
                    new LabelComponent {
                        Text = "Language Settings [Client Side]",
                    },
                    new DropdownComponent {
                        Text = "Language",
                        Value = new TMP_Dropdown.OptionData(LangParser.languages[MeltdownPlugin.config.LANGUAGE.Value]),
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
                            MeltdownPlugin.config.LANGUAGE.Value = language;
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
                        Value = MeltdownPlugin.config.OVERRIDE_APPARATUS_VALUE.Value,
                        OnValueChanged = (self, value) => {
                            MeltdownPlugin.config.OVERRIDE_APPARATUS_VALUE.LocalValue = value;
                            appratusValueSlider.Enabled = value;
                        }
                    },
                    appratusValueSlider,
                    new SliderComponent {
                        Value = MeltdownPlugin.config.MONSTER_SPAWN_AMOUNT.LocalValue,
                        MinValue = 0,
                        MaxValue = 10,
                        WholeNumbers = true,
                        Text = "Monster Spawn Amount",
                        OnValueChanged = (self, value) => { MeltdownPlugin.config.MONSTER_SPAWN_AMOUNT.LocalValue = (int)value; }
                    },
                    new ToggleComponent {
                        Text = "Facility has Emergency Lights?",
                        Value = MeltdownPlugin.config.EMERGENCY_LIGHTS.LocalValue,
                        OnValueChanged = (self, value) => {
                            MeltdownPlugin.config.EMERGENCY_LIGHTS.LocalValue = value;
                        }
                    },
                    new SliderComponent {
                        Value = MeltdownPlugin.config.APPARATUS_VALUE.LocalValue,
                        MinValue = 0,
                        MaxValue = 10 * 60,
                        WholeNumbers = true,
                        Text = "Meltdown Sequence Time [NOT SUPPORTED, EDIT AT YOUR OWN RISK, NOT RECOMMENDED]",
                        Enabled = MeltdownPlugin.config.OVERRIDE_APPARATUS_VALUE.LocalValue,
                        OnValueChanged = (self, value) => { MeltdownPlugin.config.MELTDOWN_TIME.LocalValue = (int)value; }
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
