using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Unity.IL2CPP;

namespace ModManager
{
    /// <summary>
    /// The Mod Manager's API for other mods.
    /// Every plugin is listed without it. Registering lets the player switch your mod without a restart.
    /// </summary>
    public static class Mods
    {
        static readonly Dictionary<string, Action<bool>> liveToggles = new();

        /// <summary>
        /// Call from your plugin's <c>Load()</c>. <paramref name="onToggle"/> gets <c>false</c> when the player
        /// switches your mod off and <c>true</c> when they switch it on again: undo or redo your changes to the game there.
        /// </summary>
        public static void Register(BasePlugin plugin, Action<bool> onToggle) =>
            liveToggles[MetadataHelper.GetMetadata(plugin).GUID] = onToggle;

        internal static Action<bool> FindLiveToggle(string guid) => liveToggles.GetValueOrDefault(guid);
    }
}
