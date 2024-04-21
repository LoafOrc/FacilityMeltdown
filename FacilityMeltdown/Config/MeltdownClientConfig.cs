using BepInEx.Configuration;
using FacilityMeltdown.Lang;
using FacilityMeltdown.Util.Config;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace FacilityMeltdown.Config;
[RequiresRestart(false)] // most of these values don't need a restart
internal class MeltdownClientConfig : LoafConfig<MeltdownClientConfig> {
    [ConfigGroup("Audio")]
    [ConfigDesc("What volume the music plays at. Also controls the volume of the random voicelines that play")]
    [ConfigRange(0, 100)]
    public int MusicVolume { get; private set; } = 100;

    [ConfigDesc("Does the music play outside the facility?")]
    public bool MusicPlaysOutside { get; private set; } = true;

    [ConfigGroup("Visuals")]
    [ConfigDesc("Whether or not to shake the screen during the meltdown sequence. Doesn't control other places of screen shake in the game.")]
    public bool ScreenShake { get; private set; } = true;

    [ConfigDesc("Should meltdown sequence contain particle effects? Doesn't include particle effects on the fireball.")]
    public bool ParticleEffects { get; private set; } = true;

    [ConfigIgnore] // handle this ourself ;3
    [RequiresRestart]
    public string Language { get; private set; } = "en";

    internal MeltdownClientConfig(ConfigFile file) : base(file) {
        BindProperty<string>(
            typeof(MeltdownClientConfig).GetProperty("Language"), 
            "Language", 
            new ConfigDescription(
                "What language should FacilityMeltdown use? NOTE: This only affects facility meltdown and won't change the rest of the games langauge\n" +
                "Some Languages may also need FontPatcher(https://thunderstore.io/c/lethal-company/p/LeKAKiD/FontPatcher/)\n" +
                "Languages Available: " + string.Join(", ", LangParser.languages.Keys)
            )
        );
    }
}
 