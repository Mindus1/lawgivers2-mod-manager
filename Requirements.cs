using System.Collections.Generic;
using System.Linq;

namespace ModManager
{
    /// <summary>What needs what. A mod's requirements are its hard [BepInDependency]s.</summary>
    internal static class Requirements
    {
        /// <summary>The switched-on mods that can't run without <paramref name="mod"/>. While there are any, it's locked on.</summary>
        public static List<Mod> NeededBy(Mod mod, List<Mod> mods) =>
            mods.Where(other => other.Enabled && other.Requires.Contains(mod.Guid)).ToList();

        /// <summary>Requirements that aren't installed (Mod is null) or are switched off.</summary>
        public static IEnumerable<(string Guid, Mod Mod)> Unmet(Mod mod, List<Mod> mods) =>
            mod.Requires
                .Select(guid => (guid, mods.FirstOrDefault(other => other.Guid == guid)))
                .Where(requirement => requirement.Item2 is not { Enabled: true });

        /// <summary>Switches <paramref name="mod"/> on, after everything it needs, and everything those need.</summary>
        public static void SwitchOn(Mod mod, List<Mod> mods, HashSet<string> visited = null)
        {
            visited ??= new HashSet<string>();
            if (!visited.Add(mod.Guid)) return; // Two mods that need each other.

            foreach (var required in mods.Where(other => mod.Requires.Contains(other.Guid)))
                SwitchOn(required, mods, visited);
            mod.SetEnabled(true);
        }
    }
}
