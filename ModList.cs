using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using ModLibrary;
using Mono.Cecil;

namespace ModManager
{
    /// <summary>
    /// Reads the plugins folder. Everything comes from the DLLs' attributes, so switched-off mods
    /// (which aren't loaded) are described as well as running ones.
    /// </summary>
    internal static class ModList
    {
        /// <summary>Every plugin, regular mods first, then libraries, each by name. Reads the disk on every call.</summary>
        public static List<Mod> Read()
        {
            var loaded = IL2CPPChainloader.Instance.Plugins.Keys.ToHashSet();
            var files = Directory.EnumerateFiles(Paths.PluginPath, "*.dll", SearchOption.AllDirectories)
                .Concat(Directory.EnumerateFiles(Paths.PluginPath, "*.dll" + Mod.DisabledSuffix, SearchOption.AllDirectories));

            var mods = files.SelectMany(ReadPlugins).ToList();
            foreach (var mod in mods)
            {
                mod.LoadedAtStart = loaded.Contains(mod.Guid);
                mod.LiveToggle = Mods.FindLiveToggle(mod.Guid);
            }

            return mods
                .OrderBy(mod => mod.IsLibrary)
                .ThenBy(mod => mod.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        static IEnumerable<Mod> ReadPlugins(string file)
        {
            var mods = new List<Mod>();
            try
            {
                // Decoding the DependencyFlags in [BepInDependency] means finding BepInEx.Core.dll.
                using var resolver = new DefaultAssemblyResolver();
                resolver.AddSearchDirectory(Paths.BepInExAssemblyDirectory);
                resolver.AddSearchDirectory(Paths.PluginPath);
                using var assembly = AssemblyDefinition.ReadAssembly(file, new ReaderParameters { AssemblyResolver = resolver });

                foreach (var type in assembly.MainModule.Types)
                {
                    if (ReadPlugin(type, file) is { } mod) mods.Add(mod);
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning($"Couldn't read {file}: {e.Message}");
            }

            // A switched-off file we can't read is still listed, so the player can switch it back on.
            if (mods.Count == 0 && file.EndsWith(Mod.DisabledSuffix))
                mods.Add(new Mod { Guid = file, Name = Path.GetFileName(file)[..^(".dll" + Mod.DisabledSuffix).Length], Version = "?", File = file });

            return mods;
        }

        static Mod ReadPlugin(TypeDefinition type, string file)
        {
            var plugin = type.CustomAttributes.FirstOrDefault(attribute => Is<BepInPlugin>(attribute));
            if (plugin == null) return null;

            var mod = new Mod
            {
                Guid = (string)plugin.ConstructorArguments[0].Value,
                Name = (string)plugin.ConstructorArguments[1].Value,
                Version = (string)plugin.ConstructorArguments[2].Value,
                File = file,
            };
            foreach (var attribute in type.CustomAttributes)
            {
                if (Is<BepInDependency>(attribute) && IsHardDependency(attribute))
                    mod.Requires.Add((string)attribute.ConstructorArguments[0].Value);
                else if (Is<ModDescriptionAttribute>(attribute))
                    mod.Description = (string)attribute.ConstructorArguments[0].Value;
                else if (Is<ModLibraryAttribute>(attribute))
                    mod.IsLibrary = true;
            }
            return mod;
        }

        static bool Is<T>(CustomAttribute attribute) => attribute.AttributeType.FullName == typeof(T).FullName;

        // [BepInDependency("guid")] and [BepInDependency("guid", "version range")] are hard requirements.
        // With DependencyFlags.SoftDependency it only means "use it if it's there".
        static bool IsHardDependency(CustomAttribute dependency)
        {
            var arguments = dependency.ConstructorArguments;
            if (arguments.Count < 2 || arguments[1].Type.Name != nameof(BepInDependency.DependencyFlags)) return true;
            var flags = (BepInDependency.DependencyFlags)Convert.ToInt32(arguments[1].Value);
            return flags.HasFlag(BepInDependency.DependencyFlags.HardDependency);
        }
    }
}
