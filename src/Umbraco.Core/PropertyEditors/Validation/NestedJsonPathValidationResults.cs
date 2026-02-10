using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Core.PropertyEditors.Validation;

/// <summary>
///     Represents a validation result that contains nested validation results with a JSON path.
/// </summary>
public class NestedJsonPathValidationResults : ValidationResult, INestedValidationResults, IJsonPathValidationResult
{
    /// <inheritdoc />
    public string JsonPath { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="NestedJsonPathValidationResults" /> class.
    /// </summary>
    /// <param name="jsonPath">The JSON path where the validation error occurred.</param>
    public NestedJsonPathValidationResults(string jsonPath)
        : base(string.Empty)
        => JsonPath = jsonPath;

    /// <inheritdoc />
    public IList<ValidationResult> ValidationResults { get; } = [];
}
