using System;
using System.Numerics;
using ActionTimelineReplacement.Helpers;
using ActionTimelineReplacement.Hookers;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
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
        Enabled = new BoolModel(enabled, "");
        BoolModel[] enables = [..enable, Enabled];

        AnimationStart = CreateOne(animationStart, ActionOffsetDefinition.AnimationStart, "Animation Start");
        AnimationEnd = CreateOne(animationEnd, ActionOffsetDefinition.AnimationEnd, "Animation End");
        ActionTimelineHit =
            CreateOne(actionTimelineHit, ActionOffsetDefinition.ActionTimelineHit, "Action Timeline Hit");
        CastVfx = CreateOne(castVfx, ActionOffsetDefinition.CastVfx, "Cast VFX");

        ActionOffsetModel CreateOne(ushort value, ActionOffsetDefinition definition, string name)
        {
            return new ActionOffsetModel(
                name,
                value,
                ActionOffsetAction.GetOrCreate(definition, actionId),
                priority, enables);
        }
    }

    /// <returns>True if the user clicked the remove button.</returns>
    public void Draw()
    {
        Enabled.Draw();

        ImGui.TableNextColumn();
        var action = Service.DataManager.GetExcelSheet<Lumina.Excel.Sheets.Action>().GetRow(_actionId);
        using var texture = Service.Texture.GetFromGameIcon(new GameIconLookup(action.Icon)).GetWrapOrEmpty();
        ImGui.Image(texture.Handle, Vector2.One * 36 * ImGuiHelpers.GlobalScale);
        ImGui.TableNextColumn();
        ImGui.Text($"#{_actionId:D5} {action.Name}");

        if (!_advancedMode.Value) return;

        ImGui.TableNextColumn();
        AnimationStart.Draw();
        ImGui.TableNextColumn();
        AnimationEnd.Draw();
        ImGui.TableNextColumn();
        ActionTimelineHit.Draw();
        ImGui.TableNextColumn();
        CastVfx.Draw();
    }

    public void Dispose()
    {
        AnimationStart.Dispose();
        AnimationEnd.Dispose();
        ActionTimelineHit.Dispose();
        CastVfx.Dispose();
    }
}