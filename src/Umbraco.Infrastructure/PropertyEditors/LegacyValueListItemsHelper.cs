// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json;
using System.Text.Json.Nodes;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Converts the plain string <c>items</c> of a value list configuration to option objects holding a <c>value</c>
///     and a <c>label</c> that starts out as the value.
/// </summary>
[Obsolete("Plain string value list items are read only for configuration from before Umbraco 20. Scheduled for removal in Umbraco 22.")]
internal static class LegacyValueListItemsHelper
{
    private const string ItemsKey = "items";

    /// <summary>
    ///     Converts the plain string items in a configuration JSON string.
    /// </summary>
    /// <param name="configuration">The configuration JSON.</param>
    /// <returns>The converted configuration JSON, or the given configuration when there is nothing to convert.</returns>
    public static string? ConvertPlainStringItems(string? configuration)
    {
        if (string.IsNullOrWhiteSpace(configuration))
        {
            return configuration;
        }

        JsonObject? json;
        try
        {
            json = JsonNode.Parse(configuration) as JsonObject;
        }
        catch (JsonException)
        {
            return configuration;
        }

        return json is not null && TryConvertPlainStringItems(json) ? json.ToJsonString() : configuration;
    }

    /// <summary>
    ///     Converts the plain string items in a configuration object in place.
    /// </summary>
    /// <param name="configuration">The configuration object.</param>
    /// <returns><c>true</c> if any item was converted; otherwise <c>false</c>.</returns>
    public static bool TryConvertPlainStringItems(JsonObject configuration)
    {
        var itemsKey = configuration.Select(pair => pair.Key).FirstOrDefault(key => key.InvariantEquals(ItemsKey));
        if (itemsKey is null || configuration[itemsKey] is not JsonArray items)
        {
            return false;
        }

        var changed = false;
        for (var i = 0; i < items.Count; i++)
        {
            if (items[i] is JsonValue item && item.TryGetValue(out string? value))
            {
                items[i] = new JsonObject { ["value"] = value, ["label"] = value };
                changed = true;
            }
        }

        return changed;
    }
}
