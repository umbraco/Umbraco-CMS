using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Models.Validation;

namespace Umbraco.Cms.Core.PropertyEditors.Validation;

/// <summary>
/// A validator that operates on an already-typed value and configuration.
/// <remarks>
/// Used together with an <see cref="IValueValidator"/> runner that materializes the typed value: see
/// <see cref="TypedValidatorRunner{TValue,TConfiguration}"/> for value editors whose value is already typed, and
/// <see cref="TypedJsonValidatorRunner{TValue,TConfiguration}"/> for JSON based value editors, where the value is deserialized once before validation.
/// </remarks>
/// </summary>
/// <typeparam name="TValue">The type of the value consumed by the validator.</typeparam>
/// <typeparam name="TConfiguration">The type of the configuration consumed by validator.</typeparam>
public interface ITypedValidator<TValue, TConfiguration>
{
    /// <summary>
    ///     Validates the specified value against the configuration.
    /// </summary>
    /// <param name="value">The typed value to validate.</param>
    /// <param name="configuration">The data type configuration.</param>
    /// <param name="valueType">The value type.</param>
    /// <param name="validationContext">The property validation context.</param>
    /// <returns>A collection of validation results.</returns>
    IEnumerable<ValidationResult> Validate(
        TValue? value,
        TConfiguration? configuration,
        string? valueType,
        PropertyValidationContext validationContext);
}
