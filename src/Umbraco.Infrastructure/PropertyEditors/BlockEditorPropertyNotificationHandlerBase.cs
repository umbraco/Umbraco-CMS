using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

public abstract class BlockEditorPropertyNotificationHandlerBase<TBlockLayoutItem> : ComplexPropertyEditorContentNotificationHandler
    where TBlockLayoutItem : IBlockLayoutItem, new()
{
    private readonly ILogger<BlockEditorPropertyNotificationHandlerBase<TBlockLayoutItem>> _logger;
    private readonly List<string> _keysToReplace = new List<string>();

    protected BlockEditorPropertyNotificationHandlerBase(ILogger<BlockEditorPropertyNotificationHandlerBase<TBlockLayoutItem>> logger) => _logger = logger;

    protected override string FormatPropertyValue(string rawJson, bool onlyMissingKeys)
    {
        // the block editor doesn't ever have missing keys so when this is true there's nothing to process
        if (onlyMissingKeys)
        {
            return rawJson;
        }

        return ReplaceBlockEditorKeys(rawJson);
    }

    // internal for tests
    // the purpose of this method is to replace the content and settings keys throughout the JSON structure of a block editor value.
    // the challenge is nested block editor values, which must also have their keys replaced. this becomes particularly tricky because
    // other nested property values could also contain keys, which should *not* be replaced (i.e. a content picker value).
    internal string ReplaceBlockEditorKeys(string rawJson, Func<Guid, Guid>? createGuid = null)
    {
        // used so we can test nicely
        createGuid ??= _ => Guid.NewGuid();

        if (string.IsNullOrWhiteSpace(rawJson))
        {
            return rawJson;
        }

        JsonObject? jObject = ParseObject(rawJson);
        if (jObject == null)
        {
            return rawJson;
        }

        TraverseObject(jObject);

        var oldToNewKeys = new Dictionary<Guid, Guid>();

        rawJson = Regex.Replace(
            rawJson,
            @"\w{8}-\w{4}-\w{4}-\w{4}-\w{12}",
            match =>
            {
                if (_keysToReplace.Contains(match.Value) is false)
                {
                    return match.Value;
                }

                var oldKey = Guid.Parse(match.Value);
                if (oldToNewKeys.ContainsKey(oldKey) == false)
                {
                    oldToNewKeys[oldKey] = createGuid(oldKey);
                }

                return match.Value.Replace(match.Value, oldToNewKeys[oldKey].ToString("D"));
            });

        return rawJson;
    }

    private void TraverseProperty(JsonNode property)
    {
        if (property is JsonArray jArray)
        {
            foreach (JsonNode token in jArray.WhereNotNull())
            {
                TraverseToken(token);
            }
        }
        else
        {
            TraverseToken(property);
        }
    }

    private void TraverseToken(JsonNode token)
    {
        var obj = token as JsonObject;
        if (obj == null && token is JsonValue jsonValue && jsonValue.GetValueKind() is JsonValueKind.String)
        {
            var str = jsonValue.GetValue<string>();
            obj = ParseObject(str);
        }

        if (obj == null)
        {
            return;
        }

        TraverseObject(obj);
    }

    private void TraverseObject(JsonObject obj)
    {
        // we'll assume that the object is a data representation of a block based editor if it contains "contentData" and "settingsData".
        if (obj["contentData"] is JsonArray contentData && obj["settingsData"] is JsonArray settingsData)
        {
            ParseKeys(contentData, settingsData);
            return;
        }

        foreach (JsonNode property in obj.Select(n => n.Value).WhereNotNull())
        {
            TraverseProperty(property);
        }
    }

    private void ParseKeys(JsonArray contentData, JsonArray settingsData)
    {
        // grab all keys from the objects of contentData and settingsData
        var keys = contentData.Select(c => c?["key"])
            .Union(settingsData.Select(s => s?["key"]))
            .Select(keyToken => keyToken?.GetValue<string>().NullOrWhiteSpaceAsNull())
            .ToArray();

        // the following is solely for avoiding functionality wise breakage. we should consider removing it eventually, but for the time being it's harmless.
        foreach (var keyToReplace in keys)
        {
            if (Guid.TryParse(keyToReplace ?? string.Empty, out Guid _) is false)
            {
                throw new FormatException($"Could not parse a valid {nameof(Guid)} from the string: \"{keyToReplace}\"");
            }
        }

        _keysToReplace.AddRange(keys.WhereNotNull());

        foreach (JsonObject item in contentData.Union(settingsData).WhereNotNull().OfType<JsonObject>())
        {
            foreach (JsonNode property in item.Where(p => p.Key != "contentTypeKey" && p.Key != "key").Select(p => p.Value).WhereNotNull())
            {
                TraverseProperty(property);
            }
        }
    }

    private JsonObject? ParseObject(string json)
    {
        if (json.DetectIsJson() == false)
        {
            return null;
        }

        try
        {
            return JsonNode.Parse(json) as JsonObject;
        }
        catch (Exception)
        {
            // See issue https://github.com/umbraco/Umbraco-CMS/issues/10879
            // We are detecting JSON data by seeing if a string is surrounded by [] or {}
            // If people enter text like [PLACEHOLDER] JToken  parsing fails, it's safe to ignore though
            // Logging this just in case in the future we find values that are not safe to ignore
            _logger.LogWarning("The following value was recognized as JSON but could not be parsed: {Json}", json);
            return null;
        }
    }
}
