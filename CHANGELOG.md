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