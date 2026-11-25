// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json.Nodes;
using Umbraco.Cms.Core.Serialization;
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
///         UDIs. UDIs of other entity types are ignored, as are entries that can't be parsed.
///     </para>
/// </remarks>
public static class EntityPickerValueParser
{
    /// <summary>
    ///     Parses the keys of the entities of the given type referenced by a stored picker value.
    /// </summary>
    /// <param name="value">The stored value.</param>
    /// <param name="udiEntityType">The UDI entity type of the entities to read, for example <see cref="Constants.UdiEntityType.Document" />.</param>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    /// <returns>The referenced keys, in stored order.</returns>
    public static IReadOnlyList<Guid> ParseKeys(object? value, string udiEntityType, IJsonSerializer jsonSerializer)
    {
        if (value is JsonNode jsonNode)
        {
            return jsonSerializer.TryDeserialize(jsonNode, out Guid[]? nodeKeys) ? nodeKeys : [];
        }

        var stringValue = value as string ?? value?.ToString();
        if (string.IsNullOrWhiteSpace(stringValue))
        {
            return [];
        }

        if (stringValue.DetectIsJson())
        {
            return jsonSerializer.TryDeserialize(stringValue, out Guid[]? jsonKeys) ? jsonKeys : [];
        }

        return ParseKeyList(stringValue, udiEntityType);
    }

    /// <summary>
    ///     Parses the keys of the entities of the given type referenced by a single key, a single UDI, or a
    ///     comma-separated list of UDIs.
    /// </summary>
    /// <param name="value">The stored value.</param>
    /// <param name="udiEntityType">The UDI entity type of the entities to read.</param>
    /// <returns>The referenced keys, in stored order.</returns>
    internal static IReadOnlyList<Guid> ParseKeyList(string value, string udiEntityType)
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
