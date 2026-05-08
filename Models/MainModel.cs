using System;
using System.Collections.Generic;
using System.Numerics;
using ActionTimelineReplacement.Interfaces;
using ActionTimelineReplacement.Serialization;
using Dalamud.Bindings.ImGui;
using Dalamud.Configuration;
using Dalamud.Interface.Colors;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Newtonsoft.Json;

namespace ActionTimelineReplacement.Models;

[JsonConverter(typeof(MainModelJsonConverter))]
public sealed class MainModel : IDrawItem, IDisposable, IPluginConfiguration
{
    private readonly FileDialogManager _dialogManager = new()
    {
        AddedWindowFlags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking,
    };

    private ActionTimelineReplacementSetModel? _activeSet;

    private static float Scale => ImGuiHelpers.GlobalScale;

    public int Version { get; set; } = 0;
    public BoolModel EnableReplacement { get; }
    public BoolModel AdvancedMode { get; }
    public List<ActionTimelineReplacementSetModel> ActionTimelineReplacements { get; } = [];

    public MainModel() : this(true, false) { }

    public MainModel(bool enableReplacement, bool advancedMode)
    {
        EnableReplacement = new BoolModel(enableReplacement, "Enable Replacement");
        AdvancedMode = new BoolModel(advancedMode, "Enable AdvancedMode");
    }

    internal void Save()
    {
        Service.Log.Info("Configuration saved.");
        Service.PluginInterface.SavePluginConfig(this);
    }

    public void Draw()
    {
        DrawHeader();

        using var table = ImRaii.Table("Main Table", 2, ImGuiTableFlags.Resizable);
        if (!table) return;

        ImGui.TableSetupColumn("Side Bar", ImGuiTableColumnFlags.WidthFixed, 100 * Scale);
        ImGui.TableNextColumn();

        try
        {
            using var style = ImRaii.PushStyle(ImGuiStyleVar.SelectableTextAlign, new Vector2(0.5f, 0.5f));
            DrawSideBar();
        }
        catch (Exception ex)
        {
            Service.Log.Warning(ex, "Something wrong with sideBar");
        }

        ImGui.TableNextColumn();

        try
        {
            DrawBody();
        }
        catch (Exception ex)
        {
            Service.Log.Warning(ex, "Something wrong with body");
        }

        _dialogManager.Draw();
    }

    private void DrawHeader()
    {
        EnableReplacement.Draw();
        ImGui.SameLine();
        AdvancedMode.Draw();
    }

    private void DrawBody()
    {
        using var child = ImRaii.Child("Body", -Vector2.One, false, ImGuiWindowFlags.NoScrollbar);
        if (!child) return;
        _activeSet?.Draw();
    }

    private void DrawSideBar()
    {
        using var windowPadding = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, Vector2.Zero);

        using var child = ImRaii.Child("Side bar", -Vector2.One, true, ImGuiWindowFlags.NoScrollbar);
        if (!child) return;

        var itemHeight = ImGui.CalcTextSize("C").Y + ImGui.GetStyle().FramePadding.Y * 2 +
                         ImGui.GetStyle().WindowPadding.Y;

        using (ImRaii.Child("Items",
                   new Vector2(-1,
                       ImGui.GetWindowSize().Y - ImGui.GetCursorPosY() - itemHeight - ImGui.GetStyle().WindowPadding.Y),
                   false))
        {
            var removed = new List<ActionTimelineReplacementSetModel>();
            foreach (var set in ActionTimelineReplacements)
            {
                var hashCode = set.GetHashCode();
                using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudGrey, !set.Enabled))
                {
                    if (ImGui.Selectable($"{set.Name.Value}##{hashCode}", _activeSet == set))
                    {
                        _activeSet = set;
                    }
                }

                var popUpId = $"Set{hashCode}PopUp";
                if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                {
                    ImGui.OpenPopup(popUpId);
                }

                using var popup = ImRaii.Popup(popUpId);
                if (popup)
                {
                    using var popUpChild = ImRaii.Child("PopUp" + popUpId,
                        new Vector2(80 * Scale, 60 * Scale),
                        true, ImGuiWindowFlags.NoScrollbar);

                    if (ImGui.Selectable("Export"))
                    {
                        _dialogManager.SaveFileDialog("Save", ".json", set.Name.Value, ".json", (b, file) =>
                        {
                            if (!b) return;
                            set.Export(file);
                        });
                    }

                    if (ImGui.Selectable("Delete"))
                    {
                        if (_activeSet == set) _activeSet = null;
                        removed.Add(set);
                        ImGui.CloseCurrentPopup();
                    }
                }
            }

            foreach (var set in removed)
            {
                set.Dispose();
                ActionTimelineReplacements.Remove(set);
            }

            if (removed.Count > 0) Save();
        }

        ImGui.SetCursorPosY(ImGui.GetWindowSize().Y - itemHeight);

        using (ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0)))
        {
            using var corner = ImRaii.PushStyle(ImGuiStyleVar.FrameRounding, 0);
            var width = Math.Max(ImGui.GetWindowWidth() / 2, 60 * Scale);
            var buttonSize = new Vector2(width, 0);

            ImGui.PushItemWidth(width);
            if (ImGui.Button("Create", buttonSize))
            {
                ActionTimelineReplacements.Add(
                    new ActionTimelineReplacementSetModel("New Item", true, 0, EnableReplacement, AdvancedMode));
                Save();
            }

            ImGui.SameLine();
            if (ImGui.Button("Import", buttonSize))
            {
                _dialogManager.OpenFileDialog("Import", ".json", (b, files) =>
                {
                    if (!b) return;
                    var added = false;
                    foreach (var file in files)
                    {
                        if (ActionTimelineReplacementSetModel.Import(file, EnableReplacement, AdvancedMode) is not { } set) continue;
                        ActionTimelineReplacements.Add(set);
                        added = true;
                    }

                    if (added) Save();
                }, 10, ".");
            }

            ImGui.PopItemWidth();
        }
    }

    public void Dispose()
    {
        foreach (var set in ActionTimelineReplacements)
        {
            set.Dispose();
        }
    }
}
