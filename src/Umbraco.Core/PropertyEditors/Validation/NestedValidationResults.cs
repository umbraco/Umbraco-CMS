using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Core.PropertyEditors.Validation;

/// <summary>
///     Represents a validation result that contains nested validation results.
/// </summary>
public class NestedValidationResults : ValidationResult, INestedValidationResults
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="NestedValidationResults" /> class.
    /// </summary>
    public NestedValidationResults()
        : base(string.Empty)
    {
    }

    /// <inheritdoc />
    public IList<ValidationResult> ValidationResults { get; } = [];
}
