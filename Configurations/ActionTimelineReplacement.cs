namespace ActionTimelineReplacement.Configurations;

public class ActionTimelineReplacement(
    ushort animationStart,
    ushort animationEnd,
    ushort actionTimelineHit,
    ushort castVfx)
{
    public ushort AnimationStart = animationStart;
    public ushort AnimationEnd = animationEnd;
    public ushort ActionTimelineHit = actionTimelineHit;
    public ushort CastVfx = castVfx;
}