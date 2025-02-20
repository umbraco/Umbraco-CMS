using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.Validators;

/// <summary>
///     A validator that validates that the value is a valid integer
/// </summary>
public sealed class IntegerValidator : IValueValidator
{
    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(object? value, string? valueType, object? dataTypeConfiguration, PropertyValidationContext validationContext)
    {
        if (value != null && value.ToString() != string.Empty)
        {
            Attempt<int> result = value.TryConvertTo<int>();
            if (result.Success == false)
            {
                yield return new ValidationResult("The value " + value + " is not a valid integer", new[] { "value" });
            }
        }
    }
}
