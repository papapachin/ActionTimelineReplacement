using System.Collections.Generic;
using System.IO;
using System.Linq;
using ActionTimelineReplacement.Hookers;
using Newtonsoft.Json;

namespace ActionTimelineReplacement.Configurations;

public class ActionTimelineReplacementSet(
    string name,
    Dictionary<uint, ActionTimelineReplacementConfig> replacements,
    bool enabled,
    int priority)
{
    public string Name = name;

    public Dictionary<uint, ActionTimelineReplacementConfig> Replacements { get; } = replacements;
    public bool Enabled = enabled;
    public int Priority = priority;

    public static ActionTimelineReplacementSet? Load(string jsonFile)
    {
        try
        {
            var configs =
                JsonConvert.DeserializeObject<Dictionary<uint, ActionTimelineReplacement>>(
                    File.ReadAllText(jsonFile));

            if (configs == null) return null;

            var replacements = configs.ToDictionary(
                i => i.Key,
                i =>  new ActionTimelineReplacementConfig(i.Value, true));

            return new ActionTimelineReplacementSet(Path.GetFileNameWithoutExtension(jsonFile), replacements, true, 0);
        }
        catch
        {
            return null;
        }
    }

    public bool Save(string jsonFile)
    {
        try
        {
            var dic = Replacements.ToDictionary(
                i => i.Key,
                i => i.Value.Replacement);

            File.WriteAllText(jsonFile, JsonConvert.SerializeObject(dic));
            return true;
        }
        catch
        {
            return false;
        }
    }
}

public class ActionTimelineReplacementConfig(ActionTimelineReplacement replacement, bool enabled)
{
    public bool Enabled = enabled;
    public ActionTimelineReplacement Replacement { get; } = replacement;
}

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
    public unsafe void WriteToPointer(ActionData* pointer)
    {
        pointer->CastVfx = CastVfx;
        pointer->AnimationStart = AnimationStart;
        pointer->AnimationEnd = AnimationEnd;
        pointer->ActionTimelineHit = ActionTimelineHit;
    }
}