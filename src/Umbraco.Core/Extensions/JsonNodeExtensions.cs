// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using System.Text.Json.Nodes;

namespace Umbraco.Extensions;

/// <summary>
///     Extension methods for <see cref="JsonNode" />.
/// </summary>
public static class JsonNodeExtensions
{
    /// <summary>
    ///     Reads the node as a <see cref="decimal" />, falling back to invariant-culture string parsing, and returning
    ///     <c>null</c> when the node is <c>null</c> or cannot be interpreted as a number.
    /// </summary>
    /// <param name="node">The node to read.</param>
    /// <returns>The decimal value, or <c>null</c> when the node is absent or not numeric.</returns>
    public static decimal? ToDecimal(this JsonNode? node)
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
}
