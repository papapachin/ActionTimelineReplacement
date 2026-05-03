namespace ActionTimelineReplacement.Configurations;

public class ActionTimelineReplacementConfig(ActionTimelineReplacement replacement, bool enabled)
{
    public bool Enabled = enabled;
    public ActionTimelineReplacement Replacement { get; } = replacement;
}