// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Helper for reading the minimum and maximum bounds out of a range configuration value, regardless of the shape
///     it arrives in (a typed <see cref="NumberRange" />/<see cref="DecimalRange" />, a <see cref="JsonObject" />, a
///     <see cref="JsonElement" /> or a dictionary).
/// </summary>
public static class RangeConfigurationHelper
{
    /// <summary>
    ///     Attempts to read the minimum and maximum bounds from a range configuration value.
    /// </summary>
    /// <param name="value">The range configuration value.</param>
    /// <param name="min">The minimum bound, or <c>null</c> when unbounded or not present.</param>
    /// <param name="max">The maximum bound, or <c>null</c> when unbounded or not present.</param>
    /// <returns><c>true</c> when the value could be interpreted as a range; otherwise <c>false</c>.</returns>
    public static bool TryGetBounds(object? value, out decimal? min, out decimal? max)
    {
        min = null;
        max = null;

        switch (value)
        {
            case null:
                return false;
            case NumberRange numberRange:
                min = numberRange.Min;
                max = numberRange.Max;
                return true;
            case DecimalRange decimalRange:
                min = decimalRange.Min;
                max = decimalRange.Max;
                return true;
            case JsonObject jsonObject:
                min = ToDecimal(jsonObject["min"]);
                max = ToDecimal(jsonObject["max"]);
                return true;
            case JsonElement jsonElement when jsonElement.ValueKind == JsonValueKind.Object:
                min = GetDecimal(jsonElement, "min");
                max = GetDecimal(jsonElement, "max");
                return true;
            case IDictionary<string, object> dictionary:
                min = dictionary.TryGetValue("min", out var minObject) ? ToDecimal(minObject) : null;
                max = dictionary.TryGetValue("max", out var maxObject) ? ToDecimal(maxObject) : null;
                return true;
            default:
                return false;
        }
    }

    private static decimal? GetDecimal(JsonElement jsonElement, string key)
        => jsonElement.TryGetProperty(key, out JsonElement property) && property.ValueKind == JsonValueKind.Number && property.TryGetDecimal(out var value)
            ? value
            : null;

    private static decimal? ToDecimal(JsonNode? node)
    {
        if (node is null)
        {
            return null;
        }

        try
        {
            return node.GetValue<decimal>();
        }
        catch
        {
            return decimal.TryParse(node.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;
        }
    }

    private static decimal? ToDecimal(object? value)
        => value switch
        {
            null => null,
            decimal d => d,
            int i => i,
            long l => l,
            double db => (decimal)db,
            float f => (decimal)f,
            JsonNode node => ToDecimal(node),
            string s when decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) => parsed,
            _ => null,
        };
}
