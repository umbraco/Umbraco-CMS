// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json;
using System.Text.Json.Nodes;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Reads the keys of the entities referenced by a stored picker value.
/// </summary>
/// <remarks>
///     <para>
///         A property's stored value can outlive the editor that wrote it: a property can move to a data type with
///         another editor, or a data type can change editor, without its stored values being rewritten. Pickers use this
///         parser so they read every format an entity picker has stored, not just the one they write themselves.
///     </para>
///     <para>
///         The accepted formats are a JSON array of keys, a single key, a single UDI, and a comma-separated list of
///         UDIs. Keys are read in any format <see cref="Guid.TryParse(string?, out Guid)" /> accepts. UDIs of other entity
///         types are ignored, as are entries that can't be parsed.
///     </para>
/// </remarks>
public static class EntityPickerValueParser
{
    /// <summary>
    ///     Parses the keys of the entities of the given type referenced by a stored picker value.
    /// </summary>
    /// <param name="value">The stored value.</param>
    /// <param name="udiEntityType">The UDI entity type of the entities to read, for example <see cref="Constants.UdiEntityType.Document" />.</param>
    /// <returns>The referenced keys, in stored order.</returns>
    public static IReadOnlyList<Guid> ParseKeys(object? value, string udiEntityType)
    {
        switch (value)
        {
            case JsonNode jsonNode:
                return ParseKeyArray(jsonNode.ToJsonString());
            case JsonElement jsonElement:
                return ParseKeyArray(jsonElement.GetRawText());
        }

        var stringValue = value as string ?? value?.ToString();
        if (string.IsNullOrWhiteSpace(stringValue))
        {
            return [];
        }

        return stringValue.DetectIsJson()
            ? ParseKeyArray(stringValue)
            : ParseKeyList(stringValue, udiEntityType);
    }

    // The array is read element by element, rather than deserialized as a whole, so a key written in any format, or
    // an entry that isn't a key, doesn't stop the rest from being read.
    private static IReadOnlyList<Guid> ParseKeyArray(string json)
    {
        JsonNode? node;
        try
        {
            node = JsonNode.Parse(json);
        }
        catch (JsonException)
        {
            return [];
        }

        if (node is not JsonArray array)
        {
            return [];
        }

        var keys = new List<Guid>(array.Count);
        foreach (JsonNode? item in array)
        {
            if (item is JsonValue itemValue
                && itemValue.TryGetValue(out string? entry)
                && Guid.TryParse(entry, out Guid key))
            {
                keys.Add(key);
            }
        }

        return keys;
    }

    private static IReadOnlyList<Guid> ParseKeyList(string value, string udiEntityType)
    {
        var keys = new List<Guid>();
        foreach (var entry in value.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (TryParseKey(entry, udiEntityType, out Guid key))
            {
                keys.Add(key);
            }
        }

        return keys;
    }

    private static bool TryParseKey(string entry, string udiEntityType, out Guid key)
    {
        if (Guid.TryParse(entry, out key))
        {
            return true;
        }

        if (UdiParser.TryParse(entry, out GuidUdi? udi) && udi.EntityType == udiEntityType)
        {
            key = udi.Guid;
            return true;
        }

        key = Guid.Empty;
        return false;
    }
}
