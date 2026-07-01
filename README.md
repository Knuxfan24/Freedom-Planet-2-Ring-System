# Freedom Planet 2 Ring System

A [BepInEx](https://github.com/bepinex/bepinex) mod for Freedom Planet 2 which adds a custom Power Ring item (using the [FP2Lib](https://github.com/Kuborros/FP2Lib) custom item system) that replaces the game's normal health system with a recreation of the Sonic series Ring system.

## Building

First off, ensure that your system has a modern version of [Visual Studio](https://visualstudio.microsoft.com/) installed alongside the `.NET Framework 3.5 development tools`, as well as [Unity 5.6.3](https://unity.com/releases/editor/whats-new/5.6.3#installs) and [FP2Lib](https://github.com/Kuborros/FP2Lib) (at least Version 0.5. The [Freedom Manager](https://github.com/Kuborros/FreedomManager) program should install this automatically if used.).

Open the solution file in Visual Studio then go to `Tools > Options` and select `Package Sources` under the `NuGet Package Manager` category. Then add a package source called `BepInEx` with the source url set to `https://nuget.bepinex.dev/v3/index.json`.

Next, go to the `Assemblies` category in the `Dependencies` for the project, then delete the `Assembly-CSharp` and `FP2Lib` references. Right click on the Assemblies category and click `Add Assembly Reference...`, then click `Browse...` and navigate to Freedom Planet 2's install directory. Open the `FP2_Data` directory, then the `Managed` directory and select the `Assembly-CSharp.dll` file. Click Add, then Browse again and navigate to the location that FP2Lib's DLL is installed to (likely `BepInEx\Plugins\lib`) and select the `FP2Lib.dll` file. Click Add, then click OK.

You should now be able to right click the solution and choose `Rebuild` to build the mod. Though it is recommended to change the build configuration from `Debug` to `Release`, as the debug build prints a lot of console messages that are useless to the average player.

## Installing

Navigate to `BepInEx/plugins` and create a new folder with whatever name you want. Then copy the `Freedom_Planet_2_Ring_System.dll` file from the build (`bin/Debug/net35` or `bin/Release/net35`) into it. If using Freedom Manager, you may also want to copy the included `modinfo.json` file to give the mod a proper entry in the manager, although this is not strictly required.

In addition, copy the `rings.assets` file from the root of this repo to the `mod_overrides` folder in the game's root.

# Configuration

This mod has four configurable options. To customise these, go to the `BepInEx/config` and open `K24_FP2_Rings.cfg` in a text editor after running the game with the mod installed once. The options are:

- Nerf Shields: This option prevents shields from having more than one hit point. Off by default.
- Start with Rings: This option starts the player with 10 Rings, as some stages (usually boss only ones), have none at all. On by default.
- Start with Rings at Checkpoint: An extension of the previous option, this gives the player 10 Rings when respawning at a Checkpoint. On by default.
- Enable Sonic HUD: This option replaces the HUD whenever the Power Ring item is equipped with a recreation of the Sonic 1 HUD. Off by default.

# Asset Credits

Ring Sprites and Power Ring Item Sprite - Sonic Mania

Super Rings - Custom Sprites by [GussPrint](https://www.spriters-resource.com/custom_edited/sonicthehedgehogcustoms/asset/80571/)

Sonic HUD - Sonic 1 and 2, ripped by [a Russian name that I have no hope of typing](https://www.spriters-resource.com/sega_genesis/sonicth1/asset/37424/) and [Flare](https://www.spriters-resource.com/sega_genesis/sonicth2/asset/85195/).