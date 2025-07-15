using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.Validators;

[Obsolete("This is no longer used and has been migrated to an internal class within RadioButtonsPropertyEditor. Scheduled for removal in Umbraco 17.")]
public class RadioValueValidator : IValueValidator
{
    public IEnumerable<ValidationResult> Validate(object? value, string? valueType, object? dataTypeConfiguration,
        PropertyValidationContext validationContext)
    {
        // don't validate if empty
        if (value == null || value.ToString().IsNullOrWhiteSpace())
        {
            yield break;
        }

        if (dataTypeConfiguration is not ValueListConfiguration valueListConfiguration)
        {
            yield break;
        }

        if (value is not string valueAsString)
        {
            yield break;
        }

        if (valueListConfiguration.Items.Contains(valueAsString) is false)
        {
            yield return new ValidationResult(
                $"The value {valueAsString} is not a part of the pre-values", ["items"]);
        }
    }
}
