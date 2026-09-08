using System.Text.Json;
using System.Text.Json.Nodes;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_0_0.SingleBlockList;

/// <summary>
/// Verifies that the single blocks a converted value holds survive being re-serialized for persistence.
/// </summary>
/// <remarks>
/// Re-serializing runs through the containing block editor, which resolves the value editor of each nested property
/// itself. If it resolves the wrong one the nested value is replaced with null, and the outer value remains
/// perfectly valid JSON - so counting the conversions on both sides of that step is what makes the loss detectable
/// (see https://github.com/umbraco/Umbraco-CMS/issues/23596).
/// </remarks>
[Obsolete("Scheduled for removal in Umbraco 22.")] // Available in v17, activated in v18. Migration needs to work on LTS to LTS 17=>21
internal static class SingleBlockConversionVerifier
{
    /// <summary>
    /// Counts the <see cref="SingleBlockValue" /> instances held by a converted editor value, at any nesting depth.
    /// </summary>
    /// <param name="editorValue">The converted editor value.</param>
    /// <returns>The number of single block values.</returns>
    public static int CountSingleBlockValues(object? editorValue)
        => editorValue switch
        {
            RichTextEditorValue richTextEditorValue => CountSingleBlockValues(richTextEditorValue.Blocks),
            BlockValue blockValue => (blockValue is SingleBlockValue ? 1 : 0)
                                     + blockValue.ContentData
                                         .Concat(blockValue.SettingsData)
                                         .SelectMany(blockItemData => blockItemData.Values)
                                         .Sum(blockPropertyValue => CountSingleBlockValues(blockPropertyValue.Value)),
            _ => 0,
        };

    /// <summary>
    /// Counts the single block layouts present in a serialized property value, at any nesting depth.
    /// </summary>
    /// <param name="json">The serialized property value.</param>
    /// <returns>The number of single block layouts.</returns>
    /// <remarks>
    /// Nested block editor values are stored as JSON strings within their parent, so those are parsed and descended
    /// into as well.
    /// </remarks>
    public static int CountSingleBlockLayouts(string json)
    {
        JsonNode? node;
        try
        {
            node = JsonNode.Parse(json);
        }
        catch (JsonException)
        {
            return 0;
        }

        return CountSingleBlockLayouts(node);
    }

    private static int CountSingleBlockLayouts(JsonNode? node)
    {
        switch (node)
        {
            case JsonObject jsonObject:
                return CountSingleBlockLayouts(jsonObject);
            case JsonArray jsonArray:
                return jsonArray.Sum(CountSingleBlockLayouts);
            case JsonValue jsonValue when jsonValue.TryGetValue(out string? stringValue)
                                          && stringValue.DetectIsJson():
                return CountSingleBlockLayouts(stringValue);
            default:
                return 0;
        }
    }

    private static int CountSingleBlockLayouts(JsonObject jsonObject)
    {
        var count = 0;

        foreach (KeyValuePair<string, JsonNode?> property in jsonObject)
        {
            // Values written before the move to System.Text.Json carry Pascal cased property names, and
            // JsonBlockValueConverter reads both spellings.
            if (property.Key.Equals(nameof(BlockValue.Layout), StringComparison.OrdinalIgnoreCase)
                && property.Value is JsonObject layout
                && layout.ContainsKey(Constants.PropertyEditors.Aliases.SingleBlock))
            {
                count++;
            }

            // A counted layout is still descended into, which cannot double-count it: its children are arrays of
            // layout items, and a layout item never carries a "layout" key of its own.
            count += CountSingleBlockLayouts(property.Value);
        }

        return count;
    }
}
