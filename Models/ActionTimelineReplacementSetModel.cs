using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using ActionTimelineReplacement.Helpers;
using ActionTimelineReplacement.Interfaces;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ActionTimelineReplacement.Models;

public sealed class ActionTimelineReplacementSetModel : IDrawItem, IDisposable
{
    private readonly BoolModel _enableReplacement;
    private readonly BoolModel _advancedMode;
    public StringModel Name { get; }
    public BoolModel Enabled { get; }
    public IntModel Priority { get; }

    public List<ActionTimelineReplacementModel> Replacements { get; } = [];

    public ActionTimelineReplacementSetModel(string name, bool enabled, int priority,
        BoolModel enableReplacement, BoolModel advancedMode)
    {
        _enableReplacement = enableReplacement;
        _advancedMode = advancedMode;
        Name = new StringModel(name, "Name");
        Enabled = new BoolModel(enabled, "Enabled");
        Priority = new IntModel(priority, "Priority");
    }

    internal void CreateChild(uint actionId, bool enabled,
        ushort animationStart, ushort animationEnd, ushort actionTimelineHit, ushort castVfx)
    {
        var child = new ActionTimelineReplacementModel(
            actionId, enabled,
            animationStart, animationEnd, actionTimelineHit, castVfx,
            Priority, _advancedMode, [_enableReplacement, Enabled]);
        Replacements.Add(child);
    }

    public void Draw()
    {
        Name.Draw();
        Enabled.Draw();
        ImGui.SameLine();
        Priority.Draw();
        ImGui.SameLine();
        DrawSearch();

        var advancedModeValue = _advancedMode.Value;

        using (ImRaii.Table("Tableb", advancedModeValue ? 7 : 3,
                   ImGuiTableFlags.SizingFixedSame | ImGuiTableFlags.Resizable))
        {
            ImGui.TableSetupColumn("Edit");
            ImGui.TableSetupColumn("Icon");
            ImGui.TableSetupColumn("Name");
            if (advancedModeValue)
            {
                ImGui.TableSetupColumn("Animation Start");
                ImGui.TableSetupColumn("Animation End");
                ImGui.TableSetupColumn("Action Timeline Hit");
                ImGui.TableSetupColumn("Cast VFX");
            }

            ImGui.TableHeadersRow();

            var removedItems = new List<ActionTimelineReplacementModel>();
            foreach (var actionTimelineReplacementModel in Replacements)
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                if (ImGui.Button(" - "))
                {
                    removedItems.Add(actionTimelineReplacementModel);
                }

                ImGui.SameLine();
                actionTimelineReplacementModel.Draw();
            }

            if (removedItems.Count > 0)
            {
                foreach (var actionTimelineReplacementModel in removedItems)
                {
                    actionTimelineReplacementModel.Dispose();
                    Replacements.Remove(actionTimelineReplacementModel);
                }

                Service.Model.Save();
            }
        }
    }

    private string _searchAction = string.Empty;
    private const string SearchActionsPopup = "Search actions";

    private void DrawSearch()
    {
        if (ImGui.Button(" + "))
        {
            ImGui.OpenPopup(SearchActionsPopup);
        }

        using var searchPopup = ImRaii.Popup(SearchActionsPopup);
        if (searchPopup)
        {
            var width = 200 * ImGuiHelpers.GlobalScale;

            ImGui.SetNextItemWidth(width);
            ImGui.InputText("##Search Action", ref _searchAction, 256);

            using var popUpChild = ImRaii.Child(SearchActionsPopup, new Vector2(width, width), true);
            foreach (var pair in ActionLookup.Names.OrderBy(i =>
                     {
                         if (string.IsNullOrEmpty(_searchAction)) return 0;
                         return Math.Min(ScoreString(i.Value, _searchAction),
                             ScoreString(i.Key.ToString(), _searchAction));
                     }))
            {
                if (ImGui.Selectable($"#{pair.Key:D5} {pair.Value}"))
                {
                    var (s, e, h, c) = ActionLookup.GetOriginal(pair.Key);
                    CreateChild(pair.Key, false, s, e, h, c);
                    Service.Model.Save();
                }
            }
        }
    }

    public static ActionTimelineReplacementSetModel? Import(string jsonFile,
        BoolModel enableReplacement, BoolModel advancedMode)
    {
        try
        {
            var dic = JsonConvert.DeserializeObject<Dictionary<uint, JObject>>(File.ReadAllText(jsonFile));
            if (dic is null) return null;

            var set = new ActionTimelineReplacementSetModel(
                Path.GetFileNameWithoutExtension(jsonFile), true, 0, enableReplacement, advancedMode);
            foreach (var (id, j) in dic)
            {
                set.CreateChild(id, true,
                    (ushort)((int?)j["AnimationStart"] ?? 0),
                    (ushort)((int?)j["AnimationEnd"] ?? 0),
                    (ushort)((int?)j["ActionTimelineHit"] ?? 0),
                    (ushort)((int?)j["CastVfx"] ?? 0));
            }

            return set;
        }
        catch
        {
            return null;
        }
    }

    public bool Export(string jsonFile)
    {
        try
        {
            var dic = Replacements.ToDictionary(
                r => r.ActionId,
                r => new
                {
                    AnimationStart = r.AnimationStart.Value,
                    AnimationEnd = r.AnimationEnd.Value,
                    ActionTimelineHit = r.ActionTimelineHit.Value,
                    CastVfx = r.CastVfx.Value,
                });
            File.WriteAllText(jsonFile, JsonConvert.SerializeObject(dic));
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static int ScoreString(string s1, string search)
    {
        if (s1.Contains(search, StringComparison.OrdinalIgnoreCase))
        {
            return s1.Length - search.Length;
        }

        return LevenshteinDistance(s1, search) + 20;
    }

    private static int LevenshteinDistance(string s1, string s2)
    {
        var len1 = s1.Length;
        var len2 = s2.Length;
        var dp = new int[len1 + 1, len2 + 1];

        for (var i = 0; i <= len1; i++)
            dp[i, 0] = i;
        for (var j = 0; j <= len2; j++)
            dp[0, j] = j;

        for (var i = 1; i <= len1; i++)
        {
            for (var j = 1; j <= len2; j++)
            {
                var cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
                dp[i, j] = Math.Min(Math.Min(
                        dp[i - 1, j] + 1,
                        dp[i, j - 1] + 1),
                    dp[i - 1, j - 1] + cost);
            }
        }

        return dp[len1, len2];
    }

    public void Dispose()
    {
        foreach (var actionTimelineReplacementModel in Replacements)
        {
            actionTimelineReplacementModel.Dispose();
        }
    }
}