using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.Validators;

/// <summary>
///     A validator that validates that the value is a valid integer.
/// </summary>
public sealed class IntegerValidator : IValueValidator
{
    private readonly int? _minValue;
    private readonly int? _maxValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="IntegerValidator"/> class.
    /// </summary>
    public IntegerValidator()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IntegerValidator"/> class with minimum and/or maximum values.
    /// </summary>
    public IntegerValidator(int? minValue, int? maxValue)
    {
        _minValue = minValue;
        _maxValue = maxValue;
    }

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(object? value, string? valueType, object? dataTypeConfiguration, PropertyValidationContext validationContext)
    {
        if (value == null || value.ToString() == string.Empty)
        {
            yield break;
        }

        Attempt<int> result = value.TryConvertTo<int>();
        if (result.Success == false)
        {
            yield return new ValidationResult("The value " + value + " is not a valid integer", ["value"]);
        }
        else
        {
            if (_minValue.HasValue && result.Result < _minValue.Value)
            {
                yield return new ValidationResult("The value " + value + " is less than the minimum allowed value of " + _minValue.Value, ["value"]);
            }

            if (_maxValue.HasValue && result.Result > _maxValue.Value)
            {
                yield return new ValidationResult("The value " + value + " is greater than the maximum allowed value of " + _maxValue.Value, ["value"]);
            }
        }
    }
}
