namespace Umbraco.Cms.Core.PropertyEditors.Validation;

internal class JsonPathValidationError
{
    public required string JsonPath { get; init; }

    public required IEnumerable<string> ErrorMessages { get; init; }
}
