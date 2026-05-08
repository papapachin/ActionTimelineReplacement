using System;
using ActionTimelineReplacement.Helpers;
using ActionTimelineReplacement.Hookers;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;

namespace ActionTimelineReplacement.Models;

public sealed class ActionTimelineReplacementModel : IDisposable
{
    private readonly uint _actionId;
    private readonly BoolModel _advancedMode;
    public BoolModel Enabled { get; }
    public ActionOffsetModel AnimationStart { get; }
    public ActionOffsetModel AnimationEnd { get; }
    public ActionOffsetModel ActionTimelineHit { get; }
    public ActionOffsetModel CastVfx { get; }

    public uint ActionId => _actionId;

    public ActionTimelineReplacementModel(
        uint actionId,
        bool enabled,
        ushort animationStart,
        ushort animationEnd,
        ushort actionTimelineHit,
        ushort castVfx,
        IntModel priority,
        BoolModel advancedMode,
        Span<BoolModel> enable)
    {
        _actionId = actionId;
        _advancedMode = advancedMode;
        Enabled = new BoolModel(enabled, nameof(Enabled));
        BoolModel[] enables = [..enable, Enabled];

        AnimationStart    = CreateOne(animationStart,    ActionOffsetDefinition.AnimationStart);
        AnimationEnd      = CreateOne(animationEnd,      ActionOffsetDefinition.AnimationEnd);
        ActionTimelineHit = CreateOne(actionTimelineHit, ActionOffsetDefinition.ActionTimelineHit);
        CastVfx           = CreateOne(castVfx,           ActionOffsetDefinition.CastVfx);

        ActionOffsetModel CreateOne(ushort value, ActionOffsetDefinition definition)
        {
            return new ActionOffsetModel(
                value,
                ActionOffsetAction.GetOrCreate(definition, actionId),
                priority, enables);
        }
    }

    /// <returns>True if the user clicked the remove button.</returns>
    public bool Draw()
    {
        var result = false;
        Enabled.Draw();
        ImGui.SameLine();
        if (ImGui.Button(" - "))
        {
            result = true;
        }

        ImGui.SameLine();
        ImGui.Text($"#{_actionId:D5}");

        ImGui.SameLine();
        var advancedMode = _advancedMode.Value;
        using (ImRaii.TextWrapPos((60 * ImGuiHelpers.GlobalScale) + ImGui.GetCursorPosX(), advancedMode))
        {
            ImGui.TextWrapped(ActionLookup.GetName(_actionId));
        }
        if (!advancedMode) return result;

        ImGui.SameLine();
        AnimationStart.Draw();
        ImGui.SameLine();
        AnimationEnd.Draw();
        ImGui.SameLine();
        ActionTimelineHit.Draw();
        ImGui.SameLine();
        CastVfx.Draw();

        return result;
    }

    public void Dispose()
    {
        AnimationStart.Dispose();
        AnimationEnd.Dispose();
        ActionTimelineHit.Dispose();
        CastVfx.Dispose();
    }
}
