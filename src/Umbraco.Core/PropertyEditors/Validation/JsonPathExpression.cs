using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.Validation;

public static class JsonPathExpression
{
    public static string MissingPropertyValue(string propertyAlias, string? culture, string? segment)
        => $"?(@.alias == '{propertyAlias}' && @.culture == {(culture.IsNullOrWhiteSpace() ? "null" : $"'{culture}'")} && @.segment == {(segment.IsNullOrWhiteSpace() ? "null" : $"'{segment}'")})";
}
