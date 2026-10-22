<div align="center">

# Mod Manager

An in-game mod list for Lawgivers II.

![Lawgivers II 0.17.2](https://img.shields.io/badge/Lawgivers_II-0.17.2-F28019?style=flat-square)
![BepInEx 6 IL2CPP](https://img.shields.io/badge/BepInEx-6_IL2CPP-4B5563?style=flat-square)
![License: MIT](https://img.shields.io/badge/license-MIT-6B7280?style=flat-square)

<img src="docs/mods-window.png" alt="The Mods window" width="760">

</div>

The Mod Manager adds a MODS button to the main menu and the pause menu. It lists every BepInEx plugin you have installed and lets you switch them on and off.

Nothing gets deleted: a mod that's off simply isn't loaded the next time you start the game, and mods that support it switch right away. When a mod needs other mods, they get switched on with it, and they can't be switched off while it's on.

## Installation

1. Install [BepInEx 6 for IL2CPP](https://builds.bepinex.dev/projects/bepinex_be) (x64, build 790 or newer) and start the game once so it can set itself up.
2. Download `ModManager.dll` from [Releases](../../releases) and `ModLibrary.dll` from the [Mod Library](https://github.com/Mindus1/lawgivers2-mod-library).
3. Put both into `Lawgivers II\BepInEx\plugins`.

The MODS button sits right below Settings, both in the main menu and in the pause menu. If it doesn't show up, `BepInEx\LogOutput.log` usually says why.

<p align="center">
  <img src="docs/main-menu.png" alt="The MODS button in the main menu" width="360">
</p>

## Documentation

The [wiki](../../wiki) explains [how switching works](../../wiki/How-it-works), including restarts, requirements and libraries, and [what mod authors can do](../../wiki/For-mod-authors) to describe their mod and let it switch without a restart.

## License

[MIT](LICENSE)
