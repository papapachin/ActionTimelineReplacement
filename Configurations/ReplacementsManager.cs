using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Lumina.Excel.Sheets;

namespace ActionTimelineReplacement.Configurations;

public static class ReplacementsManager
{
    private static Dictionary<uint, string>? _actionNames;

    private static readonly Dictionary<uint, ActionTimelineReplacement> OldValue = [];

    public static Dictionary<uint, string> ActionNames => _actionNames
        ??= Service.DataManager.GetExcelSheet<Action>()
            .Where(i => !string.IsNullOrEmpty(i.Name.ToString()))
            .ToDictionary(i => i.RowId, i => i.Name.ToString());

    public static IEnumerable<uint> AllActionIds =>
        Service.Config.ActionTimelineReplacements.SelectMany(i => i.Replacements.Keys);

    public static string GetName(uint id)
    {
        return ActionNames.GetValueOrDefault(id, "Unknown");
    }

    public static ActionTimelineReplacement GetReplacement(uint actionId)
        => GetConfigReplacement(actionId) ?? GetOriginalReplacement(actionId);

    private static ActionTimelineReplacement? GetConfigReplacement(uint actionId)
    {
        if (!Service.Config.EnableReplacement) return null;

        foreach (var replacement in Service.Config.ActionTimelineReplacements
                     .Where(replacement => replacement.Enabled)
                     .OrderByDescending(replacement => replacement.Priority))
        {
            if (!replacement.Replacements.TryGetValue(actionId, out var replacementValue)) continue;
            if (!replacementValue.Enabled) continue;

            return replacementValue.Replacement;
        }

        return null;
    }

    public static ActionTimelineReplacement GetOriginalReplacement(uint actionId)
    {
        ref var replacement = ref CollectionsMarshal.GetValueRefOrAddDefault(OldValue, actionId, out var exists);
        if (!exists)
        {
            var act = Service.DataManager.GetExcelSheet<Action>()?.GetRow(actionId);
            replacement = new ActionTimelineReplacement(
                (ushort)(act?.AnimationStart.RowId ?? 0),
                (ushort)(act?.AnimationEnd.RowId ?? 0),
                (ushort)(act?.ActionTimelineHit.RowId ?? 0),
                (ushort)(act?.VFX.RowId ?? 0));
        }

        return replacement!;
    }
}