using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Defines a required value validator.
/// </summary>
public interface IValueRequiredValidator
{
    /// <summary>
    ///     Validates a value.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="valueType">The value type.</param>
    /// <returns>Validation results.</returns>
    /// <remarks>
    ///     <para>This is used to validate values when the property type specifies that a value is required.</para>
    /// </remarks>
    IEnumerable<ValidationResult> ValidateRequired(object? value, string valueType);
}
