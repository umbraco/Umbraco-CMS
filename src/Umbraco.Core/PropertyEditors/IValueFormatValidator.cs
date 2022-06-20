using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Defines a value format validator.
/// </summary>
public interface IValueFormatValidator
{
    /// <summary>
    ///     Validates a value.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="valueType">The value type.</param>
    /// <param name="format">A format definition.</param>
    /// <returns>Validation results.</returns>
    /// <remarks>
    ///     <para>The <paramref name="format" /> is expected to be a valid regular expression.</para>
    ///     <para>This is used to validate values against the property type validation regular expression.</para>
    /// </remarks>
    IEnumerable<ValidationResult> ValidateFormat(object? value, string valueType, string format);
}
