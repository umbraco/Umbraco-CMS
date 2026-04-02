namespace Umbraco.Cms.Core.PropertyEditors.Validation;

/// <summary>
///     Represents a validation result that includes a JSON path for locating the error.
/// </summary>
public interface IJsonPathValidationResult
{
    /// <summary>
    ///     Gets the JSON path where the validation error occurred.
    /// </summary>
    string JsonPath { get; }
}
