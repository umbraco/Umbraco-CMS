using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.Validators;

public class MultipleValueValidator : IValueValidator
{
    public IEnumerable<ValidationResult> Validate(object? value, string? valueType, object? dataTypeConfiguration, PropertyValidationContext validationContext)
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

        if (value is not IEnumerable<string> values)
        {
            yield break;
        }

        foreach (var selectedValue in values)
        {
            if (valueListConfiguration.Items.Contains(selectedValue) is false)
            {
                yield return new ValidationResult(
                    $"The value {selectedValue} is not a part of the pre-values", ["items"]);
            }
        }
    }
}
