using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using ModLibrary;

namespace ModManager
{
    [BepInPlugin(Guid, "Mod Manager", "1.1.0")]
    [BepInDependency(ModLibrary.Plugin.Guid)]
    [ModDescription("Lists your mods and switches them on and off.")]
    public class Plugin : BasePlugin
    {
        /// <summary>Put this in <c>[BepInDependency(ModManager.Plugin.Guid)]</c> when your mod calls <see cref="Mods.Register"/>.</summary>
        public const string Guid = "mindus.lawgivers2.modmanager";

        internal static new ManualLogSource Log;

        public override void Load()
        {
            Log = base.Log;
            GameMenus.AddButton("Mods", () => GameText.Translate(texts => texts.Mods, "Mods"), ModsWindow.Open);
        }
    }
}
