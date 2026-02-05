namespace Umbraco.Cms.Core.PropertyEditors.Validation;

/// <summary>
///     Represents a validation error with its JSON path location.
/// </summary>
internal sealed class JsonPathValidationError
{
    /// <summary>
    ///     Gets the JSON path where the validation error occurred.
    /// </summary>
    public required string JsonPath { get; init; }

    /// <summary>
    ///     Gets the collection of error messages for this validation error.
    /// </summary>
    public required IEnumerable<string> ErrorMessages { get; init; }
}
