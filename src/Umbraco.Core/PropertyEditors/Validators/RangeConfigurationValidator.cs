// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Umbraco.Cms.Core.Models.Validation;

namespace Umbraco.Cms.Core.PropertyEditors.Validators;

/// <summary>
///     Validates that a range configuration value does not have a maximum that is lower than its minimum.
/// </summary>
/// <remarks>
///     Works against the raw configuration value for a range field (a <see cref="NumberRange" /> or
///     <see cref="DecimalRange" />), whatever shape it arrives in.
/// </remarks>
public sealed class RangeConfigurationValidator : IValueValidator
{
    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(object? value, string? valueType, object? dataTypeConfiguration, PropertyValidationContext validationContext)
    {
        if (RangeConfigurationHelper.TryGetBounds(value, out decimal? min, out decimal? max)
            && min.HasValue
            && max.HasValue
            && max.Value < min.Value)
        {
            yield return new ValidationResult(
                $"The maximum value ({max.Value.ToString(CultureInfo.InvariantCulture)}) cannot be less than the minimum value ({min.Value.ToString(CultureInfo.InvariantCulture)}).",
                ["value"]);
        }
    }
}
