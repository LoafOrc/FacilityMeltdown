### v2.5
- Many, many, many, internal changes
- Decresead memory usage by correctly unloading assetbundle and not having all lang files loaded at all times.
- Geiger Counter now receives more radiation from the Apparatus
- Outside facility lights will now also flash
  - This could look a little bit silly, espically on modded moons.
   - Modded moon creators can reach out and I can show how to customise their lights better
- TerminalAPI is no longer needed, meltdown handles the terminal command itself
- Meltdown depends on LobbyCompatibility.
- Update to CSync v4.0.0
- Russian Translation (forgot to add this, woops), thanks Singularia!

### v2.5.1
- Slightly change Korean Transation
- Fix certain effects during the meltdown sequence to not correctly loop.