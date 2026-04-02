using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.Validation;

/// <summary>
///     Provides helper methods for creating JSON path expressions used in validation error reporting.
/// </summary>
public static class JsonPathExpression
{
    /// <summary>
    ///     Creates a JSON path expression for a missing property value.
    /// </summary>
    /// <param name="propertyAlias">The property alias.</param>
    /// <param name="culture">The culture, or <c>null</c> for invariant.</param>
    /// <param name="segment">The segment, or <c>null</c> for no segment.</param>
    /// <returns>A JSON path filter expression.</returns>
    public static string MissingPropertyValue(string propertyAlias, string? culture, string? segment)
        => $"?(@.alias == '{propertyAlias}' && @.culture == {(culture.IsNullOrWhiteSpace() ? "null" : $"'{culture}'")} && @.segment == {(segment.IsNullOrWhiteSpace() ? "null" : $"'{segment}'")})";
}
