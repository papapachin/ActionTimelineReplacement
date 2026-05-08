using System;
using System.Linq;
using ActionTimelineReplacement.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ActionTimelineReplacement.Serialization;

internal sealed class MainModelJsonConverter : JsonConverter<MainModel>
{
    public override void WriteJson(JsonWriter writer, MainModel? value, JsonSerializer serializer)
    {
        if (value is null)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteStartObject();

        writer.WritePropertyName("Version");
        writer.WriteValue(value.Version);

        writer.WritePropertyName("EnableReplacement");
        writer.WriteValue(value.EnableReplacement.Value);

        writer.WritePropertyName("AdvancedMode");
        writer.WriteValue(value.AdvancedMode.Value);

        writer.WritePropertyName("ActionTimelineReplacements");
        writer.WriteStartArray();
        foreach (var set in value.ActionTimelineReplacements)
        {
            WriteSet(writer, set);
        }
        writer.WriteEndArray();

        writer.WriteEndObject();
    }

    public override MainModel ReadJson(JsonReader reader, Type objectType, MainModel? existingValue,
        bool hasExistingValue, JsonSerializer serializer)
    {
        var jo = JObject.Load(reader);
        var model = new MainModel(
            (bool?)jo["EnableReplacement"] ?? true,
            (bool?)jo["AdvancedMode"] ?? false)
        {
            Version = (int?)jo["Version"] ?? 0,
        };

        if (jo["ActionTimelineReplacements"] is JArray sets)
        {
            foreach (var setJo in sets.OfType<JObject>())
            {
                model.ActionTimelineReplacements.Add(ReadSet(setJo, model.EnableReplacement, model.AdvancedMode));
            }
        }

        return model;
    }

    private static void WriteSet(JsonWriter writer, ActionTimelineReplacementSetModel set)
    {
        writer.WriteStartObject();

        writer.WritePropertyName("Name");
        writer.WriteValue(set.Name.Value);

        writer.WritePropertyName("Replacements");
        writer.WriteStartObject();
        foreach (var rep in set.Replacements)
        {
            writer.WritePropertyName(rep.ActionId.ToString());
            WriteReplacement(writer, rep);
        }
        writer.WriteEndObject();

        writer.WritePropertyName("Enabled");
        writer.WriteValue(set.Enabled.Value);

        writer.WritePropertyName("Priority");
        writer.WriteValue(set.Priority.Value);

        writer.WriteEndObject();
    }

    private static ActionTimelineReplacementSetModel ReadSet(JObject jo,
        BoolModel enableReplacement, BoolModel advancedMode)
    {
        var set = new ActionTimelineReplacementSetModel(
            (string?)jo["Name"] ?? "Unnamed",
            (bool?)jo["Enabled"] ?? true,
            (int?)jo["Priority"] ?? 0,
            enableReplacement, advancedMode);

        if (jo["Replacements"] is JObject reps)
        {
            foreach (var prop in reps.Properties())
            {
                if (uint.TryParse(prop.Name, out var id) && prop.Value is JObject repJo)
                {
                    ReadReplacement(repJo, id, set);
                }
            }
        }

        return set;
    }

    private static void WriteReplacement(JsonWriter writer, ActionTimelineReplacementModel rep)
    {
        writer.WriteStartObject();

        writer.WritePropertyName("Replacement");
        writer.WriteStartObject();
        writer.WritePropertyName("AnimationStart");
        writer.WriteValue(rep.AnimationStart.Value);
        writer.WritePropertyName("AnimationEnd");
        writer.WriteValue(rep.AnimationEnd.Value);
        writer.WritePropertyName("ActionTimelineHit");
        writer.WriteValue(rep.ActionTimelineHit.Value);
        writer.WritePropertyName("CastVfx");
        writer.WriteValue(rep.CastVfx.Value);
        writer.WriteEndObject();

        writer.WritePropertyName("Enabled");
        writer.WriteValue(rep.Enabled.Value);

        writer.WriteEndObject();
    }

    private static void ReadReplacement(JObject jo, uint actionId, ActionTimelineReplacementSetModel set)
    {
        var r = jo["Replacement"] as JObject ?? new JObject();
        set.CreateChild(actionId,
            (bool?)jo["Enabled"] ?? true,
            (ushort)((int?)r["AnimationStart"]    ?? 0),
            (ushort)((int?)r["AnimationEnd"]      ?? 0),
            (ushort)((int?)r["ActionTimelineHit"] ?? 0),
            (ushort)((int?)r["CastVfx"]           ?? 0));
    }
}
