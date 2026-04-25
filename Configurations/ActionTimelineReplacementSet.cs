using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace ActionTimelineReplacement.Configurations;

public class ActionTimelineReplacementSet(
    string name,
    Dictionary<uint, ActionTimelineReplacementConfig> replacements,
    bool enabled,
    int priority)
{
    public string Name = name;
    public bool Enabled = enabled;
    public int Priority = priority;
    public Dictionary<uint, ActionTimelineReplacementConfig> Replacements { get; } = replacements;

    public static ActionTimelineReplacementSet? Import(string jsonFile)
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

    public bool Export(string jsonFile)
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