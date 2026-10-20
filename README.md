<div align="center">

# Mod Manager

An in-game mod list for Lawgivers II.

![Lawgivers II 0.17.1](https://img.shields.io/badge/Lawgivers_II-0.17.1-F28019?style=flat-square)
![BepInEx 6 IL2CPP](https://img.shields.io/badge/BepInEx-6_IL2CPP-4B5563?style=flat-square)
![License: MIT](https://img.shields.io/badge/license-MIT-6B7280?style=flat-square)

<img src="docs/mods-window.png" alt="The Mods window" width="760">

</div>

The Mod Manager adds a MODS button to the main menu and the pause menu. It lists every BepInEx plugin you have installed and lets you switch them on and off.

## What it does

Turning a mod off renames its DLL to `.dll.disabled`. BepInEx skips those, so nothing gets deleted, and turning the mod back on just renames it back. Mods that are switched off still show up in the list.

Most mods only change after you restart the game. The list marks those in orange. Mods that support the Mod Manager switch on and off immediately.

Mods can declare which other mods they need. When you turn one on, everything it needs gets turned on too. A mod that another active mod depends on can't be switched off, and its row tells you which mods depend on it. Libraries, mods that only exist for other mods to use, are tagged LIBRARY and listed at the bottom.

The Mod Manager itself and the [Mod Library](https://github.com/Mindus1/lawgivers2-mod-library) it's built on are always on. To get rid of them, delete their DLLs.

<img src="docs/main-menu.png" alt="The MODS button in the main menu" width="300" align="right">

## Installation

1. Install [BepInEx 6 for IL2CPP](https://builds.bepinex.dev/projects/bepinex_be) (x64, build 790 or newer) and start the game once so it can set itself up.
2. Download `ModManager.dll` from [Releases](../../releases) and `ModLibrary.dll` from the [Mod Library](https://github.com/Mindus1/lawgivers2-mod-library).
3. Put both into `Lawgivers II\BepInEx\plugins`.

The MODS button sits right below Settings. If it doesn't show up, `BepInEx\LogOutput.log` usually says why.

<br clear="right">

## For mod authors

Your mod shows up in the list without any changes. A few attributes give it a description and tell the Mod Manager what it needs:

```csharp
[BepInPlugin("you.lawgivers2.mymod", "My Mod", "1.0.0")]
[BepInDependency(ModLibrary.Plugin.Guid)]
[BepInDependency("someone.otherlib")]
[ModDescription("One sentence about what My Mod does.")]
public class Plugin : BasePlugin { ... }
```

`[BepInDependency]` is the normal BepInEx attribute. Hard dependencies count as requirements, soft ones are ignored. `[ModDescription]` (and `[ModLibrary]`, if your mod is a library) come from the Mod Library. The Mod Manager reads all of them straight from your DLL, so they show even while your mod is off.

If you want your mod to switch without a restart, reference `ModManager.dll` with `Private="false"` and register in `Load()`:

```csharp
[BepInDependency(ModManager.Plugin.Guid)]
public class Plugin : BasePlugin
{
    public override void Load()
    {
        Mods.Register(this, enabled =>
        {
            // Undo (false) or redo (true) whatever your mod changed in the running game.
        });
    }
}
```

Your Harmony patches stay active while the mod is switched off, so they need to check a flag of their own. For UI that matches the Mods window, use the [Mod Library](https://github.com/Mindus1/lawgivers2-mod-library).

## Building

You need the .NET SDK (6 or newer) and the game with BepInEx installed. The project compiles against the installed `ModLibrary.dll`, so build and install the Mod Library first.

```bash
dotnet build -c Release
```

The project expects the game in `D:\Games\Steam\steamapps\common\Lawgivers II`. Pass `-p:GameDir="..."` if yours is somewhere else. The build copies the DLL into `BepInEx\plugins`, so close the game first.

Pushing a version tag like `v1.2.0` builds the mod on a self-hosted runner and attaches the DLL to a GitHub release.

## License

[MIT](LICENSE)
