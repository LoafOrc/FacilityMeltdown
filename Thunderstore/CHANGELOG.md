### v2.5
- Reorganised the entire project. Mods like MeltdownChance, PizzaProbability and FacilityMeltdownPatch will most likely be broken by these changes.
- Lang files are no longer embedded and loaded at all times.
- Geiger Counter recieves a buff, the apparatice now emits more radiation over a longer distance.
- Outside facility lights will now also flash.
 - Creators of custom moons can add a MeltdownMoonMapper to customise what lights will flash.
- Temporarily removed the dependency on LethalLib.
- TerminalAPI is no longer a dependency, terminal commands are now completly handled by Meltdown.
- Fixed the lag spike at the start of meltdown
- Meltdown now depends on LobbyCompatibility. If you hate dependencies, too bad!
 - LobbyCompatibility doesn't change anything but the main menu, it will not be the cause of any issues.

TODO:
- Support LethalAchievements
- Require at least 2 people to pull the apparatus.
 - Make this a config option as well supporting both a min amount and a percentage amount.