## v2.2
- Added multi-language support
- Update config syncing to use [CSync](https://thunderstore.io/c/lethal-company/p/Owen3H/CSync/) to be not silly anymore
- Spanish is avaliable as a language, thanks to @moroxide on discord!
- Fixed an issue where meltdown sequence would ignore max count variable. Woops!

### v2.2.2
- im going to become the joker
- no i really am
- if lethallib breaks NetworkPrefabs one more time im actually going explode
- anyway:
- Fixed an issue where only the host had the meltdown happen

### v2.2.3
- While we try and pinpoint the issue this is a temporary patch to disable config syncing.

### v2.2.4
maybe building with the correct branch could actually help

### v2.2.5 and v2.2.6
- Re-enabled syncing
- Updated csync version to 1.0.7
- update readme

## v2.1
- Changed terminal dialogue again
- Staggered creation of effects to mitigate lagspike on meltdown start
- Hopefully fixed incompatibility with LCSoundTool by changing audio name
- Added link to github

### v2.1.1
- Reverted LCSoundTool fix because i dont have the full asset bundle

## v2.0
- Added a terminal command, "reactor health". This commands utilises the ship's onboard scanners to survey the reactor and how it's currently going. With that survery that you'll be given information about how long until the ractor goes catastrophic and what the company currently recommends you do. However the ship's onboard scanners need to use so much energy to perform the reactor health check that they need some time to cooldown before you can scan again.
- Made certain effects during the meltdown sequence speed up as it gets towards the reactor's explosion.
- Added a new equipment item, the Geiger Counter! The Geiger Counter points you to certain areas of radioactive actvity (mainly the appartus).
- Added credits for voicelines/music.
- Fixed several spelling mistakes.
- Added config options for the "reactor" command.
- Allowed music and other visual settings to be modified in game with LethalSettings
- Changed game balance options to signal a restart in LethalConfig
- Made the shockwave hide while inside of the facility
- Changed spawning mechanics to now disallow Snare fleas and Hoarding bugs from spawning becuase of the Appartus being pulled. (Changeable in config, this should also support modded enemies)

### v2.0.1
- Fix spelling mistakes
- Rewrite some terminal dialogue to better match the theme.
- Added tutorial on how to add custom music.
- Update Dependency versions

### v2.0.2
- Slight readme change

## v1.2
- Added particle effects to explosion fireball
- Added particle effects to the inside of the facility
- Fixed an issue where leaving before the explosion occured caused shockwaves to not be destroyed.
- *Hopefully* fixed modded moons not working.
- Mod is also much more resistant to errors occuring

### v1.2.1 Hotfix
- Fixed config syncing not working and producing an error longer than a highschool essay

### v1.2.2 "Patch" (May as well be a full blown update)
- Fixed explosion fireball particles not rendering correctly
- Did a *small, tiny* major rewrite of the meltdown handler
- Expose an API so other mods can add their own effects on custom modded moons.
- Change Meltdown Handler into a network object to make sure the meltdown sequence properly starts for all clients
- Fixed NPE when attempting to get effect origin for fireball
- Added LethalLib as a depedency (in this update just for networking, but used for more in later versions)
- Add 2 new settings:
 - Client sided:
  - Enable Particle Effects (NOTE: This does not disable the particle effects on the fireball. Only the ones inside the facility)
 - Host-sided/Synced:
  - Emergency Lights: Disabling emergency lights mimics vanilla in that lights will not turn back on for more visibility.
- Improved the intro dialogue to highlight the time remaining.

### v1.2.3
- Fixed players not dying when they were inside the facility as it explodes.