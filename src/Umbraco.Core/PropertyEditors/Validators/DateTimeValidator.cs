using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.Validators;

/// <summary>
///     Used to validate if the value is a valid date/time
/// </summary>
public class DateTimeValidator : IValueValidator
{
    /// <summary>
    ///    Validates if the value is a valid date/time
    /// </summary>
    /// <param name="value"></param>
    /// <param name="valueType"></param>
    /// <param name="dataTypeConfiguration"></param>
    /// <param name="validationContext"></param>
    /// <returns></returns>
    public IEnumerable<ValidationResult> Validate(object? value, string? valueType, object? dataTypeConfiguration, PropertyValidationContext validationContext)
    {
        // don't validate if empty
        if (value == null || value.ToString().IsNullOrWhiteSpace())
        {
            yield break;
        }

        if (DateTime.TryParse(value.ToString(), out DateTime dt) == false)
        {
            yield return new ValidationResult(
                $"The string value {value} cannot be parsed into a DateTime",
                new[]
                {
                    // we only store a single value for this editor so the 'member' or 'field'
                    // we'll associate this error with will simply be called 'value'
                    "value",
                });
        }
    }
}
