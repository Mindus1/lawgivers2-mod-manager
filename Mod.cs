using System;
using System.Collections.Generic;

namespace ModManager
{
    /// <summary>One plugin in the plugins folder, switched on or off.</summary>
    internal sealed class Mod
    {
        public const string DisabledSuffix = ".disabled";

        public string Guid;
        public string Name;
        public string Version;
        public string Description;
        public bool IsLibrary;
        /// <summary>GUIDs from the plugin's hard [BepInDependency]s.</summary>
        public List<string> Requires = new();
        /// <summary>The DLL as it is on disk right now: "X.dll" or "X.dll.disabled".</summary>
        public string File;
        public bool LoadedAtStart;
        /// <summary>Set for mods that called <see cref="Mods.Register"/>.</summary>
        public Action<bool> LiveToggle;

        public bool IsManager => Guid == Plugin.Guid;
        public bool Enabled => !File.EndsWith(DisabledSuffix, StringComparison.OrdinalIgnoreCase);

        /// <summary>Registered mods switch right away, all others when the game restarts.</summary>
        public bool NeedsRestart => LiveToggle == null && Enabled != LoadedAtStart;

        /// <summary>
        /// Renames the DLL. BepInEx only loads "*.dll", so a ".dll.disabled" file is skipped on the next start.
        /// Windows allows renaming a loaded DLL, so this works while the game runs.
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            if (enabled == Enabled) return;

            var newFile = enabled ? File[..^DisabledSuffix.Length] : File + DisabledSuffix;
            System.IO.File.Move(File, newFile);
            File = newFile;

            if (LiveToggle == null) return;
            try { LiveToggle(enabled); }
            catch (Exception e) { Plugin.Log.LogError($"{Name} failed to switch {(enabled ? "on" : "off")}: {e}"); }
        }
    }
}
