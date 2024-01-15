# Facility Meltdown

[![GitHub Build Status](https://img.shields.io/github/actions/workflow/status/loaforc/facilitymeltdown/build.yml?style=for-the-badge&logo=github)](https://github.com/loaforc/facilitymeltdown/actions/workflows/build.yml)
[![Thunderstore Version](https://img.shields.io/thunderstore/v/LoafOrc/FacilityMeltdown?style=for-the-badge&logo=thunderstore&logoColor=white)](https://thunderstore.io/c/lethal-company/p/LoafOrc/FacilityMeltdown/)
[![Thunderstore Downloads](https://img.shields.io/thunderstore/dt/LoafOrc/FacilityMeltdown?style=for-the-badge&logo=thunderstore&logoColor=white)](https://thunderstore.io/c/lethal-company/p/LoafOrc/FacilityMeltdown/)

The apparatus was a vital part of the upkeep of the facility's nuclear reactor - and you just took it out.

With the facility meltdown mod the appartus has a lot more risk (and reward) attached to it. When you take it out the entire facility spirals into a self destruction sequence! You'll have 2 minutes to escape and take off before you get vaporized.

Take a look at the known isuses if you have a problem, otherwise join the [lethal company modding discord](https://discord.gg/lcmod) and find the FacilityMeltdown thread and report it there!

## Config
Only the host needs to change their config as it is synced.

### OPTIONAL ingame config editors
[lethalconfig](https://thunderstore.io/c/lethal-company/p/AinaVT/LethalConfig/) or [lethalsettings](https://thunderstore.io/c/lethal-company/p/willis81808/LethalSettings/) will let you edit the settings in-game.

## Known Issues
There has been previous reports of modded moons not playing well with this mod! As always go over to the [lethal company modding discord](https://discord.gg/lcmod) and I can try and take a look. There seems to be no problem with Lethal Level Loader or LECore so it could be map dependent. In which case I wil NOT be fixing it. I can not fix every modded moon in this mod otherwise I would get nothing done.

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
void Start -> Called the moment the appartice is taken out
IEnumerator Play -> Called either once (if .IsOneShot is true) or repeatedly until the exposion happens, or the ship leaves
IEnumerator Stop -> Called when the explosion happens or .Playing is set to False in .Play
void Cleanup -> Called when the ship leaves
bool IsEnabledOnThisMoon -> return true if this effect should play on this moon
