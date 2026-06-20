using System;
using Dalamud.Plugin.Services;
using Lumina.Excel;
using Lumina.Text.ReadOnly;

namespace ActionTimelineReplacement.Hookers;

public sealed record ActionOffsetDefinition
{
    private readonly Func<short, ReadOnlyMemory<byte>> _lookup;
    public string Name { get; }
    public uint Offset { get; }
    public short Max { get; }

    private ActionOffsetDefinition(string name, uint offset, Func<short, ReadOnlyMemory<byte>> lookup, short max)
    {
        _lookup = lookup;
        Max = max;
        Name = name;
        Offset = offset;
    }

    private static ActionOffsetDefinition Create<T>(string name, uint offset, Func<T, ReadOnlySeString> lookup,
        string prefix, string extension) where T : struct, IExcelRow<T>
    {
        return new(name, offset, key =>
            {
                var str = lookup(Service.DataManager.GetExcelSheet<T>()[(ushort)key]);
                if (str.IsEmpty)
                {
                    return str;
                }

                return prefix + str + extension;
            },
            (short)(Service.DataManager.GetExcelSheet<T>().Count - 1));
    }

    public ReadOnlyMemory<byte> LookUp(short key)
    {
        if (key < 0 || key >= Max)
        {
            return new ReadOnlyMemory<byte>();
        }

        return _lookup(key);
    }

    public static ActionOffsetDefinition CastVfx { get; } = Create<Lumina.Excel.Sheets.ActionCastVFX>(
        "Cast Vfx", 10,
        static data => data.VFX.Value.Location, "vfx/common/eff/", ".avfx");

    public static ActionOffsetDefinition AnimationStart { get; } = Create<Lumina.Excel.Sheets.ActionCastTimeline>(
        "Start timeline", 36,
        static data => data.Name.Value.Key, "chara/action/", ".tmb");

    public static ActionOffsetDefinition AnimationEnd { get; } = Create<Lumina.Excel.Sheets.ActionTimeline>(
        "End timeline", 32,
        static data => data.Key, "chara/action/", ".tmb");

    public static ActionOffsetDefinition ActionTimelineHit { get; } = Create<Lumina.Excel.Sheets.ActionTimeline>(
        "Hit timeline", 12,
        static data => data.Key, "chara/action/", ".tmb");
}