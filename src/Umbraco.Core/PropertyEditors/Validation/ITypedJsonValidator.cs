using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Models.Validation;

namespace Umbraco.Cms.Core.PropertyEditors.Validation;

/// <summary>
/// A specific validator used for JSON based value editors, to avoid doing multiple deserialization.
/// <remarks>Is used together with <see cref="TypedJsonValidatorRunner{TValue,TConfiguration}"/></remarks>
/// </summary>
/// <typeparam name="TValue">The type of the value consumed by the validator.</typeparam>
/// <typeparam name="TConfiguration">The type of the configuration consumed by validator.</typeparam>
public interface ITypedJsonValidator<TValue, TConfiguration>
{
    /// <summary>
    ///     Validates the specified value against the configuration.
    /// </summary>
    /// <param name="value">The deserialized value to validate.</param>
    /// <param name="configuration">The data type configuration.</param>
    /// <param name="valueType">The value type.</param>
    /// <param name="validationContext">The property validation context.</param>
    /// <returns>A collection of validation results.</returns>
    public abstract IEnumerable<ValidationResult> Validate(
        TValue? value,
        TConfiguration? configuration,
        string? valueType,
        PropertyValidationContext validationContext);
}
