# Facility Meltdown 
The apparatus was a vital part of the upkeep of the facility's nuclear reactor - and you just took it out.

With the facility meltdown mod the appartus has a lot more risk (and reward) attached to it. When you take it out the entire facility spirals into a self destruction sequence! You'll have 2 minutes to escape and take off before you get vaporized.

FacilityMeltdown should also work correctly with most\* custom moons and interiors. If you are expereincing an issue one a particluar modded moon - I CAN NOT HELP. This is down to be not being able to fix every moon in my mod, instead those moons need to fix stuff - most likely other mods will be broken by that moon as well.

If you are strugling with playing the mod have a look at the bottom of this readme to find some useful tips and tricks. Or you can modify certain things in the config.

## Credits
Music and voice lines come from portal 2. I love portal 2.

## What this mod adds
- A 2 minute meltdown sequence where everything begins to go wrong inside the facility. After the 2 minutes is up the nuclear reactor blows up into a fireball that will engulf you if you aren't fast enough.
- A new piece of Equipment - the Geiger Counter! For just 90 credits you can be surverying the area for radiation in no time! Use it to find the appartus or other places of radioactive significance.
- A new terminal command - "reactor health"! When the facility's reactor is melting down you need information! The reactor health command is here to provide you with vital information on how much time left you have until the facility goes catastrophic. It also includes what the company thinks you should do at this moment.

## Custom Music
### with PizzaTowerEscapeMusic
FacilityMeltdown itself doesn't support custom music. However, this tutorial made by "platinumbin" on Discord shows you how to use the [PizzaTowerEscapeMusic](https://thunderstore.io/c/lethal-company/p/BGN/PizzaTowerEscapeMusic/) mod to add custom music.
Link to tutorial: https://www.youtube.com/watch?v=D5hKkSLlEhk

### with loaforcsSoundAPI
[loaforcsSoundAPI](https://thunderstore.io/c/lethal-company/p/loaforc/loaforcsSoundAPI/) is a mod I built specfically for the LR team. There is a template avaliable [here](https://github.com/LoafOrc/loaforcsSoundAPI-examples), otherwise read the [getting started wiki](https://github.com/LoafOrc/loaforcsSoundAPI/wiki/Making-a-simple-Sound%E2%80%90Pack).
**This tool is still in development and I recommened using PizzaTowerEscapeMusic for now**
However this comes with the added bonus of being able to use Meltdown's dynamic system of speeding up the warning voice sounds as the meltdown occurs.

## Config
All game balance options are synced (only the host needs to edit them, but it wont hurt to have everyone using the same config). Visual and Audio settings are independent of what is synced.

### OPTIONAL ingame config editors
[lethalconfig](https://thunderstore.io/c/lethal-company/p/AinaVT/LethalConfig/) or [lethalsettings](https://thunderstore.io/c/lethal-company/p/willis81808/LethalSettings/) will let you edit the settings in-game.

## Known Issues
There are currently no known issues. If you find one go to the [lethal company modding discord](https://discord.gg/lcmod) and I can try and take a look.

## For Modders
If you are a modder and you want to add custom effects in general or for your specific moon you can add a refernce to the dll and extend the MeltdownSequenceEffect class. Example:
```cs
internal class EmergencyLightsEffect : MeltdownSequenceEffect {
    public EmergencyLightsEffect() : base(MeltdownPlugin.modGUID, "EmergencyLights") {}

    public override IEnumerator Play(float timeLeftUntilMeltdown) {
        for (int i = 0; i < RoundManager.Instance.allPoweredLightsAnimators.Count; i++) {
            RoundManager.Instance.allPoweredLightsAnimators[i].SetBool("on", true);
        }

        yield return new WaitForSeconds(2f);

        for (int i = 0; i < RoundManager.Instance.allPoweredLightsAnimators.Count; i++) {
            RoundManager.Instance.allPoweredLightsAnimators[i].SetBool("on", false);
        }

        yield return new WaitForSeconds(5f);
        yield break;
    }
}
```
Then register it by creating an instance in your plugins Awake `new EmergencyLightsEffect()`.
### Available methods to override
- void Start -> Called the moment the appartice is taken out
- IEnumerator Play -> Called either once (if .IsOneShot is true) or repeatedly until the exposion happens, or the ship leaves
- IEnumerator Stop -> Called when the explosion happens or .Playing is set to False in .Play
- void Cleanup -> Called when the ship leaves
- bool IsEnabledOnThisMoon -> return true if this effect should play on this moon

### Tips and Tricks
- Make sure you plan your route before you take it. It's most likely you have time to double check you know where to go.
- Having someone at the ship and running the "reactor health" command can give vital information on how much time you have left.
- Pay attention to the inside of the facility! As the reactor inside of the facility begins to fail you'll notice more and more explosions happening. You'll also get another radiation warning at exactly halfway.
- Your ship is safe from the explosion! The ship will automatically take off as the explosion occurs. As long as you are inside the ship before it takes off you will be 100% safe.
- Lastly is to stay calm! It can be hard with all the enemies, noises and explosions but panicking is the easiest way to lose focus and forget where to go.