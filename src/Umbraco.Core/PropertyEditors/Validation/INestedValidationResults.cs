using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Core.PropertyEditors.Validation;

/// <summary>
///     Represents a validation result that contains nested validation results.
/// </summary>
public interface INestedValidationResults
{
    /// <summary>
    ///     Gets the collection of nested validation results.
    /// </summary>
    IList<ValidationResult> ValidationResults { get; }
}
