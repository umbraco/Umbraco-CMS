using System.ComponentModel.DataAnnotations;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.Validators;

/// <summary>
///     Used to validate if the value is a valid date/time
/// </summary>
public class DateTimeValidator : IValueValidator
{
    public IEnumerable<ValidationResult> Validate(object? value, string? valueType, object? dataTypeConfiguration)
    {
        // don't validate if empty
        if (value == null || value.ToString().IsNullOrWhiteSpace())
        {
            yield break;
        }

        if (DateTime.TryParse(value.ToString(), out DateTime dt) == false)
        {
            yield return new ValidationResult(
                string.Format("The string value {0} cannot be parsed into a DateTime", value),
                new[]
                {
                    // we only store a single value for this editor so the 'member' or 'field'
                    // we'll associate this error with will simply be called 'value'
                    "value",
                });
        }
    }
}
