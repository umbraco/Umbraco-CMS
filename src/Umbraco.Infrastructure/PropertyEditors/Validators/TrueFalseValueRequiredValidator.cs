using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors.Validators;

namespace Umbraco.Cms.Infrastructure.PropertyEditors.Validators;

/// <summary>
/// Custom validator for true/false (toggle) required validation.
/// </summary>
internal class TrueFalseValueRequiredValidator : RequiredValidator
{
    /// <inheritdoc/>
    public override IEnumerable<ValidationResult> ValidateRequired(object? value, string? valueType)
    {
        IEnumerable<ValidationResult> validationResults = base.ValidateRequired(value, valueType);

        if (value is null)
        {
            return validationResults;
        }

        if (value is bool valueAsBool && valueAsBool is false)
        {
            validationResults = validationResults.Append(new ValidationResult(Constants.Validation.ErrorMessages.Properties.Empty, ["value"]));
        }

        return validationResults;
    }
}
