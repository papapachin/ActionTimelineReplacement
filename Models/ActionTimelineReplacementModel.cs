using System;
using System.Numerics;
using ActionTimelineReplacement.Hookers;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility;

namespace ActionTimelineReplacement.Models;

public sealed class ActionTimelineReplacementModel : IDisposable
{
    private readonly BoolModel _advancedMode;
    public BoolModel Enabled { get; }
    public ActionOffsetModel AnimationStart { get; }
    public ActionOffsetModel AnimationEnd { get; }
    public ActionOffsetModel ActionTimelineHit { get; }
    public ActionOffsetModel CastVfx { get; }

    public uint ActionId { get; }

    public ActionTimelineReplacementModel(
        uint actionId,
        bool enabled,
        short animationStart,
        short animationEnd,
        short actionTimelineHit,
        short castVfx,
        IntModel priority,
        BoolModel advancedMode,
        Span<BoolModel> enable)
    {
        ActionId = actionId;
        _advancedMode = advancedMode;
        Enabled = new BoolModel(enabled, "");
        BoolModel[] enables = [..enable, Enabled];

        AnimationStart = CreateOne(animationStart, ActionOffsetDefinition.AnimationStart);
        AnimationEnd = CreateOne(animationEnd, ActionOffsetDefinition.AnimationEnd);
        ActionTimelineHit =
            CreateOne(actionTimelineHit, ActionOffsetDefinition.ActionTimelineHit);
        CastVfx = CreateOne(castVfx, ActionOffsetDefinition.CastVfx);

        ActionOffsetModel CreateOne(short value, ActionOffsetDefinition definition)
        {
            return new ActionOffsetModel(value,
                ActionOffsetAction.GetOrCreate(definition, actionId),
                priority, enables);
        }
    }

    /// <returns>True if the user clicked the remove button.</returns>
    public void Draw()
    {
        Enabled.Draw();

        ImGui.TableNextColumn();
        var action = Service.DataManager.GetExcelSheet<Lumina.Excel.Sheets.Action>().GetRow(ActionId);
        using var texture = Service.Texture.GetFromGameIcon(new GameIconLookup(action.Icon)).GetWrapOrEmpty();
        ImGui.Image(texture.Handle, Vector2.One * 36 * ImGuiHelpers.GlobalScale);
        ImGui.TableNextColumn();
        ImGui.Text($"#{ActionId:D5} {action.Name}");

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