namespace ActionTimelineReplacement.Hookers;

public sealed record ActionOffsetDefinition
{
    public string Name { get; }
    public uint Offset { get; }

    private ActionOffsetDefinition(string name, uint offset)
    {
        Name = name;
        Offset = offset;
    }
    
    public static ActionOffsetDefinition CastVfx { get; } = new("Cast Vfx", 10);
    public static ActionOffsetDefinition AnimationStart { get; } = new("Start timeline", 36);
    public static ActionOffsetDefinition AnimationEnd { get; } = new("End timeline", 32);
    public static ActionOffsetDefinition ActionTimelineHit { get; } = new("Hit timeline", 12);
}