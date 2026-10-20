using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BepInEx;
using Lawgivers.UIElements;
using ModLibrary;
using TMPro;
using UnityEngine.UI;

namespace ModManager
{
    /// <summary>The Mods window: every installed mod with a switch.</summary>
    internal static class ModsWindow
    {
        const float Width = 1100;
        const float MaxListHeight = 800;
        const float ButtonWidth = 400;

        static GameUi.Window window;
        static GameUi.ScrollList list;
        static TextMeshProUGUI restartNote;
        static UIButton backButton;
        static List<Mod> mods;
        static readonly Dictionary<Mod, GameUi.SwitchRow> rows = new();

        public static void Open()
        {
            try
            {
                window ??= Build();
                window.Panel.Title.text = GameText.Translate(texts => texts.Mods, "Mods");
                backButton.Text.text = GameText.Translate(texts => texts.Back, "Back");
                window.Open(); // Before filling: rows can only be measured while visible.
                FillList();
            }
            catch (Exception e) { Plugin.Log.LogError(e); }
        }

        static GameUi.Window Build()
        {
            var newWindow = GameUi.CreateWindow("ModsWindow", "", Width);
            var body = newWindow.Panel.Body;
            body.GetComponent<VerticalLayoutGroup>().spacing = 20;

            list = GameUi.CreateScrollList(body, MaxListHeight);

            restartNote = GameUi.Text(body, "Restart the game to apply the changes in orange.", 24, GameUi.AccentColor);
            restartNote.alignment = TextAlignmentOptions.Center;

            var buttons = GameUi.CreateButtonBar(body);
            GameUi.CreateButton(buttons, "Open folder", ButtonWidth, () => Process.Start("explorer.exe", $"\"{Paths.PluginPath}\""));
            backButton = GameUi.CreateButton(buttons, "Back", ButtonWidth, () => window.Close());

            return newWindow;
        }

        static void FillList()
        {
            list.Clear();
            rows.Clear();
            mods = ModList.Read();

            foreach (var mod in mods)
            {
                var row = GameUi.CreateSwitchRow(list.Content, mod.Name, "", mod.Enabled, on => Switch(mod, on));
                row.Name.text = $"{mod.Name}  <size=70%>{GameUi.Colored("v" + mod.Version, GameUi.DetailsColor)}</size>";
                if (mod.IsLibrary) row.Name.text += $"  <size=60%>{GameUi.Colored("LIBRARY", GameUi.TagColor)}</size>";
                rows[mod] = row;
            }
            if (mods.Count == 0)
                GameUi.Text(list.Content, "No mods installed yet. Put them in BepInEx\\plugins.", 28, GameUi.DetailsColor).alignment = TextAlignmentOptions.Center;

            ShowStates();
            list.FitToContent();
        }

        static void Switch(Mod mod, bool on)
        {
            // Switching off is only possible when nothing needs the mod: otherwise its switch is locked.
            try
            {
                if (on) Requirements.SwitchOn(mod, mods);
                else mod.SetEnabled(false);
            }
            catch (Exception e) { Plugin.Log.LogError($"Couldn't switch {mod.Name} {(on ? "on" : "off")}: {e.Message}"); }

            ShowStates();
        }

        // One switch can change others (requirements, locks), so every row is refreshed.
        static void ShowStates()
        {
            foreach (var (mod, row) in rows)
            {
                var neededBy = Requirements.NeededBy(mod, mods);
                row.Show(mod.Enabled);
                row.SetLocked(mod.IsManager || neededBy.Count > 0);
                row.Details.text = Details(mod, neededBy);
                row.Details.gameObject.SetActive(true);
            }
            restartNote.gameObject.SetActive(mods.Any(mod => mod.NeedsRestart));
        }

        static string Details(Mod mod, List<Mod> neededBy)
        {
            var lines = new List<string>
            {
                string.IsNullOrEmpty(mod.Description) ? Path.GetFileName(mod.File).Replace(Mod.DisabledSuffix, "") : mod.Description,
            };

            if (mod.IsManager) lines.Add("Always on: it runs this window.");
            else if (neededBy.Count > 0) lines.Add($"Needed by {string.Join(", ", neededBy.Select(other => other.Name))}.");

            if (mod.Enabled)
            {
                foreach (var (guid, required) in Requirements.Unmet(mod, mods))
                {
                    var problem = required == null ? "isn't installed" : "is switched off";
                    lines.Add(GameUi.Colored($"Needs {required?.Name ?? guid}, which {problem}.", GameUi.AccentColor));
                }
            }
            if (mod.NeedsRestart)
                lines.Add(GameUi.Colored($"{(mod.Enabled ? "Turns on" : "Turns off")} after a restart.", GameUi.AccentColor));

            return string.Join("\n", lines);
        }
    }
}
